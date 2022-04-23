using System;
using System.Net.Mail;
using System.Threading.Tasks;
using HunterWebServices.EmailService.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;

namespace HunterWebServices.EmailService
{
    public static class QueueEmail
    {
        [FunctionName("QueueEmail")]
        [return: ServiceBus(Constants.PendingEmailsQueue, Connection = Constants.HunterWebAppsServiceBus)]
        public static async Task<MessageDetails> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "SendEmail")] HttpRequest request)
        {
            var body = await request.ReadAsStringAsync();

            var details = JsonConvert.DeserializeObject<MessageDetails>(body);

            if (!IsValidEmail(details.Email))
            {
                throw new ArgumentException("The provided email is invalid", nameof(details.Email));
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
