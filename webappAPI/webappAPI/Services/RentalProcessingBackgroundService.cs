using webappAPI.Services;

namespace webappAPI.Services
{
    public class RentalProcessingBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RentalProcessingBackgroundService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1); // Check every hour

        public RentalProcessingBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<RentalProcessingBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Rental Processing Background Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Processing overdue rentals at: {time}", DateTimeOffset.Now);

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var rentalService = scope.ServiceProvider.GetRequiredService<ICarRentalService>();
                        await rentalService.ProcessOverdueRentalsAsync();
                    }

                    _logger.LogInformation("Overdue rentals processed successfully at: {time}", DateTimeOffset.Now);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing overdue rentals.");
                }

                // Wait for the next check interval
                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("Rental Processing Background Service is stopping.");
        }
    }
}
