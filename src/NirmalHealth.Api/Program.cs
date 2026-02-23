using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NirmalHealth.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
{
    var conn = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(conn, b => b.MigrationsAssembly("NirmalHealth.Infrastructure"));
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key not set")))
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? new[] { "http://localhost:5173" })
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Nirmal Health API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme { In = Microsoft.OpenApi.Models.ParameterLocation.Header, Name = "Authorization", Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement { { new Microsoft.OpenApi.Models.OpenApiSecurityScheme { Reference = new Microsoft.OpenApi.Models.OpenApiReference { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() } });
});

builder.Services.AddScoped<NirmalHealth.Api.Services.IAuthService, NirmalHealth.Api.Services.AuthService>();
builder.Services.AddScoped<NirmalHealth.Api.Services.IHospitalService, NirmalHealth.Api.Services.HospitalService>();
builder.Services.AddScoped<NirmalHealth.Api.Services.IDoctorService, NirmalHealth.Api.Services.DoctorService>();
builder.Services.AddScoped<NirmalHealth.Api.Services.SlotGenerationService>();
builder.Services.AddScoped<NirmalHealth.Api.Services.IAppointmentService, NirmalHealth.Api.Services.AppointmentService>();
builder.Services.AddScoped<NirmalHealth.Api.Services.ISymptomAnalysisService, NirmalHealth.Api.Services.SymptomAnalysisService>();
builder.Services.AddScoped<NirmalHealth.Api.Services.IAuditService, NirmalHealth.Api.Services.AuditService>();
builder.Services.AddHttpClient<NirmalHealth.Api.Services.IOpenAiService, NirmalHealth.Api.Services.OpenAiService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();
    await SeedData.SeedAsync(db, logger);
}

app.Run();
