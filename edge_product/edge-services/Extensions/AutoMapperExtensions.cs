using AutoMapper;
using MaritimeEdge.DTOs.Common;
using MaritimeEdge.Mappings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MaritimeEdge.Extensions;

/// <summary>
/// Dependency Injection extensions for registering AutoMapper.
/// Add to Program.cs: services.AddAutoMapperProfiles();
/// </summary>
public static class AutoMapperDependencyInjection
{
    /// <summary>
    /// Register all AutoMapper profiles and configuration.
    /// Usage in Program.cs:
    /// var builder = WebApplication.CreateBuilder(args);
    /// builder.Services.AddAutoMapperProfiles();
    /// </summary>
    public static IServiceCollection AddAutoMapperProfiles(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(AutoMapperDependencyInjection).Assembly);
        return services;
    }
}
