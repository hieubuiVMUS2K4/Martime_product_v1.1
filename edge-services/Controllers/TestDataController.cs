using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MaritimeEdge.Data;
using MaritimeEdge.Models;

namespace MaritimeEdge.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestDataController : ControllerBase
{
    private readonly EdgeDbContext _context;
    private readonly ILogger<TestDataController> _logger;

    public TestDataController(EdgeDbContext context, ILogger<TestDataController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpPost("seed-crew")]
    public async Task<IActionResult> SeedCrewData()
    {
        try
        {
            // Check if data already exists
            var existingCount = await _context.CrewMembers.CountAsync();
            if (existingCount >= 10)
            {
                return Ok(new { message = $"Crew data already exists ({existingCount} members)", count = existingCount });
            }

            var sampleCrew = new List<CrewMember>
            {
                new CrewMember
                {
                    CrewId = "CREW001",
                    FullName = "Captain John Smith",
                    Position = "Master",
                    Rank = "Officer",
                    CertificateNumber = "STCW-12345",
                    CertificateExpiry = DateTime.UtcNow.AddDays(180),
                    MedicalExpiry = DateTime.UtcNow.AddDays(120),
                    Nationality = "United Kingdom",
                    PassportNumber = "GB123456789",
                    DateOfBirth = new DateTime(1975, 5, 15),
                    EmbarkDate = new DateTime(2024, 1, 15),
                    IsOnboard = true,
                    EmergencyContact = "Jane Smith (Wife): +44 7700 900123",
                    EmailAddress = "john.smith@vessel.com",
                    PhoneNumber = "+44 7700 900456",
                    CreatedAt = DateTime.UtcNow
                },
                new CrewMember
                {
                    CrewId = "CREW002",
                    FullName = "Maria Garcia",
                    Position = "Chief Engineer",
                    Rank = "Officer",
                    CertificateNumber = "STCW-23456",
                    CertificateExpiry = DateTime.UtcNow.AddDays(200),
                    MedicalExpiry = DateTime.UtcNow.AddDays(150),
                    Nationality = "Spain",
                    PassportNumber = "ES987654321",
                    DateOfBirth = new DateTime(1980, 8, 22),
                    EmbarkDate = new DateTime(2024, 2, 1),
                    IsOnboard = true,
                    EmergencyContact = "Carlos Garcia (Husband): +34 600 123 456",
                    EmailAddress = "maria.garcia@vessel.com",
                    PhoneNumber = "+34 600 789 012",
                    CreatedAt = DateTime.UtcNow
                },
                new CrewMember
                {
                    CrewId = "CREW003",
                    FullName = "Wei Chen",
                    Position = "Chief Officer",
                    Rank = "Officer",
                    CertificateNumber = "STCW-34567",
                    CertificateExpiry = DateTime.UtcNow.AddDays(90), // WARNING: Expiring soon
                    MedicalExpiry = DateTime.UtcNow.AddDays(60), // CRITICAL: Expiring very soon
                    Nationality = "China",
                    PassportNumber = "CN456789123",
                    DateOfBirth = new DateTime(1985, 3, 10),
                    EmbarkDate = new DateTime(2024, 3, 1),
                    IsOnboard = true,
                    EmergencyContact = "Li Chen (Mother): +86 138 0000 1234",
                    EmailAddress = "wei.chen@vessel.com",
                    PhoneNumber = "+86 138 0000 5678",
                    CreatedAt = DateTime.UtcNow
                },
                new CrewMember
                {
                    CrewId = "CREW004",
                    FullName = "Ahmed Hassan",
                    Position = "Second Engineer",
                    Rank = "Officer",
                    CertificateNumber = "STCW-45678",
                    CertificateExpiry = DateTime.UtcNow.AddDays(250),
                    MedicalExpiry = DateTime.UtcNow.AddDays(200),
                    Nationality = "Egypt",
                    PassportNumber = "EG789123456",
                    DateOfBirth = new DateTime(1988, 11, 5),
                    EmbarkDate = new DateTime(2024, 3, 15),
                    IsOnboard = true,
                    EmergencyContact = "Fatima Hassan (Sister): +20 100 123 4567",
                    EmailAddress = "ahmed.hassan@vessel.com",
                    PhoneNumber = "+20 100 765 4321",
                    CreatedAt = DateTime.UtcNow
                },
                new CrewMember
                {
                    CrewId = "CREW005",
                    FullName = "Raj Kumar",
                    Position = "Second Officer",
                    Rank = "Officer",
                    CertificateNumber = "STCW-56789",
                    CertificateExpiry = DateTime.UtcNow.AddDays(150),
                    MedicalExpiry = DateTime.UtcNow.AddDays(100),
                    Nationality = "India",
                    PassportNumber = "IN321654987",
                    DateOfBirth = new DateTime(1990, 7, 20),
                    EmbarkDate = new DateTime(2024, 4, 1),
                    IsOnboard = true,
                    EmergencyContact = "Priya Kumar (Wife): +91 98765 43210",
                    EmailAddress = "raj.kumar@vessel.com",
                    PhoneNumber = "+91 98765 12345",
                    CreatedAt = DateTime.UtcNow
                },
                new CrewMember
                {
                    CrewId = "CREW006",
                    FullName = "Nguyen Van Tuan",
                    Position = "Bosun",
                    Rank = "Rating",
                    CertificateNumber = "STCW-67890",
                    CertificateExpiry = DateTime.UtcNow.AddDays(220),
                    MedicalExpiry = DateTime.UtcNow.AddDays(180),
                    Nationality = "Vietnam",
                    PassportNumber = "VN654321789",
                    DateOfBirth = new DateTime(1982, 12, 8),
                    EmbarkDate = new DateTime(2024, 2, 15),
                    IsOnboard = true,
                    EmergencyContact = "Tran Thi Lan (Wife): +84 90 123 4567",
                    EmailAddress = "nguyen.tuan@vessel.com",
                    PhoneNumber = "+84 90 765 4321",
                    CreatedAt = DateTime.UtcNow
                },
                new CrewMember
                {
                    CrewId = "CREW007",
                    FullName = "Juan Rodriguez",
                    Position = "Able Seaman",
                    Rank = "Rating",
                    CertificateNumber = "STCW-78901",
                    CertificateExpiry = DateTime.UtcNow.AddDays(25), // CRITICAL: Almost expired
                    MedicalExpiry = DateTime.UtcNow.AddDays(80),
                    Nationality = "Philippines",
                    PassportNumber = "PH987456321",
                    DateOfBirth = new DateTime(1995, 4, 18),
                    EmbarkDate = new DateTime(2024, 3, 20),
                    IsOnboard = true,
                    EmergencyContact = "Rosa Rodriguez (Mother): +63 917 123 4567",
                    EmailAddress = "juan.rodriguez@vessel.com",
                    PhoneNumber = "+63 917 765 4321",
                    CreatedAt = DateTime.UtcNow
                },
                new CrewMember
                {
                    CrewId = "CREW008",
                    FullName = "Oleg Petrov",
                    Position = "Oiler",
                    Rank = "Rating",
                    CertificateNumber = "STCW-89012",
                    CertificateExpiry = DateTime.UtcNow.AddDays(280),
                    MedicalExpiry = DateTime.UtcNow.AddDays(240),
                    Nationality = "Russia",
                    PassportNumber = "RU159753486",
                    DateOfBirth = new DateTime(1987, 9, 3),
                    EmbarkDate = new DateTime(2024, 1, 20),
                    IsOnboard = true,
                    EmergencyContact = "Svetlana Petrova (Wife): +7 900 123 45 67",
                    EmailAddress = "oleg.petrov@vessel.com",
                    PhoneNumber = "+7 900 765 43 21",
                    CreatedAt = DateTime.UtcNow
                },
                new CrewMember
                {
                    CrewId = "CREW009",
                    FullName = "Michael O'Brien",
                    Position = "Chief Cook",
                    Rank = "Rating",
                    CertificateNumber = "STCW-90123",
                    CertificateExpiry = DateTime.UtcNow.AddDays(-10), // EXPIRED!
                    MedicalExpiry = DateTime.UtcNow.AddDays(130),
                    Nationality = "Ireland",
                    PassportNumber = "IE753951456",
                    DateOfBirth = new DateTime(1978, 6, 25),
                    EmbarkDate = new DateTime(2024, 2, 10),
                    IsOnboard = true,
                    EmergencyContact = "Sean O'Brien (Brother): +353 87 123 4567",
                    EmailAddress = "michael.obrien@vessel.com",
                    PhoneNumber = "+353 87 765 4321",
                    CreatedAt = DateTime.UtcNow
                },
                new CrewMember
                {
                    CrewId = "CREW010",
                    FullName = "Kim Min-ho",
                    Position = "Third Engineer",
                    Rank = "Officer",
                    CertificateNumber = "STCW-01234",
                    CertificateExpiry = DateTime.UtcNow.AddDays(300),
                    MedicalExpiry = DateTime.UtcNow.AddDays(270),
                    Nationality = "South Korea",
                    PassportNumber = "KR246813579",
                    DateOfBirth = new DateTime(1992, 2, 14),
                    EmbarkDate = new DateTime(2024, 4, 5),
                    IsOnboard = true,
                    EmergencyContact = "Park Soo-jin (Wife): +82 10 1234 5678",
                    EmailAddress = "kim.minho@vessel.com",
                    PhoneNumber = "+82 10 8765 4321",
                    CreatedAt = DateTime.UtcNow
                }
            };

            _context.CrewMembers.AddRange(sampleCrew);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully seeded {Count} crew members", sampleCrew.Count);

            return Ok(new 
            { 
                message = "Sample crew data inserted successfully", 
                count = sampleCrew.Count,
                crew = sampleCrew.Select(c => new { c.CrewId, c.FullName, c.Position })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding crew data");
            return StatusCode(500, new { error = "Failed to seed data", details = ex.Message });
        }
    }

    [HttpDelete("clear-crew")]
    public async Task<IActionResult> ClearCrewData()
    {
        try
        {
            var allCrew = await _context.CrewMembers.ToListAsync();
            _context.CrewMembers.RemoveRange(allCrew);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Deleted {allCrew.Count} crew members" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing crew data");
            return StatusCode(500, new { error = "Failed to clear data", details = ex.Message });
        }
    }
}
