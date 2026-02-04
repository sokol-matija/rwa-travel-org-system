using System;

namespace WebApp.Models
{
    public class LogModel
    {
        public int Id { get; set; }

        public DateTime Timestamp { get; set; }

        public string Level { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public string FormattedTimestamp => Timestamp.ToString("yyyy-MM-dd HH:mm:ss");

        public string LevelBadgeClass => Level?.ToLower() switch
        {
            "error" => "bg-danger",
            "warning" => "bg-warning text-dark",
            "info" => "bg-info",
            "debug" => "bg-secondary",
            _ => "bg-primary"
        };

        public string LevelIcon => Level?.ToLower() switch
        {
            "error" => "fas fa-exclamation-circle",
            "warning" => "fas fa-exclamation-triangle",
            "info" => "fas fa-info-circle",
            "debug" => "fas fa-bug",
            _ => "fas fa-file-alt"
        };
    }
}
