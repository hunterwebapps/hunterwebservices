using HunterWebServices.EmailService.EmailTemplates;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace HunterWebServices.EmailService.Models;

public class MessageDetails
{
    public EmailType Type { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Message { get; set; }

    public static Dictionary<EmailType, List<string>> corsUrls = new Dictionary<EmailType, List<string>>()
    {
        [EmailType.PortfolioContact] = new List<string>()
        {
            "https://hunterwebapps.dev/",
            "http://localhost:8080/",
        },
    };

    public bool IsSourceDomainValid(HttpRequest request)
    {
        var referrer = new Uri(request.Headers["Referer"].ToString());
        return corsUrls[this.Type].Contains(referrer.AbsoluteUri);
    }
}
