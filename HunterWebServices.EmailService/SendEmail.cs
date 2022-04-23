using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using HunterWebServices.EmailService.Models;
using System.Net.Mail;
using System.Net;

namespace HunterWebServices.EmailService
{
    public class SendEmail
    {
        private readonly string sendgridApiKey;

        public SendEmail(IConfiguration configuration)
        {
            this.sendgridApiKey = configuration["SENDGRID-API-KEY"];
        }

        [FunctionName(nameof(SendEmail))]
        public async Task Run(
            [ServiceBusTrigger(
                queueName: Constants.PendingEmailsQueue,
                Connection = Constants.HunterWebAppsServiceBus)] MessageDetails details,
            ILogger log)
        {
            log.LogInformation("Sending an email to {0}.", details.Email);

            var smtp = new SmtpClient("smtp.sendgrid.net", 465)
            {
                Credentials = new NetworkCredential("apikey", this.sendgridApiKey),
            };

            var from = new MailAddress("hunter@hunterwebapps.com", "Hunter Web Apps");
            var bcc = new MailAddress("dwaynewhunter@gmail.com", "Dwayne Hunter");
            var to = new MailAddress(details.Email, details.Name);

            var mail = new MailMessage(from, to)
            {
                IsBodyHtml = true,
                Subject = "I'll be in touch",
                Body = $@"Hi {details.Name},<br><br>
                I'm excited to hear from you. I'll get back to you within the next 12 hours.<br><br>
                This is my direct email if you have any additional questions.<br><br>
                Original Message:<br>
                {details.Message}",
                Bcc = { bcc },
            };

            await smtp.SendMailAsync(mail);

            log.LogInformation("Email sent.");
        }
    }
}
