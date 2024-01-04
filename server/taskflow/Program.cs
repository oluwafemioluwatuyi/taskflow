using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using taskflow.Data;
using taskflow.Mappings;
using taskflow.Models.Domain;
using taskflow.Repositories;
using taskflow.Repositories.Impls;
using taskflow.Repositories.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();


// Inject TaskFlowDbContext into the app
/*builder.Services.AddDbContext<TaskFlowDbContext>(options => 
        options.UseSqlServer(builder.Configuration.GetConnectionString("TaskFlowConnectionString")));*/

builder.Services.AddDbContext<TaskFlowDbContext>(options => 
    options.UseSqlServer(builder.Configuration.GetConnectionString("TaskFlowConnectionString")));


// Inject Repositories
builder.Services.AddScoped<ITokenRepository, TokenRepository>();

// Inject AutoMapper
builder.Services.AddAutoMapper(typeof(AutoMapperProfiles));

// Add Identity packages/solutions
builder.Services.AddIdentityCore<ApplicationUser>()
    .AddTokenProvider<DataProtectorTokenProvider<ApplicationUser>>("TaskFlow")
    .AddEntityFrameworkStores<TaskFlowDbContext>()
    .AddDefaultTokenProviders();

// Setup Identity Options.
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;
});

// Add authentication to the services as well
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException()))
        });



builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseAuthorization();

app.MapControllers();

app.Run();
