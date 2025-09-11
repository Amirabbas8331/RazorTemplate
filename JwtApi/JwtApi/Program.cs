
using JwtApi.Api.Database;
using JwtApi.Api.Extensions;
using JwtApi.Api.Users;
using JwtApi.Api.Users.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Net.Mail;
using System.Text;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGenWithAuth();

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("Database")));

builder.Services.AddSingleton<PasswordHasher>();
builder.Services.AddSingleton<TokenProvider>();
builder.Services.AddScoped<EmailVerificationLinkFactory>();

builder.Services.AddHttpContextAccessor();
builder.Services
    .AddFluentEmail("amirabbas8331@gmail.com")
    .AddSmtpSender(new SmtpClient("smtp.gmail.com")
    {
        Port = 587,
        EnableSsl = true,
        Credentials = new NetworkCredential("amirabbas8331@gmail.com", "xiejexiimynzwxzu")
    });

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.RequireHttpsMetadata = false;
        o.TokenValidationParameters = new TokenValidationParameters
        {
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!)),
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddScoped<RegisterUser>();
builder.Services.AddScoped<LoginUser>();
builder.Services.AddScoped<VerifyEmail>();
builder.Services.AddScoped<GetUser>();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.ApplyMigrations();
}

UserEndpoints.Map(app);

app.UseAuthentication();

app.UseAuthorization();

app.Run();
