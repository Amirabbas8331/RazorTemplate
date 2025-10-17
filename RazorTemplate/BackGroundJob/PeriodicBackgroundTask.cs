
namespace RazorTemplate.BackGroundJob;

public class PeriodicBackgroundTask : BackgroundService
{
    private readonly TimeSpan _period=TimeSpan.FromSeconds(5);
    private readonly ILogger<PeriodicBackgroundTask> _logger;

    public PeriodicBackgroundTask(ILogger<PeriodicBackgroundTask> logger)
    {
        _logger = logger;
    }
    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
       using PeriodicTimer timer = new PeriodicTimer(_period);
        while (!stoppingToken.IsCancellationRequested &&
               await timer.WaitForNextTickAsync(stoppingToken))
        {
            _logger.LogInformation("Executing PeriodicBackgroundTask");
        }
    }
}
