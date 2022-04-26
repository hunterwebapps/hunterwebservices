using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using HunterWebServices.EmailService.Models;
using System;
using SendGrid;
using SendGrid.Helpers.Mail;
using HunterWebServices.EmailService.EmailTemplates;

namespace HunterWebServices.EmailService
{
    public class SendEmail
    {
        private const string adminEmail = "hunter@hunterwebapps.com";

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

            var client = new SendGridClient(this.sendgridApiKey);

            var from = new EmailAddress(adminEmail, "Hunter Web Apps");
            var to = new EmailAddress(details.Email, details.Name);

            var subject = "I'll be in touch";
            var (htmlBody, plainBody) = details.Type switch
            {
                EmailType.PortfolioContact => MakePortfolioContactMessage(details),
                _ => throw new ArgumentException("Unhandled EmailType", nameof(details.Type)),
            };

            var mail = MailHelper.CreateSingleEmail(from, to, subject, plainBody, htmlBody);

            mail.AddBcc(from);

            Response response;
            try
            {
                response = await client.SendEmailAsync(mail);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "SendMailAsync Failed");
                return;
            }

            log.LogInformation("Email sent. Status Code: {0}", response.StatusCode);
        }

        private (string html, string plain) MakePortfolioContactMessage(MessageDetails details) =>
            (
                $@"Hey {details.Name},<br><br>
                This is Dwayne Hunter. I'm excited to hear from you. Someone will get back to you, probably myself, within 2-4 hours (on business days).<br><br>
                This is my direct email if you have any additional questions.<br><br>
                Original Message:<br>
                {details.Message}",
                $@"Hi {details.Name}. This is Dwayne Hunter. I'm excited to hear from you. Someone will get back to you, probably myself, within 2-4 hours (on business days). This is my direct email if you have any additional questions. Original Message: {details.Message}"
            );
    }
}
