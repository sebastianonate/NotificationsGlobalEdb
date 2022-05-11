using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notifications.Core.Helpers
{
    public class ListIFormFileFormatAttribute: ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            IConfiguration _configuration = (IConfiguration) validationContext.GetService(typeof(IConfiguration));
            if (value == null) return ValidationResult.Success;
            var attachments = value as IEnumerable<IFormFile>;
            if(attachments == null) return ValidationResult.Success;

            var validExtensions = _configuration["Files:ValidExtensions"].Trim().Split(',');

            foreach (var item in attachments) {
                try
                {
                    var splitFileArray = item.FileName.Split('.');
                    var extension = splitFileArray[splitFileArray.Length - 1];
                    if (validExtensions.FirstOrDefault(x=> x.Trim() == extension) == null)
                    {
                        return new ValidationResult($"La extensión del archivo {item.FileName} no es válida.");
                    };
                }
                catch (Exception)
                {
                    return new ValidationResult($"La extensión del archivo {item.FileName} no es válida.");
                }
            }

            return ValidationResult.Success;
        }
    }
}
