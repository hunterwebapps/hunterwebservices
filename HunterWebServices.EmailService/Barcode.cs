using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using IronBarCode;
using System.Linq;

namespace HunterWebServices.EmailService;

public class Barcode
{
    [FunctionName("Barcode")]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "Barcode")]
        HttpRequest request,
        ILogger log)
    {
        var barcodeValue = request.Query["value"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(barcodeValue) || barcodeValue.Length > 48)
        {
            return new BadRequestObjectResult("Please provide a barcode value with max 48 characters.");
        }

        var barcode = BarcodeWriter.CreateBarcode(barcodeValue, BarcodeWriterEncoding.Code128);
        return new FileContentResult(barcode.ToPngBinaryData(), "image/png");
    }
}
