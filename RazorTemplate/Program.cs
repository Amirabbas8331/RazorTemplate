using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Polly;
using Polly.CircuitBreaker;
using Polly.RateLimiting;
using Polly.Retry;
using Polly.Timeout;
using RazorTemplate.BackGroundJob;
using RazorTemplate.Context;
using RazorTemplate.CustomHealthCheck;
using RazorTemplate.Exceptions;
using System.Threading.RateLimiting;

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

var tokenBucket = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
{
    TokenLimit = 10,
    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
    QueueLimit = 0,
    ReplenishmentPeriod = TimeSpan.FromSeconds(5),
    TokensPerPeriod = 5,
    AutoReplenishment = true
});
var fixedwindow = new FixedWindowRateLimiter(new FixedWindowRateLimiterOptions
{
    PermitLimit = 5,                           // allow 5 requests
    Window = TimeSpan.FromSeconds(5),          // per 5‑second window
    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
    QueueLimit = 0                             // don’t queue extra requests
});
var slidingWindow = new SlidingWindowRateLimiter(new SlidingWindowRateLimiterOptions
{
    PermitLimit = 10,                          // max requests
    Window = TimeSpan.FromSeconds(10),         // total window size
    SegmentsPerWindow = 2,                     // divides window into segments
    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
    QueueLimit = 0
});
var concurrencyLimiter = new ConcurrencyLimiter(new ConcurrencyLimiterOptions
{
    PermitLimit = 5,                           // max concurrent operations
    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
    QueueLimit = 10                            // how many can wait
});
builder.Services.AddResiliencePipeline("retry", builder =>
{
    builder.AddRetry(new RetryStrategyOptions
    {
        ShouldHandle = new PredicateBuilder().Handle<Exception>(),
        Delay = TimeSpan.FromSeconds(1),
        MaxRetryAttempts = 2,
        BackoffType = DelayBackoffType.Exponential,
        UseJitter = true,
    });
    builder.AddRateLimiter(new RateLimiterStrategyOptions
    {
        RateLimiter = async args => await tokenBucket.AcquireAsync(1)
    });
    // Timeout: cancel operations after 10 seconds
    builder.AddTimeout(new TimeoutStrategyOptions
    {
        Timeout = TimeSpan.FromSeconds(10)
    });
    // Circuit breaker: open after 3 consecutive failures, reset after 30s
    builder.AddCircuitBreaker(new CircuitBreakerStrategyOptions
    {
        FailureRatio = 0.5,                      // 50% failure rate
        MinimumThroughput = 10,                  // at least 10 calls before evaluating
        SamplingDuration = TimeSpan.FromSeconds(30),
        BreakDuration = TimeSpan.FromSeconds(30),
        ShouldHandle = new PredicateBuilder().Handle<Exception>() // break on any exception
    });
});

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
builder.Services.AddExceptionHandler<CustomExceptionHandler>();
builder.Services.AddRazorPages();
builder.Services.AddDbContext<ShopDbConext>(option=>option.UseNpgsql(builder.Configuration.GetConnectionString("Shop")))
    .AddAllElasticApm();
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
builder.Services.AddHostedService<PeriodicBackgroundTask>();
builder.Services.AddAuthorization();


builder.Services.AddAllElasticApm();
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
app.MapReverseProxy();
app.MapGet("", () => "").RequireRateLimiting("retry");
app.Run();
