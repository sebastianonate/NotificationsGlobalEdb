using Notifications.Infrastructure.Dataverse;
using Notifications.Infrastructure.Mails;
using Notifications.Infrastructure.Mails.Helpers;
using Notifications.Infrastructure.Teams;
using Notifications.Infrastructure.Teams.Helpers;

namespace Notifications.WebAPI.Services
{
    public class FetchNotificationsScheduledHostedService : IHostedService, IDisposable
    {
        private Timer _timer;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        public FetchNotificationsScheduledHostedService(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            double minutes = 1;
            double.TryParse(_configuration["CheckScheduleNotificationsEvery:Minutes"], out minutes);
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(minutes));
            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var notificationsScheduleService = scope.ServiceProvider.GetRequiredService<FetchNotificationsScheduleService>();
                var emailService = scope.ServiceProvider.GetRequiredService<MailSmtpService>();
                var teamsService = scope.ServiceProvider.GetRequiredService<TeamsService>();
                var notificationsScheduledForNow = await notificationsScheduleService.GetNotificationFromCurrentDate();
                var table = "NotificacionesProgramadas";
                if (notificationsScheduledForNow.Any())
                {
                    Console.WriteLine("Hay notificaciones");
                    foreach (var notification in notificationsScheduledForNow)
                    {
                        var tipo = (string?)notification.data[notificationsScheduleService.GetAttributeName(table, "Type")];
                        var notificationScheduledId = (string?)notification.data[notificationsScheduleService.GetAttributeName(table, "Id")];

                        switch (tipo.ToUpper())
                        {
                            case "CORREO":
                                var mailData = MailNotificationDataConverter.From(notification, _configuration);
                                var r = await emailService.Send(mailData);
                                await notificationsScheduleService.DeleteNotificationSchedule(notificationScheduledId, notification.files);
                                Console.WriteLine(r.Message);
                                break;
                            case "TEAMS":
                                var teamsData = TeamsNotificationsDataConverter.From(notification, _configuration);
                                var response = await teamsService.Send(teamsData);
                                await notificationsScheduleService.DeleteNotificationSchedule(notificationScheduledId, notification.files);
                                Console.WriteLine(response.Message);
                                break;
                            default:
                                break;
                        }
                    }

                }
                else {
                    Console.WriteLine("No hay notificaciones");
                }
               
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
