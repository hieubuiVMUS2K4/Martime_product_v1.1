using System;
using Microsoft.EntityFrameworkCore;
using MaritimeEdge.Data;
using MaritimeEdge.Models;
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var connectionString = configuration.GetSection("Database:ConnectionString").Value;

var optionsBuilder = new DbContextOptionsBuilder<EdgeDbContext>();
optionsBuilder.UseNpgsql(connectionString);

using var context = new EdgeDbContext(optionsBuilder.Options);

Console.WriteLine("=================================================");
Console.WriteLine("KIỂM TRA SỐ LƯỢNG RECORDS TRONG 5 BẢNG");
Console.WriteLine("=================================================\n");

try
{
    var crewCertCount = await context.CrewCertificates.CountAsync();
    var countryCertCount = await context.CountryCertificates.CountAsync();
    var crewMemberCount = await context.CrewMembers.CountAsync();
    var certCount = await context.Certificates.CountAsync();
    var countryCount = await context.Countries.CountAsync();
    
    Console.WriteLine($"📊 crew_certificates:      {crewCertCount,6} records");
    Console.WriteLine($"📊 country_certificates:   {countryCertCount,6} records");
    Console.WriteLine($"📊 crew_members:           {crewMemberCount,6} records");
    Console.WriteLine($"📊 certificates:           {certCount,6} records");
    Console.WriteLine($"📊 countries:              {countryCount,6} records");
    
    Console.WriteLine("\n=================================================");
    
    var totalRecords = crewCertCount + countryCertCount + crewMemberCount + certCount + countryCount;
    
    if (totalRecords == 0)
    {
        Console.WriteLine("✅ TẤT CẢ 5 BẢNG ĐÃ ĐƯỢC XÓA SẠCH!");
    }
    else
    {
        Console.WriteLine($"⚠️  Còn {totalRecords} records trong database");
    }
    
    Console.WriteLine("=================================================");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Lỗi: {ex.Message}");
    Console.WriteLine($"   {ex.StackTrace}");
}
