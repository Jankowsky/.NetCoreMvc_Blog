using System.Collections.Generic;
using System.Threading.Tasks;
using Milka.Models;

namespace Milka.Settings
{
    public class EmailSettings
    {
        public string PrimaryDomain { get; set; }

        public int PrimaryPort { get; set; }

        public string SecondayDomain { get; set; }

        public int SecondaryPort { get; set; }

        public string UsernameEmail { get; set; }

        public string UsernamePassword { get; set; }

        public string FromEmail { get; set; }

        public string ToEmail { get; set; }

        public string CcEmail { get; set; }
    }
    
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
        Task SendEmailsWithTemplate(string title, Subscriber model, string template);
        Task SendEmailsWithTemplate(string title, List<Subscriber> subscribers, Post model, string template);
    }
}