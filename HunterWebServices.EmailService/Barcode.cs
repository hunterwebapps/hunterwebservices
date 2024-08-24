using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using System.IO;
using System.Linq;
using ZXing;
using ZXing.Common;

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

        var writer = new BarcodeWriterPixelData()
        {
            Format = BarcodeFormat.CODE_128,
            Options = new EncodingOptions()
            {
                PureBarcode = true,
                Height = 100,
                Width = 300,
            },
        };
        var image = writer.WriteAsImageSharp<SixLabors.ImageSharp.PixelFormats.Rgba32>(barcodeValue);
        using var memoryStream = new MemoryStream();
        image.SaveAsPng(memoryStream);

        return new FileContentResult(memoryStream.ToArray(), "image/png");
    }
}
