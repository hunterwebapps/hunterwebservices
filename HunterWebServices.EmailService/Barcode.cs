using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using IronBarCode;

namespace HunterWebServices.EmailService;

public class Barcode
{
    [FunctionName("Barcode")]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "Barcode")]
        HttpRequest request,
        ILogger log)
    {
        var bolNumber = request.Query["bolNumber"];
        var barcode = BarcodeWriter.CreateBarcode(bolNumber, BarcodeWriterEncoding.Code128);
        return new FileContentResult(barcode.ToPngBinaryData(), "image/png");
    }
}
