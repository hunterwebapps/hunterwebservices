using System.Collections.Generic;

namespace HunterWebServices.EmailService.Models;

public class CategoryResult
{
    public string Name { get; set; }
    public int Score { get; set; }
    public int Max { get; set; }
    public int Percentage { get; set; }
    public List<QuestionResult> Questions { get; set; }
}

public class QuestionResult
{
    public string Text { get; set; }
    public int? Score { get; set; }
}
