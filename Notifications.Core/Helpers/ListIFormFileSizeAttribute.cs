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
    public class ListIFormFileSizeAttribute: ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            IConfiguration _configuration = (IConfiguration) validationContext.GetService(typeof(IConfiguration));
            if (value == null) return ValidationResult.Success;
            var attachments = value as IEnumerable<IFormFile>;
            if(attachments == null) return ValidationResult.Success;

            var oneMbInBytes = 1048576;
            long defaultSizeInMegaBytes = 10;
            long.TryParse(_configuration["Files:SizeInMegaBytes"], out defaultSizeInMegaBytes);

            long defaultSizeInBytes = oneMbInBytes * defaultSizeInMegaBytes;

            long size = 0;
            foreach (var item in attachments) { 
                size = size + item.Length;
            }

            if (size > defaultSizeInBytes) {
                return new ValidationResult($"Los archivos adjuntos excenden el limete del tamaño de {defaultSizeInMegaBytes} MB");
            }

            return ValidationResult.Success;
        }
    }
}
