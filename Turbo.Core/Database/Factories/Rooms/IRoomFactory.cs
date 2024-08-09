using Turbo.Core.Database.Entities.Room;
using Turbo.Core.Game.Rooms;

namespace Turbo.Core.Database.Factories;

public interface IRoomFactory
{
    public IRoom Create(RoomEntity roomEntity);
}