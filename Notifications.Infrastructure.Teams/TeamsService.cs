using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Notifications.Core;
using Notifications.Core.Contracts;
using Microsoft.Graph;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using Notifications.Core.Helpers;
using System.Security;
using Microsoft.Identity.Client;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using Notifications.Infrastructure.Dataverse;
using Newtonsoft.Json.Linq;
using Notifications.Infrastructure.Dataverse.Helper;
using Notifications.Infrastructure.Logs;

namespace Notifications.Infrastructure.Teams
{
    public class TeamsService : INotificationSender<Task<TeamsNotificationResponse>, TeamsNotificationData>
    {
        private readonly GraphServiceClient _graphServiceClient;
        private readonly FlatFileService _flatFileService;
        private readonly IConfiguration _configuration;
        private readonly DataverseService _dataverseService;
        private readonly ScheduledNotificationsTeamsService _scheduleNotificationsTeamsService;
        private readonly LogService<TeamsService> _logger;
        public TeamsService(
            IConfiguration configuration, 
            GraphTokenGenerator graphTokenGenerator,
            DataverseService dataverseService, 
            FlatFileService flatFileService, 
            ScheduledNotificationsTeamsService scheduleNotificationsTeamsService,
            LogService<TeamsService> logger)
        {
            _configuration = configuration;
            _graphServiceClient = new GraphServiceClient(new DelegateAuthenticationProvider(async (requestMessage) =>
                {
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", graphTokenGenerator.GetToken().Result);
                })
            );
            _flatFileService = flatFileService;
            _dataverseService = dataverseService;
            _scheduleNotificationsTeamsService = scheduleNotificationsTeamsService;
            _logger = logger;
        }

        public async Task<TeamsNotificationResponse> Send(TeamsNotificationData team)
        {
            var traceId = Guid.NewGuid();
            var evento = "Envio Teams";
            //Se valida si la solucion existe
            var solution = await _dataverseService.GetSolution(team.SolutionId);
            if (!_dataverseService.SolutionExist(solution)) {
                _logger.LogError(evento, $"La solución que consume el servicio no se encuentra registrada. Id: {team.SolutionId}", traceId);
                return new TeamsNotificationResponse(false, 400, "La solución que consume el servicio no se encuentra registrada.");
            }
            var solutionName = solution[GetAttributeName("Soluciones", "Name")].ToString();

            var from = "";
            try
            {
                from = (await Me()).Mail;
            }
            catch (Exception ex)
            {
                _logger.LogError(evento, ex.Message, traceId);
                await SaveNotificationData(team, from, false, solutionName, traceId);
                return new TeamsNotificationResponse(false, 400, "Ha ocurrido un error en el cliente de Grahp. Consulte los Logs para conocer detalles");
            }
            
            team.To = team.To.Distinct().ToList();
            var members = MailHelper.GetValidEmails(team.To, out var reject);
            if (!members.Any()) { 
                _logger.LogError(evento, "No hay destinatarios válidos. Escriba direcciones de correo válidas.", traceId);
                return new TeamsNotificationResponse(false, 400, "No hay destinatarios válidos. Escriba direcciones de correo válidas.");
            }

            if (team.DeliverDate > DateTimeHelper.GetDateTimeNow())
            {
                var responseScheduleNotifications = await _scheduleNotificationsTeamsService.SaveScheduleNotificationsTeams(team);
                if (!responseScheduleNotifications.IsSuccessStatusCode) {
                    _logger.LogError(evento, responseScheduleNotifications.ReasonPhrase, traceId);
                    await SaveNotificationData(team, from, false, solutionName, traceId);
                    return new TeamsNotificationResponse(true, 400, "Ha ocurrido un error al programar la notificación. Consulte el Log para conocer detalles.");
                }
                _logger.LogInformation(evento, "Notificación programada exitosamente.", traceId);
                return new TeamsNotificationResponse(true, 200, "Notificación programada exitosamente.");
            }

            members.Add(from);
            var chat = new Chat();
            var row = "";
            var chatId = "";
            //Se define el tipo de chat
            if (members.Count == 2)
                chat.ChatType = ChatType.OneOnOne;
            else
            {
                chat.ChatType = ChatType.Group;
                if (_flatFileService.Exists($"chatsOf{from}.txt"))
                    row = _flatFileService.FindOnFile($"chatsOf{from}.txt", members);
            }

            //Se agregan los miembros
            chat.Members = GetMemberCollectionPage(members);
            //Se define el mensaje
            var chatMessage = GetChatMessage(team.Subject, solutionName, team.Message);

            if (string.IsNullOrWhiteSpace(row))
            {
                try
                {
                    var returnedChat = await _graphServiceClient.Chats
                        .Request()
                        .AddAsync(chat);
                    chatId = returnedChat.Id;
                }
                catch (Exception ex) {
                    _logger.LogError(evento,$"Es posible que el correo no esté registrado en la organización {ex.Message}"  , traceId);
                    await SaveNotificationData(team, from, false, solutionName, traceId);
                    return new TeamsNotificationResponse(false, 400, "Ha ocurrido un error al crear el chat. Consulte el Log para conocer detalles. \n Es posible que el correo no esté registrado en la organización");
                }
                 if (chat.ChatType == ChatType.Group)
                    _flatFileService.SaveInFile($"chatsOf{from}.txt", $"{chatId};{string.Join('|', members)} \n");
            }
            else
            {
                chatId = row.Split(';')[0];
            }

            try
            {
                var response = await _graphServiceClient.Chats[chatId].Messages
                    .Request()
                    .AddAsync(chatMessage);
                await SaveNotificationData(team, from, true, solutionName, traceId);
                _logger.LogInformation(evento, "Se envió la notificación", traceId);
                return new TeamsNotificationResponse(true, 200, "Se envió la notificación");
            }
            catch (Exception ex)
            {
                await SaveNotificationData(team, from, false, solutionName, traceId);
                _logger.LogInformation(evento, ex.Message, traceId);
                return new TeamsNotificationResponse(false, 400, "Ha ocurrido un error al enviar el mensaje. Consulte los Logs para conocer detalles.");
            }
        }

