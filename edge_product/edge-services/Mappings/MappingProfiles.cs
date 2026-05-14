using AutoMapper;
using MaritimeEdge.Models;
using MaritimeEdge.DTOs;

namespace MaritimeEdge.Mappings;

/// <summary>
/// AutoMapper Profile for Voyage-related entities.
/// Consolidates DTO projections into declarative mappings.
/// </summary>
public class VoyageProfile : Profile
{
    public VoyageProfile()
    {
        CreateMap<VoyageRecord, VoyageDetailDto>().ReverseMap();
        CreateMap<PortCall, PortCallDto>().ReverseMap();
        CreateMap<VoyagePlanLeg, VoyagePlanLegDto>().ReverseMap();
        CreateMap<VoyageStatusHistory, VoyageStatusHistoryDto>().ReverseMap();
        CreateMap<VoyageCrewAssignment, VoyageCrewAssignmentDto>().ReverseMap();
    }
}

/// <summary>
/// AutoMapper Profile for Crew-related entities.
/// Consolidates 100+ manual crew property mappings into declarative configurations.
/// </summary>
public class CrewProfile : Profile
{
    public CrewProfile()
    {
        // Crew Member to DTO mapping with ForMember configurations for complex logic
        CreateMap<CrewMember, CrewMemberDto>()
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src =>
                (src.FullName ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries).Length > 0
                    ? (src.FullName ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries)[0]
                    : src.FullName ?? ""))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src =>
                (src.FullName ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries).Length > 1
                    ? string.Join(" ", (src.FullName ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries).Skip(1))
                    : ""))
            .ForMember(dest => dest.Rank, opt => opt.MapFrom(src => src.Rank))
            .ForMember(dest => dest.CountryName, opt => opt.MapFrom(src => src.Country != null ? src.Country.CountryName : null))
            .ForMember(dest => dest.RankGroup, opt => opt.MapFrom(src => GetRankGroup(src)));
        
        CreateMap<CrewMember, CrewDetailDto>()
            .IncludeBase<CrewMember, CrewMemberDto>();
        
        CreateMap<Rank, RankDto>();
    }

    /// <summary>
    /// Determine rank group for frontend crew filtering/display
    /// </summary>
    private static string? GetRankGroup(CrewMember crew)
    {
        if (crew.Rank?.RankName?.Contains("Officer", StringComparison.OrdinalIgnoreCase) == true)
            return "Officers";
        if (crew.Department?.Equals("Deck", StringComparison.OrdinalIgnoreCase) == true)
            return "Deck";
        if (crew.Department?.Equals("Engine", StringComparison.OrdinalIgnoreCase) == true)
            return "Engine";
        if (crew.Department?.Contains("Catering", StringComparison.OrdinalIgnoreCase) == true ||
            crew.Department?.Contains("Galley", StringComparison.OrdinalIgnoreCase) == true)
            return "Galley";
        return null;
    }
}

/// <summary>
/// AutoMapper Profile for Port Master Data.
/// </summary>
public class PortProfile : Profile
{
    public PortProfile()
    {
        CreateMap<Port, PortDto>().ReverseMap();
    }
}

/// <summary>
/// AutoMapper Profile for Reporting entities.
/// Phase 3 Enhancement: Consolidate 200+ manual report DTO projections.
/// </summary>
public class ReportingProfile : Profile
{
    public ReportingProfile()
    {
        // Report Type
        CreateMap<ReportType, ReportTypeDto>().ReverseMap();
        
        // Maritime Report (parent entity)
        CreateMap<MaritimeReport, ReportSummaryDto>().ReverseMap();

        // Noon Reports - Consolidates 50+ property mappings
        CreateMap<NoonReport, NoonReportDto>().ReverseMap();

        // Departure Reports - Consolidates 30+ property mappings
        CreateMap<DepartureReport, DepartureReportDto>().ReverseMap();

        // Arrival Reports - Consolidates 30+ property mappings
        CreateMap<ArrivalReport, ArrivalReportDto>().ReverseMap();

        // Bunker Reports - Consolidates 25+ property mappings
        CreateMap<BunkerReport, BunkerReportDto>().ReverseMap();

        // Position Reports - Consolidates 10+ property mappings
        CreateMap<PositionReport, PositionReportDto>().ReverseMap();
    }
}

/// <summary>
/// AutoMapper Profile for Maintenance-related entities.
/// </summary>
public class MaintenanceProfile : Profile
{
    public MaintenanceProfile()
    {
        // No maintenance mappings at this time
    }
}
