using FluentValidation;
using Notifications.Core.Helpers;
using Notifications.Infrastructure.Mails;
using Notifications.Infrastructure.Mails.DTOs;
using Notifications.Infrastructure.Teams;

namespace Notifications.WebAPI.Validators
{
    public class MailNotificationDataJsonValidator: AbstractValidator<EmailNotificationDataJson>
    {
        private readonly IConfiguration _configuration;
        private string messageFromValidExtensions;
        private string messageFromValidSize;
        private List<Byte[]> listOfBytesArrays;
        private bool validBase64;

        public MailNotificationDataJsonValidator(IConfiguration configuration)
        {
            _configuration = configuration;
            listOfBytesArrays = new List<Byte[]>();
            messageFromValidExtensions = "Los archivos adjuntos tienen extensiones no permitidas.";
            messageFromValidSize = "Los archivos adjuntos exceden el peso máximo permitido.";
            RuleFor(x => x.SolutionId).Must(x => x != default(Guid) && x != Guid.Empty).WithMessage("Es requerido. No debe estar vacio");
            RuleFor(x => x.TemplateId).Must(x => x != default(Guid) && x != Guid.Empty).WithMessage("Es requerido. No debe estar vacio");
            //RuleFor(x => x.ParamsTemplate).Must(x => x.Count > 0).WithMessage("No debe estar vacio");
            RuleFor(x => x.From).Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Es requerido. No debe estar vacio");
            When(x => !string.IsNullOrWhiteSpace(x.From), () => { 
                RuleFor(x => x.From).Must(x => MailHelper.IsValidEmail(x.Trim())).WithMessage("Debe ser un email válido");
            });
            RuleFor(x => x.To).Must(x => BeNotNullOrEmpty(x)).WithMessage("Es requerido. No debe estar vacio");
            When(x => BeNotNullOrEmpty(x.Attachments), () => {
                RuleFor(x => x.Attachments).Must(ValidBase64).WithMessage("Debe ser un base64 válido. La entrada contiene caracteres que no son base64 y no son válidos.");
                When(x=> validBase64, ()=>{ 
                    RuleFor(x => x.Attachments).Must(HaveValidExtensions).WithMessage(messageFromValidExtensions);
                    RuleFor(x => x.Attachments).Must(HaveValidSize).WithMessage(messageFromValidSize);
                });
            });
        }

        private bool ValidBase64(List<FileDataBase64> attachments)
        {
            if (attachments == null) return true;
            bool response = true;
            validBase64 = true;
            attachments.ForEach(x => {
                try
                {
                    listOfBytesArrays.Add(Convert.FromBase64String(x.base64Data));
                }
                catch (Exception)
                {
                    validBase64 = false;
                    response = false;
                    return;
                }
            });
            return response;
        }

        private bool HaveValidSize(List<FileDataBase64> attachments) {
            if (attachments == null) return true;

            var oneMbInBytes = 1048576;
            long defaultSizeInMegaBytes = 10;
            long.TryParse(_configuration["Files:SizeInMegaBytes"], out defaultSizeInMegaBytes);

            long defaultSizeInBytes = oneMbInBytes * defaultSizeInMegaBytes;

            long size = 0;
            int index = 0;
            foreach (var item in attachments)
            {
                size = size + listOfBytesArrays[index].Length;
                index++;
            }

            if (size > defaultSizeInBytes)
            {
                messageFromValidSize = $"Los archivos adjuntos excenden el limete del tamaño de {defaultSizeInMegaBytes} MB";
                return false;
            }

            return true;

        }

        private bool HaveValidExtensions(List<FileDataBase64> attachments) {
            if (attachments == null) return true;
            var validExtensions = _configuration["Files:ValidExtensions"].Trim().Split(',');
            foreach (var item in attachments)
            {
                try
                {
                    var splitFileArray = item.Filename.Split('.');
                    var extension = splitFileArray[splitFileArray.Length - 1];
                    if (validExtensions.FirstOrDefault(x => x.Trim() == extension) == null)
                    {
                        messageFromValidExtensions = $"La extensión del archivo {item.Filename} no es válida.";
                        return false;
                    };
                }
                catch (Exception)
                {
                    messageFromValidExtensions = $"La extensión del archivo {item.Filename} no es válida.";
                    return false;
                }
            }

            return true;
        }

        private bool BeNotNullOrEmpty<T>(List<T> lista) {
            if (lista != null) {
                return lista.Any();
            }
            return false;
        }
    }
}
