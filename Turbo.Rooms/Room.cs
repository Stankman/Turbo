﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Turbo.Core.Database.Entities.Room;
using Turbo.Core.Database.Factories.Rooms;
using Turbo.Core.Events;
using Turbo.Core.Game;
using Turbo.Core.Game.Players;
using Turbo.Core.Game.Rooms;
using Turbo.Core.Game.Rooms.Managers;
using Turbo.Core.Game.Rooms.Mapping;
using Turbo.Core.Game.Rooms.Object.Constants;
using Turbo.Core.Game.Rooms.Utils;
using Turbo.Core.Networking.Game.Clients;
using Turbo.Core.Packets.Messages;
using Turbo.Core.Storage;
using Turbo.Core.Utilities;
using Turbo.Events.Game.Rooms.Avatar;
using Turbo.Packets.Outgoing.Navigator;
using Turbo.Packets.Outgoing.Room.Engine;
using Turbo.Packets.Outgoing.Room.Layout;
using Turbo.Packets.Outgoing.Room.Session;
using Turbo.Rooms.Cycles;
using Turbo.Rooms.Managers;
using Turbo.Rooms.Mapping;

namespace Turbo.Rooms;

public class Room : Component, IRoom
{
    private readonly ITurboEventHub _eventHub;
    private readonly object _roomObserverLock = new();
    private readonly IList<ISession> _roomObservers = [];
    private int _remainingDisposeTicks = -1;

    public Room(
        ILogger<IRoom> logger,
        IRoomManager roomManager,
        RoomEntity roomEntity,
        IRoomSecurityFactory roomSecurityFactory,
        IRoomFurnitureFactory roomFurnitureFactory,
        IRoomUserFactory roomUserFactory,
        IRoomChatFactory roomChatFactory,
        ITurboEventHub eventHub,
        IStorageQueue _storageQueue)
    {
        RoomManager = roomManager;
        Logger = logger;
        RoomDetails = new RoomDetails(this, roomEntity, _storageQueue);

        RoomCycleManager = new RoomCycleManager(this);
        RoomSecurityManager = roomSecurityFactory.Create(this);
        RoomFurnitureManager = roomFurnitureFactory.Create(this);
        RoomUserManager = roomUserFactory.Create(this);
        RoomChatManager = roomChatFactory.Create(this);

        _eventHub = eventHub;
    }

    public ILogger<IRoom> Logger { get; }
    public IRoomManager RoomManager { get; }
    public IRoomDetails RoomDetails { get; }
    public IRoomCycleManager RoomCycleManager { get; }
    public IRoomSecurityManager RoomSecurityManager { get; }
    public IRoomFurnitureManager RoomFurnitureManager { get; }
    public IRoomUserManager RoomUserManager { get; }
    public IRoomChatManager RoomChatManager { get; }

    public IRoomModel RoomModel { get; private set; }
    public IRoomMap RoomMap { get; private set; }

    public void TryDispose()
    {
        if (IsDisposed || IsDisposing) return;

        if (_remainingDisposeTicks != -1) return;

        if (RoomDetails.UsersNow > 0) return;

        RoomCycleManager.Stop();

        // clear the users waiting at the door

        _remainingDisposeTicks = DefaultSettings.RoomDisposeTicks;
    }

    public void CancelDispose()
    {
        RoomCycleManager.Start();

        _remainingDisposeTicks = -1;
    }

    public void EnterRoom(IPlayer player, IPoint location = null)
    {
        if (player == null) return;

        player.Session.SendQueue(new RoomEntryTileMessage
        {
            Direction = RoomModel.DoorLocation.Rotation,
            X = RoomModel.DoorLocation.X,
            Y = RoomModel.DoorLocation.Y
        });

        player.Session.SendQueue(new HeightMapMessage
        {
            RoomModel = RoomModel,
            RoomMap = RoomMap
        });

        player.Session.SendQueue(new FloorHeightMapMessage
        {
            IsZoomedIn = false,
            WallHeight = RoomDetails.WallHeight,
            RoomModel = RoomModel
        });

        player.Session.Flush();

        AddObserver(player.Session);

        RoomUserManager.AddPlayerToRoom(player);

        RoomFurnitureManager.SendFurnitureToSession(player.Session);

        var avatarObject = RoomUserManager.CreateRoomObjectAndAssign(player, location);

        avatarObject.Logic.AddStatus(RoomObjectAvatarStatus.FlatControl, ((int) RoomSecurityManager.GetControllerLevel(player)).ToString());

        player.Session.Send(new RoomVisualizationSettingsMessage
        {
            WallsHidden = RoomDetails.HideWalls,
            FloorThickness = (int)RoomDetails.ThicknessFloor,
            WallThickness = (int)RoomDetails.ThicknessWall
        });

        player.Session.Send(new RoomEntryInfoMessage
        {
            RoomId = Id,
            Owner = RoomSecurityManager.IsOwner(player)
        });

        player.Session.Send(new RoomEventMessage());
        player.Session.Send(new HanditemConfigurationMessage());

        if (player.RoomObject != null)
        {
            var message = _eventHub.Dispatch(new AvatarEnterRoomEvent
            {
                Avatar = player.RoomObject
            });

            if (message.IsCancelled) player.RoomObject.Dispose();
        }
    }

    public void AddObserver(ISession session)
    {
        lock (_roomObserverLock)
        {
            _roomObservers.Add(session);
        }
    }

    public void RemoveObserver(ISession session)
    {
        lock (_roomObserverLock)
        {
            _roomObservers.Remove(session);
        }
    }

    public async Task Cycle()
    {
        if (_remainingDisposeTicks > -1)
        {
            if (_remainingDisposeTicks == 0)
            {
                await DisposeAsync();

                _remainingDisposeTicks = -1;

                return;
            }

            _remainingDisposeTicks--;
        }

        await RoomCycleManager.Cycle();
    }

    public void SendComposer(IComposer composer)
    {
        lock (_roomObserverLock)
        {
            foreach (var session in _roomObservers) session.Send(composer);
        }
    }

    public int Id => RoomDetails.Id;

    public bool IsGroupRoom => false;

    protected override async Task OnInit()
    {
        await LoadMapping();

        await RoomSecurityManager.InitAsync();
        await RoomFurnitureManager.InitAsync();
        await RoomUserManager.InitAsync();
        await RoomChatManager.InitAsync();

        RoomCycleManager.AddCycle(new RoomObjectCycle(this));
        RoomCycleManager.AddCycle(new RoomRollerCycle(this));
        RoomCycleManager.AddCycle(new RoomUserStatusCycle(this));

        RoomCycleManager.Start();
    }

    protected override async Task OnDispose()
    {
        _remainingDisposeTicks = -1;

        RoomCycleManager.Stop();

        await RoomManager.RemoveRoom(Id);

        RoomCycleManager.Dispose();

        await RoomUserManager.DisposeAsync();
        await RoomFurnitureManager.DisposeAsync();
        await RoomSecurityManager.DisposeAsync();
    }

    private async Task LoadMapping()
    {
        if (RoomMap != null)
        {
            RoomMap.Dispose();

            RoomMap = null;
        }

        RoomModel = null;

        var roomModel = await RoomManager.GetModel(RoomDetails.ModelId);

        if (roomModel == null || !roomModel.IsValid) return;

        RoomModel = roomModel;
        RoomMap = new RoomMap(this);

        RoomMap.GenerateMap();
    }
}