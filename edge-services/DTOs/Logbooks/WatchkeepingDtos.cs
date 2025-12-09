using System;
using System.ComponentModel.DataAnnotations;

namespace MaritimeEdge.DTOs.Logbooks
{
    public class CreateWatchkeepingLogDto
    {
        [Required]
        public DateTime WatchDate { get; set; }

        [Required]
        [MaxLength(20)]
        public string WatchPeriod { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string WatchType { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string OfficerOnWatch { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Lookout { get; set; }

        public string? WeatherConditions { get; set; }
        
        [MaxLength(50)]
        public string? SeaState { get; set; }
        
        [MaxLength(50)]
        public string? Visibility { get; set; }

        public double? CourseLogged { get; set; }
        public double? SpeedLogged { get; set; }
        public double? PositionLat { get; set; }
        public double? PositionLon { get; set; }
        public double? DistanceRun { get; set; }

        [MaxLength(200)]
        public string? EngineStatus { get; set; }
        
        public string? NotableEvents { get; set; }
    }

    public class UpdateWatchkeepingLogDto : CreateWatchkeepingLogDto
    {
    }

    public class WatchkeepingLogResponseDto
    {
        public Guid Id { get; set; }
        public DateTime WatchDate { get; set; }
        public string WatchPeriod { get; set; } = string.Empty;
        public string WatchType { get; set; } = string.Empty;
        public string OfficerOnWatch { get; set; } = string.Empty;
        
        public string? Lookout { get; set; }
        public string? WeatherConditions { get; set; }
        public string? SeaState { get; set; }
        public string? Visibility { get; set; }
        
        public double? CourseLogged { get; set; }
        public double? SpeedLogged { get; set; }
        public double? PositionLat { get; set; }
        public double? PositionLon { get; set; }
        public double? DistanceRun { get; set; }
        
        public string? EngineStatus { get; set; }
        public string? NotableEvents { get; set; }
        
        public string? MasterSignature { get; set; }
        
        public bool IsSynced { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string OriginNode { get; set; } = string.Empty;
    }

    public class SignWatchkeepingLogDto
    {
        [Required]
        [MaxLength(200)]
        public string MasterSignature { get; set; } = string.Empty;
    }
}
