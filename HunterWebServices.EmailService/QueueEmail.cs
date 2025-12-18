using System;
using System.Net.Mail;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using HunterWebServices.EmailService.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace HunterWebServices.EmailService
{
    public class QueueEmail
    {
        private readonly string serviceBusConnectionString;

        public QueueEmail(IConfiguration configuration)
        {
            this.serviceBusConnectionString = configuration[Constants.HunterWebAppsServiceBus];
        }

        [FunctionName("QueueEmail")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "SendEmail")] HttpRequest request,
            ILogger log)
        {
            var body = await request.ReadAsStringAsync();

            var details = JsonConvert.DeserializeObject<MessageDetails>(body);

            log.LogInformation("Queueing an email to {0}", details.Email);

            if (!IsValidEmail(details.Email))
            {
                log.LogWarning("The provided email is invalid: {0}.", details.Email);

                return new BadRequestObjectResult("The provided email is invalid");
            }

            await using var client = new ServiceBusClient(serviceBusConnectionString);
            await using var sender = client.CreateSender(Constants.PendingEmailsQueue);

            var message = new ServiceBusMessage(JsonConvert.SerializeObject(details));
            await sender.SendMessageAsync(message);

            return new OkResult();
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
