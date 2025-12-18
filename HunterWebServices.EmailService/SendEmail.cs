using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using HunterWebServices.EmailService.Models;
using HunterWebServices.EmailService.Services;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace HunterWebServices.EmailService
{
    public class SendEmail
    {
        private const string AdminEmail = "hunter@hunterwebapps.com";

        private readonly string sendgridApiKey;
        private readonly IEmailTemplateService emailTemplateService;

        public SendEmail(IConfiguration configuration, IEmailTemplateService emailTemplateService)
        {
            this.sendgridApiKey = configuration["SENDGRID-API-KEY"];
            this.emailTemplateService = emailTemplateService;
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

            var from = new EmailAddress(AdminEmail, "Hunter Web Apps");
            var to = new EmailAddress(details.Email, details.Name);

            var emailContent = emailTemplateService.CreateEmail(details);

            SendGridMessage mail;
            if (emailContent.SendToAdmin)
            {
                // Send to admin as primary recipient
                mail = MailHelper.CreateSingleEmail(from, from, emailContent.Subject, emailContent.PlainBody, emailContent.HtmlBody);

                if (emailContent.CcClient)
                {
                    mail.AddCc(to);
                }
            }
            else
            {
                // Send to client as primary recipient
                mail = MailHelper.CreateSingleEmail(from, to, emailContent.Subject, emailContent.PlainBody, emailContent.HtmlBody);
                mail.AddBcc(from);
            }

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
    }
}
