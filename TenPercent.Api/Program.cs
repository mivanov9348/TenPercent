using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TenPercent.Api.Services;
using TenPercent.Application.Interfaces;
using TenPercent.Application.Services;
using TenPercent.Application.Services.Interfaces;
using TenPercent.Data;
using TenPercent.Data.Models;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IPlayerGeneratorService, PlayerGeneratorService>();
builder.Services.AddScoped<ISeasonService, SeasonService>();
builder.Services.AddScoped<IScheduleService, ScheduleService>();
builder.Services.AddScoped<ILeagueService, LeagueService>();
builder.Services.AddScoped<IFixtureService, FixtureService>();
builder.Services.AddScoped<IMatchEngineService, MatchEngineService>();
builder.Services.AddScoped<ISimulationService, SimulationService>();
builder.Services.AddScoped<IAgencyService, AgencyService>();
builder.Services.AddScoped<IFinanceService, FinanceService>();
builder.Services.AddScoped<IPlayerContractService, PlayerContractService>();
builder.Services.AddScoped<IPlayerService, PlayerService>();
builder.Services.AddScoped<IClubService, ClubService>();
builder.Services.AddScoped<IScoutingEngine, ScoutingEngine>();
builder.Services.AddScoped<INegotiationService, NegotiationService>();
builder.Services.AddScoped<IAdminBankService, AdminBankService>();
builder.Services.AddScoped<IAdminSettingsService, AdminSettingsService>();
builder.Services.AddScoped<IScoutReportGenerator, ScoutReportGenerator>();
builder.Services.AddScoped<IScoutingService, ScoutingService>();

builder.Services
    .AddIdentity<User, IdentityRole<int>>(options => 
    {
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 3;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactAppPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await DbSeeder.SeedRolesAndAdminAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Seeding error");
    }
}


app.UseHttpsRedirection();

app.UseCors("ReactAppPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();