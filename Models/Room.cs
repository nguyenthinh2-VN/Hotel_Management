namespace HotelManagement.Models
{
    public class Room
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public int RoomTypeId { get; set; }
        public string Status { get; set; } = "Available";
        public int Floor { get; set; }
        public RoomType? RoomType { get; set; }
    }

    public class RoomType
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
