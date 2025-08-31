using DotNet.Web.Api.Template.Repositories.Interfaces;

namespace DotNet.Web.Api.Template.Services
{
    public class DeadlineCheckService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DeadlineCheckService> _logger;

        public DeadlineCheckService(IServiceProvider serviceProvider, ILogger<DeadlineCheckService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Deadline Check Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Deadline Check Service is running at: {time}", DateTimeOffset.Now);

                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var decisionRepository = scope.ServiceProvider.GetRequiredService<IDecisionRepository>();
                        await decisionRepository.ProcessExpiredDeadlinesAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while processing deadlines.");
                }

                // Wait for a certain period before the next check (e.g., every 5 minutes)
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }

            _logger.LogInformation("Deadline Check Service is stopping.");
        }
    }
}
