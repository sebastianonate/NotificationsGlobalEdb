using Notifications.Infrastructure.BlobStorage;

namespace Notifications.WebAPI.Services
{
    public class DeleteOldLogFilesHostedService : IHostedService, IDisposable
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;
        private Timer _timer;
        public DeleteOldLogFilesHostedService(IWebHostEnvironment env, IConfiguration configuration)
        {
            _env = env;
            _configuration = configuration;
        }
     

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromDays(1));
            return Task.CompletedTask;
        }

        private void DoWork(object state) {
            Console.WriteLine("Verificando logs a eliminar");
            var maxNumbersOfLogs = 10;
            var folderPath = _env.ContentRootPath + Path.DirectorySeparatorChar + "Logs";

            var files = Directory.GetFiles(folderPath)
            .Select(f => new FileInfo(f))
            .OrderBy(f => f.CreationTime);

            if (files.Count() > maxNumbersOfLogs)
            {
                var rangeToDelete = files.Count() - maxNumbersOfLogs;
                var filesToDelete = files.Take(rangeToDelete).ToList();
                filesToDelete.ForEach(f => f.Delete());
            }
           
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
