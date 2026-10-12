using System.Text.Json.Serialization;
using Microsoft.AspNetCore.OData;
using Microsoft.OpenApi.Models;
using Prescription.Api.Controllers;
using Prescription.Api.Errors;
using Prescription.Api.LoggedProfessional;
using Prescription.Application;
using Prescription.Data;
using Prescription.Data.Context;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("PrescriptionDb")
    ?? throw new InvalidOperationException("Connection string 'PrescriptionDb' não configurada.");
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddApplication().AddData(connectionString);

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
    .AddOData(options =>
    {
        options.Select().Filter().OrderBy().Count()
               .SetMaxTop(PrescriptionsODataController.MaxTop)
               .AddRouteComponents(PrescriptionsODataController.RoutePrefix,
                                   PrescriptionsODataController.BuildModel());
        // Dates are stored in Brasília time; without this OData writes them as UTC ("...Z").
        options.TimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
    });

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy => policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddHealthChecks().AddDbContextCheck<PrescriptionDbContext>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Prescription API", Version = "v1" });
    options.OperationFilter<ProfessionalHeaderOperationFilter>();
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

/// <summary>Entry point; public so the integration tests can start the API (WebApplicationFactory).</summary>
public partial class Program;
