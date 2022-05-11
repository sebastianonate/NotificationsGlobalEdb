using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Notifications.Core.Helpers
{
    public static class MailHelper
    {
        public static Boolean IsValidEmail(String email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;
            String expresion;
            expresion = "\\w+([-+.']\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*";
            if (Regex.IsMatch(email, expresion))
            {
                return Regex.Replace(email, expresion, String.Empty).Length == 0;
            }
            else
            {
                return false;
            }
        }

        public static List<string> GetValidEmails(List<string> emails, out List<string> rejected)
        {

            var validEmails = new List<string>();
            var emailsReject = new List<string>();
            emails.ForEach(email => {
                if (!string.IsNullOrWhiteSpace(email)) { 
                    var trimEmail = email.Trim();
                    if (MailHelper.IsValidEmail(trimEmail))
                        validEmails.Add(trimEmail);
                    else
                        emailsReject.Add(trimEmail);
                }
            });
            rejected = emailsReject;

            return validEmails;
        }
    }
}
