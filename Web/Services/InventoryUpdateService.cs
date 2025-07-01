namespace Web.Services
{
    public interface IInventoryUpdateService
    {
        event Func<Task>? OnInventoryUpdated;
        Task StartAsync();
        Task StopAsync();
        bool IsRunning { get; }
    }

    public class InventoryUpdateService : IInventoryUpdateService, IDisposable
    {
        private readonly ILogger<InventoryUpdateService> _logger;
        private PeriodicTimer? _timer;
        private Task? _timerTask;
        private CancellationTokenSource? _cancellationTokenSource;

        public bool IsRunning { get; private set; }
        public event Func<Task>? OnInventoryUpdated;

        public InventoryUpdateService(ILogger<InventoryUpdateService> logger)
        {
            _logger = logger;
        }

        public Task StartAsync()
        {
            if (IsRunning)
                return Task.CompletedTask;

            _cancellationTokenSource = new CancellationTokenSource();
            _timer = new PeriodicTimer(TimeSpan.FromMinutes(3));
            _timerTask = RunPeriodicAsync(_cancellationTokenSource.Token);
            IsRunning = true;

            _logger.LogInformation("Inventory update service started - running every 3 minutes");
            return Task.CompletedTask;
        }

        public async Task StopAsync()
        {
            if (!IsRunning)
                return;

            _cancellationTokenSource?.Cancel();
            _timer?.Dispose();
            IsRunning = false;
            
            if (_timerTask != null)
            {
                try
                {
                    await _timerTask;
                }
                catch (OperationCanceledException)
                {
                    // Expected when cancelling
                }
            }

            _logger.LogInformation("Inventory update service stopped");
        }

        private async Task RunPeriodicAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (await _timer!.WaitForNextTickAsync(cancellationToken))
                {
                    try
                    {
                        _logger.LogDebug("Running periodic inventory update");
                        
                        if (OnInventoryUpdated != null)
                        {
                            await OnInventoryUpdated.Invoke();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error occurred during periodic inventory update");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("Periodic inventory update cancelled");
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _timer?.Dispose();
            _cancellationTokenSource?.Dispose();
            IsRunning = false;
        }
    }
}