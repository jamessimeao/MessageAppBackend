namespace MessageREST.Models
{
    public class Message
    {
        public required int Id { get; set; }
        public required string RoomId { get; set; }
        public required int SenderId { get; set; }
        public required string Content { get; set; }
        public DateTime Time { get; set; }
    }
}
