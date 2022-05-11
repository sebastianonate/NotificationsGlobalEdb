using Notifications.Core.Helpers;
using Notifications.Infrastructure.BlobStorage;

namespace Notifications.WebAPI.Services
{
    public class SaveLogsInBlobStorageHostedService : IHostedService, IDisposable
    {
        private readonly BlobStorageService _blobStorageService;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;
        private Timer _timer;
        public SaveLogsInBlobStorageHostedService(BlobStorageService blobStorageService, IWebHostEnvironment env, IConfiguration configuration)
        {
            _blobStorageService = blobStorageService;
            _env = env;
            _configuration = configuration;
        }
     

        public Task StartAsync(CancellationToken cancellationToken)
        {
            double minutes = 1;
            double.TryParse(_configuration["UploadToBlobStorageEvery:Minutes"], out minutes);
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(minutes));
            return Task.CompletedTask;

        }

        private void DoWork(object state) {
            string date = DateTimeHelper.GetDateTimeNow().ToString("dd/MM/yyyy").Replace('/', '-');
            var folderPath = _env.ContentRootPath + Path.DirectorySeparatorChar + "Logs";
            var fullFile = folderPath + Path.DirectorySeparatorChar + $"LOG_{date}.txt";
            if (File.Exists(fullFile)) { 
                File.Copy(fullFile, folderPath + Path.DirectorySeparatorChar + $"LOG_{date}copy.txt", true);
                _blobStorageService.SaveHostedFile($"LOG_{date}.txt", $"LOG_{date}copy.txt", folderPath).Wait();
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
