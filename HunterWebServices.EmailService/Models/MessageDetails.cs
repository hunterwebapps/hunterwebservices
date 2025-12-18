using System.Collections.Generic;

namespace HunterWebServices.EmailService.Models;

public class MessageDetails
{
    public EmailType Type { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Message { get; set; }

    // Scorecard-specific fields
    public string Company { get; set; }
    public string Phone { get; set; }
    public int? OverallScore { get; set; }
    public string MaturityLevel { get; set; }
    public List<CategoryResult> CategoryResults { get; set; }
}
