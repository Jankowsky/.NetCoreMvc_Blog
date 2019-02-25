using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Milka.Models;
using Milka.Settings;
using RazorLight;


namespace Milka.Services
{
    public class EmailSender: Settings.IEmailSender
    {
        public EmailSender(IOptions<EmailSettings> emailSettings, IHostingEnvironment env)
        {
            _emailSettings = emailSettings.Value;
            _env = env;
        }

        public EmailSettings _emailSettings { get; }
        private IHostingEnvironment _env;

        public Task SendEmailAsync(string email, string subject, string message)
        {

            Execute (email, subject, message).Wait();
            return Task.FromResult(0);
        }

        public async Task SendEmailsWithTemplate(string title, List<Subscriber> subscribers, Post model, string template)
        {
            var path = Path.Combine(
                _env.WebRootPath, "Templates/");
           
            var engine = new RazorLightEngineBuilder()
                .UseFilesystemProject(path)
                .UseMemoryCachingProvider()
                .Build();

            string body = "";
            // send email to me to check what it looks like
             body = await engine.CompileRenderAsync(template, model);
             Execute("", title, body).Wait();
            
            if (subscribers.Count != 0)
            {
                // send to every subscriber email with template 
                foreach (var sub in subscribers)
                {
                    body = await engine.CompileRenderAsync(template, model);

                    Execute(Regex.Replace(sub.Email, "%40", "@"), title, body).Wait();
                }
            }
            
            

        }
        
        
        public async Task SendEmailsWithTemplate(string title, Subscriber model, string template)
        {
            var path = Path.Combine(
                _env.WebRootPath, "Templates/");

            var engine = new RazorLightEngineBuilder()
                .UseFilesystemProject(path)
                .UseMemoryCachingProvider()
                .Build();
            string body = "";

            if (model.Email != string.Empty)
            {
                // send to subscriber email with template 
              
                    body = await engine.CompileRenderAsync(template, model);

                Execute(Regex.Replace(model.Email, "%40", "@"), title, body).Wait();       
            }
        }

        public async Task Execute(string email, string subject, string message)
        {
            try
            {
                string toEmail = string.IsNullOrEmpty(email) ? _emailSettings.ToEmail : email;
                MailMessage mail = new MailMessage()
                {
                    From = new MailAddress(_emailSettings.FromEmail, "I Love Cook!")
                };
                mail.To.Add(new MailAddress(toEmail));
                mail.CC.Add(new MailAddress(_emailSettings.CcEmail));
                mail.Subject = "" + subject;
                mail.Body = message;
                mail.IsBodyHtml = true;
                mail.Priority = MailPriority.High;

                using (SmtpClient smtp = new SmtpClient(_emailSettings.SecondayDomain, _emailSettings.SecondaryPort))
                {
                    smtp.Credentials = new NetworkCredential(_emailSettings.UsernameEmail, _emailSettings.UsernamePassword);
                    smtp.EnableSsl = true;
                    await smtp.SendMailAsync(mail);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
   
}