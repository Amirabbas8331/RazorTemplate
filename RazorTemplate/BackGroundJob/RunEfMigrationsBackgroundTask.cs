using Microsoft.EntityFrameworkCore;
using RazorTemplate.Context;


namespace RazorTemplate.BackGroundJob;

public class RunEfMigrationsBackgroundTask : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public RunEfMigrationsBackgroundTask(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using IServiceScope scope = _serviceProvider.CreateScope();

        await using ShopDbConext dbContext =
            scope.ServiceProvider.GetRequiredService<ShopDbConext>();

        await dbContext.Database.MigrateAsync(stoppingToken);
    }
}