using HunterWebServices.EmailService.Models;

namespace HunterWebServices.EmailService.Services;

public interface IEmailTemplateService
{
    EmailContent CreateEmail(MessageDetails details);
}

public class EmailContent
{
    public string Subject { get; set; }
    public string HtmlBody { get; set; }
    public string PlainBody { get; set; }
    public bool SendToAdmin { get; set; }
    public bool CcClient { get; set; }
}
