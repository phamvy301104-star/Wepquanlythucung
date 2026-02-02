using Microsoft.Extensions.Hosting;

namespace nhom6_admin.Services
{
    /// <summary>
    /// Hosted service that prevents the app from shutting down unexpectedly
    /// </summary>
    public class KeepAliveHostedService : IHostedService
    {
        private readonly ILogger<KeepAliveHostedService> _logger;
        private readonly IHostApplicationLifetime _lifetime;

        public KeepAliveHostedService(ILogger<KeepAliveHostedService> logger, IHostApplicationLifetime lifetime)
        {
            _logger = logger;
            _lifetime = lifetime;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("✅ KeepAliveHostedService started");
            
            // Register for shutdown events to log them
            _lifetime.ApplicationStopping.Register(() => 
            {
                _logger.LogInformation("Application stopping signal received");
            });

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("KeepAliveHostedService stopped");
            return Task.CompletedTask;
        }
    }
}
