using HunterWebServices.EmailService.EmailTemplates;
using System.Net.Mail;

namespace HunterWebServices.EmailService.Models;

public class MessageDetails
{
    public EmailType Type { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Message { get; set; }
}
