using System;
using System.Linq;
using System.Text;
using HunterWebServices.EmailService.Models;

namespace HunterWebServices.EmailService.Services;

public class EmailTemplateService : IEmailTemplateService
{
    private static readonly string[] ScoreLabels = { "Manual", "Minimal", "Hybrid", "Mostly", "Full" };

    public EmailContent CreateEmail(MessageDetails details)
    {
        return details.Type switch
        {
            EmailType.PortfolioContact => CreatePortfolioContactEmail(details),
            EmailType.ScorecardSubmission => CreateScorecardSubmissionEmail(details),
            _ => throw new ArgumentException($"Unhandled EmailType: {details.Type}", nameof(details))
        };
    }

    private static EmailContent CreatePortfolioContactEmail(MessageDetails details)
    {
        var html = $@"Hey {details.Name},<br><br>
This is Dwayne Hunter. I'm excited to hear from you. Someone will get back to you, probably myself, within 2-4 hours (on business days).<br><br>
This is my direct email if you have any additional questions.<br><br>
Original Message:<br>
{details.Message}";

        var plain = $@"Hi {details.Name}. This is Dwayne Hunter. I'm excited to hear from you. Someone will get back to you, probably myself, within 2-4 hours (on business days). This is my direct email if you have any additional questions. Original Message: {details.Message}";

        return new EmailContent
        {
            Subject = "I'll be in touch",
            HtmlBody = html,
            PlainBody = plain,
            SendToAdmin = false,
            CcClient = false
        };
    }

    private static EmailContent CreateScorecardSubmissionEmail(MessageDetails details)
    {
        var maturityColor = GetMaturityColor(details.OverallScore ?? 0);
        var categorySections = BuildCategorySections(details);
        var (priorityHtml, priorityPlain) = BuildPriorityAreas(details);

        var html = BuildScorecardHtml(details, maturityColor, categorySections.html, priorityHtml);
        var plain = BuildScorecardPlainText(details, categorySections.plain, priorityPlain);

        return new EmailContent
        {
            Subject = $"Scorecard Submission: {details.Company ?? "Unknown Company"}",
            HtmlBody = html,
            PlainBody = plain,
            SendToAdmin = true,
            CcClient = true
        };
    }

