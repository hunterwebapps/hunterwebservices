using System;
using System.Net.Mail;
using System.Threading.Tasks;
using HunterWebServices.EmailService.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace HunterWebServices.EmailService
{
    public class QueueEmail
    {
        [FunctionName("QueueEmail")]
        [return: ServiceBus(Constants.PendingEmailsQueue, Connection = Constants.HunterWebAppsServiceBus)]
        public async Task<MessageDetails> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "SendEmail")] HttpRequest request,
            ILogger log)
        {
            var body = await request.ReadAsStringAsync();

            var details = JsonConvert.DeserializeObject<MessageDetails>(body);

            log.LogInformation("Queueing an email to {0}", details.Email);

            if (!details.IsSourceDomainValid(request))
            {
                log.LogError("Received invalid request URL for EmailType: {0}.", details.Type);

                throw new UnauthorizedAccessException("EmailType does not pass CORS restriction.");
            }

            if (!IsValidEmail(details.Email))
            {
                log.LogWarning("The provided email is invalid: {0}.", details.Email);

                throw new ArgumentException("The provided email is invalid", nameof(request));
            }

            return details;
        }

        private static bool IsValidEmail(string email)
        {
            var trimmedEmail = email.Trim();

            if (trimmedEmail.EndsWith("."))
            {
                return false;
            }

            try
            {
                var addr = new MailAddress(email);
                return addr.Address == trimmedEmail;
            }
            catch
            {
                return false;
            }
        }
    }
}
