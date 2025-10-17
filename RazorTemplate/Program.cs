using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using RazorTemplate.BackGroundJob;
using RazorTemplate.Context;
using RazorTemplate.CustomHealthCheck;
using RazorTemplate.Exceptions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddProblemDetails(option =>
{
    option.CustomizeProblemDetails = context =>
    {
        var problem = new ProblemDetailsContext()
        {
         HttpContext=context.HttpContext,
          AdditionalMetadata = context.AdditionalMetadata,
           Exception = context.Exception,
            ProblemDetails=new ProblemDetails()
            {
              Status=StatusCodes.Status400BadRequest
            }
        
        };
};
});
builder.Services.AddHealthChecks()
    .AddCheck<SqlHealthCheck>("custom-sql",HealthStatus.Unhealthy)
    .AddRedis("Redis Connectionstring")
    .AddNpgSql("Database Connectionstring");
builder.Services.AddExceptionHandler<CustomExceptionHandler>();
builder.Services.AddRazorPages();
builder.Services.AddDbContext<ShopDbConext>(option=>option.UseNpgsql(builder.Configuration.GetConnectionString("Shop")));
builder.Services.AddHttpClient();
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "JwtApi",
            ValidAudience = "account",
            IssuerSigningKey = new SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes("ThisIsASecretKeyForJwtTokenGeneration"))
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Cookies["AuthToken"];
                if (!string.IsNullOrEmpty(token))
                    context.Token = token;
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddAuthorization();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseHttpsRedirection();

app.MapHealthChecks("/healthCheck",
    new HealthCheckOptions()
    {
     ResponseWriter= UIResponseWriter.WriteHealthCheckUIResponse,
     ResultStatusCodes =
     {
           [HealthStatus.Healthy]=StatusCodes.Status200OK,
           [HealthStatus.Degraded]=StatusCodes.Status204NoContent,
           [HealthStatus.Unhealthy]=StatusCodes.Status505HttpVersionNotsupported
     }
    });
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
