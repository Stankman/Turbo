﻿using Turbo.Core.Game.Rooms.Utils;

namespace Turbo.Core.Game.Players.Rooms;
public interface IPendingRoomInfo
{
    public int RoomId { get; set; }
    public bool Approved { get; set; }
    public IPoint Location { get; set; }
}