        private ChatMembersCollectionPage GetMemberCollectionPage(List<string> members)
        {
            var chatMembers = new ChatMembersCollectionPage();
            members.ForEach(x =>
            {
                var member = new AadUserConversationMember
                {
                    Roles = new List<String>() { "owner" },
                    AdditionalData = new Dictionary<string, object>() { { "user@odata.bind", "https://graph.microsoft.com/v1.0/users/" + x } }
                };
                chatMembers.Add(member);
            });
            return chatMembers;
        }

        private ChatMessage GetChatMessage(string subject, string solutionName, string message)
        {
            return new ChatMessage
            {
                Body = new ItemBody
                {
                    Content = !string.IsNullOrWhiteSpace(subject) ? $"<b>{solutionName}, {subject}:</b> <br> {message}" : $"<b>{solutionName},</b> <br> {message}",
                    ContentType = BodyType.Html
                },
            };
        }

        public async Task<User> Me()
        {
            var result = await _graphServiceClient.Me.Request().GetAsync();
            return result;
        }

        private string GetAttributeName(string tableName, string property)
        {
            return _dataverseService.GetAttributeName(tableName, property);
        }

        public void ValidateData(TeamsNotificationData data)
        {
            data.Subject ??= "";
            data.DeliverDate ??= DateTime.MinValue;
        }

        public async Task<HttpResponseMessage> SaveNotificationData(TeamsNotificationData data, string from, bool isSuccess, string solutionName, Guid traceId)
        {
            JObject content = new JObject();
            DateTime utcTime = DateTime.UtcNow;
            TimeZoneInfo myZone = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
            DateTime currentDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, myZone);

            var table = "Notificaciones";
            content[GetAttributeName(table, "From")] = from;
            content[GetAttributeName(table, "Type")] = "Teams";
            content[GetAttributeName(table, "To")] = string.Join(";", data.To);
            content[GetAttributeName(table, "Subject")] = data.Subject;
            content[GetAttributeName(table, "Envio")] = isSuccess ? "Exitoso" : "Fallido";
            content[GetAttributeName(table, "DeliverDate")] = currentDateTime;
            content[GetAttributeName(table, "DeliverDateText")] = currentDateTime.ToString("yyyy-MM-ddTHH:mm:ss");
            content[$"{GetAttributeName(table, "SolutionId")}@odata.bind"] =
                    FieldBuilderHelper.BuildRelationshipField(_configuration["Dataverse:Tables:Soluciones"], data.SolutionId);
            content[$"{GetAttributeName(table, "SolutionId")}@OData.Community.Display.V1.FormattedValue"] = solutionName;
            
            var response = await _dataverseService.RegistarDatos(_configuration["Dataverse:Tables:Notificaciones"], content);
            if (response.IsSuccessStatusCode)
                _logger.LogInformation("Envio Teams", "Se ha registrado la notificación exitosamente.", traceId);
            else
                _logger.LogError("Envio Teams", $"No se pudo guardar la informacion en dataverse. {response.ReasonPhrase}", traceId);

            return response;
        }

        public async Task<IGraphServiceChatsCollectionPage> GetChats()
        {
            var response = await _graphServiceClient.Chats.Request().GetAsync();
            return response;
        }

    }
    public record TeamsNotificationResponse(bool IsSuccess, int StatusCode, string Message);
}