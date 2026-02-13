namespace Rooms.Dtos
{
    public class UpdateRoomNameDto
    {
        public required int RoomId { get; set; }
        public required string Name { get; set; }
    }
}
