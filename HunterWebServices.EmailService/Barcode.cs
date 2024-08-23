using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace HunterWebServices.EmailService;

public class Barcode
{
    [FunctionName("Barcode")]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "Barcode")]
        HttpRequest request,
        ILogger log)
    {
        log.LogInformation("C# HTTP trigger function processed a request.");
        return new OkObjectResult("Welcome to Azure Functions!");
    }
}
