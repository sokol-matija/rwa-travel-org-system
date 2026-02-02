using System;

namespace WebAPI.DTOs
{
    public class LogDTO
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public required string Level { get; set; }
        public required string Message { get; set; }
    }
}