    private static (string html, string plain) BuildCategorySections(MessageDetails details)
    {
        var htmlBuilder = new StringBuilder();
        var plainBuilder = new StringBuilder();

        if (details.CategoryResults == null)
            return (string.Empty, string.Empty);

        foreach (var category in details.CategoryResults)
        {
            var catColor = GetMaturityColor(category.Percentage);

            htmlBuilder.Append($@"
<div style=""margin-bottom: 24px; border: 1px solid #e9ecef; border-radius: 8px; overflow: hidden;"">
    <div style=""background: #f8f9fa; padding: 12px 16px; border-bottom: 1px solid #e9ecef;"">
        <div style=""display: flex; justify-content: space-between; align-items: center;"">
            <strong style=""color: #1a3a5c;"">{category.Name}</strong>
            <span style=""color: {catColor}; font-weight: bold;"">{category.Percentage}%</span>
        </div>
        <div style=""background: #e9ecef; height: 8px; border-radius: 4px; margin-top: 8px;"">
            <div style=""background: {catColor}; height: 8px; border-radius: 4px; width: {category.Percentage}%;""></div>
        </div>
    </div>
    <div style=""padding: 12px 16px;"">");

            plainBuilder.AppendLine($"\n{category.Name}: {category.Percentage}% ({category.Score}/{category.Max})");

            if (category.Questions != null)
            {
                foreach (var question in category.Questions)
                {
                    var scoreDisplay = question.Score.HasValue ? question.Score.Value.ToString() : "-";
                    var scoreLabel = question.Score.HasValue ? ScoreLabels[question.Score.Value] : "Not answered";
                    var scoreColor = question.Score.HasValue ? GetScoreColor(question.Score.Value) : "#6c757d";

                    htmlBuilder.Append($@"
        <div style=""padding: 8px 0; border-bottom: 1px solid #f0f0f0;"">
            <div style=""display: flex; align-items: flex-start;"">
                <span style=""display: inline-block; min-width: 28px; height: 28px; line-height: 28px; text-align: center; background: {scoreColor}; color: white; border-radius: 4px; font-weight: bold; margin-right: 12px;"">{scoreDisplay}</span>
                <div>
                    <div style=""color: #333;"">{question.Text}</div>
                    <div style=""color: #6c757d; font-size: 12px;"">{scoreLabel}</div>
                </div>
            </div>
        </div>");

                    plainBuilder.AppendLine($"  [{scoreDisplay}] {question.Text}");
                }
            }

            htmlBuilder.Append(@"
    </div>
</div>");
        }

        return (htmlBuilder.ToString(), plainBuilder.ToString());
    }

    private static (string html, string plain) BuildPriorityAreas(MessageDetails details)
    {
        var htmlBuilder = new StringBuilder();
        var plainBuilder = new StringBuilder();

        if (details.CategoryResults == null || details.CategoryResults.Count == 0)
            return (string.Empty, string.Empty);

        var sorted = details.CategoryResults
            .OrderBy(c => c.Percentage)
            .Take(3)
            .ToList();

        htmlBuilder.Append(@"<ol style=""margin: 0; padding-left: 20px;"">");
        plainBuilder.AppendLine("\nPRIORITY AREAS:");

        for (int i = 0; i < sorted.Count; i++)
        {
            var cat = sorted[i];
            htmlBuilder.Append($@"<li style=""margin-bottom: 8px;""><strong>{cat.Name}</strong> ({cat.Percentage}%)</li>");
            plainBuilder.AppendLine($"{i + 1}. {cat.Name} ({cat.Percentage}%)");
        }

        htmlBuilder.Append("</ol>");

        return (htmlBuilder.ToString(), plainBuilder.ToString());
    }

    private static string BuildScorecardHtml(MessageDetails details, string maturityColor, string categorySections, string priorityHtml)
    {
        var phoneHtml = string.IsNullOrEmpty(details.Phone) ? "" : $"<strong>Phone:</strong> {details.Phone}<br>";

        return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
</head>
<body style=""margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f4f4f4;"">
    <div style=""max-width: 600px; margin: 0 auto; background: white;"">
        <!-- Header -->
        <div style=""background: linear-gradient(135deg, #1a3a5c 0%, #2d5a87 100%); color: white; padding: 32px; text-align: center;"">
            <h1 style=""margin: 0 0 8px 0; font-size: 24px;"">Spreadsheet-to-System Scorecard</h1>
            <p style=""margin: 0; opacity: 0.9;"">Assessment Results</p>
        </div>

        <!-- Company Info -->
        <div style=""background: #f8f9fa; padding: 20px 32px; border-bottom: 1px solid #e9ecef;"">
            <h2 style=""margin: 0 0 12px 0; color: #1a3a5c; font-size: 20px;"">{details.Company ?? "Company Not Provided"}</h2>
            <p style=""margin: 0; color: #666;"">
                <strong>Email:</strong> {details.Email}<br>
                {phoneHtml}
            </p>
        </div>

        <!-- Overall Score -->
        <div style=""text-align: center; padding: 32px;"">
            <table style=""margin: 0 auto;"" cellpadding=""0"" cellspacing=""0"" border=""0"">
                <tr>
                    <td style=""width: 120px; height: 120px; border-radius: 50%; background: {maturityColor}; color: white; font-size: 48px; font-weight: bold; text-align: center; vertical-align: middle;"">{details.OverallScore ?? 0}</td>
                </tr>
            </table>
            <div style=""color: #666; margin-top: 8px;"">/ 100</div>
            <div style=""display: inline-block; background: {maturityColor}; color: white; padding: 8px 24px; border-radius: 20px; margin-top: 16px; font-weight: bold;"">{details.MaturityLevel ?? "Unknown"}</div>
        </div>

        <!-- Score Legend -->
        <div style=""background: #f8f9fa; padding: 16px 32px; margin: 0 32px 24px 32px; border-radius: 8px;"">
            <div style=""font-weight: bold; margin-bottom: 8px; color: #1a3a5c;"">Score Legend</div>
            <div style=""display: flex; flex-wrap: wrap; gap: 8px; font-size: 12px;"">
                <span style=""background: #dc3545; color: white; padding: 4px 8px; border-radius: 4px;"">0 = Manual</span>
                <span style=""background: #fd7e14; color: white; padding: 4px 8px; border-radius: 4px;"">1 = Minimal</span>
                <span style=""background: #ffc107; color: #333; padding: 4px 8px; border-radius: 4px;"">2 = Hybrid</span>
                <span style=""background: #0d6efd; color: white; padding: 4px 8px; border-radius: 4px;"">3 = Mostly</span>
                <span style=""background: #198754; color: white; padding: 4px 8px; border-radius: 4px;"">4 = Full</span>
            </div>
        </div>

        <!-- Category Breakdown -->
        <div style=""padding: 0 32px 32px 32px;"">
            <h3 style=""color: #1a3a5c; border-bottom: 2px solid #ec8b5e; padding-bottom: 8px;"">Category Breakdown</h3>
            {categorySections}
        </div>

        <!-- Priority Areas -->
        <div style=""background: #fff3cd; padding: 24px 32px; margin: 0 32px 32px 32px; border-radius: 8px; border-left: 4px solid #ffc107;"">
            <h4 style=""margin: 0 0 12px 0; color: #856404;"">Priority Areas for Improvement</h4>
            {priorityHtml}
        </div>

        <!-- Footer -->
        <div style=""background: #1a3a5c; color: white; padding: 24px 32px; text-align: center;"">
            <p style=""margin: 0 0 8px 0;"">We'll review your results and be in touch within 24 hours.</p>
            <p style=""margin: 0; opacity: 0.8; font-size: 14px;"">
                Hunter Web Apps | <a href=""mailto:hunter@hunterwebapps.com"" style=""color: #ec8b5e;"">hunter@hunterwebapps.com</a> | (407) 349-3633
            </p>
        </div>
    </div>
</body>
</html>";
    }

    private static string BuildScorecardPlainText(MessageDetails details, string categoryPlain, string priorityPlain)
    {
        return $@"SCORECARD SUBMISSION
====================

Company: {details.Company ?? "Not provided"}
Email: {details.Email}
Phone: {details.Phone ?? "Not provided"}

OVERALL RESULTS
---------------
Overall Score: {details.OverallScore ?? 0}/100
Maturity Level: {details.MaturityLevel ?? "Unknown"}

CATEGORY BREAKDOWN
------------------
{categoryPlain}
{priorityPlain}

---
We'll review your results and be in touch within 24 hours.
Hunter Web Apps | hunter@hunterwebapps.com | (407) 349-3633";
    }

    private static string GetMaturityColor(int score)
    {
        if (score < 25) return "#dc3545";
        if (score < 50) return "#fd7e14";
        if (score < 75) return "#0d6efd";
        return "#198754";
    }

    private static string GetScoreColor(int score)
    {
        return score switch
        {
            0 => "#dc3545",
            1 => "#fd7e14",
            2 => "#ffc107",
            3 => "#0d6efd",
            4 => "#198754",
            _ => "#6c757d"
        };
    }
}
