using QRCoder;
using Qr_API.Requests;
using System.Drawing;
using System.Drawing.Imaging;

namespace Qr_API.Services;

public interface IQrGenerationService
{
    string GenerateQrCodeBase64(string data, QrSize size = QrSize.Medium, QrFormat format = QrFormat.PNG);
    byte[] GenerateQrCodeBytes(string data, QrSize size = QrSize.Medium, QrFormat format = QrFormat.PNG);
    // string GenerateQrCodeSvg(string data, QrSize size = QrSize.Medium);
}

public class QrGenerationService : IQrGenerationService
{
    private readonly ILogger<QrGenerationService> _logger;

    public QrGenerationService(ILogger<QrGenerationService> logger)
    {
        _logger = logger;
    }

    public string GenerateQrCodeBase64(string data, QrSize size = QrSize.Medium, QrFormat format = QrFormat.PNG)
    {
        try
        {
            _logger.LogInformation($"Generating QR code for data: {data?.Substring(0, Math.Min(data?.Length ?? 0, 50))}...");

            // if (format == QrFormat.SVG)
            // {
            //     return GenerateQrCodeSvg(data, size);
            // }

            var bytes = GenerateQrCodeBytes(data, size, format);
            return Convert.ToBase64String(bytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error generating QR code for data: {data}");
            throw;
        }
    }

    public byte[] GenerateQrCodeBytes(string data, QrSize size = QrSize.Medium, QrFormat format = QrFormat.PNG)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
        
        // if (format == QrFormat.SVG)
        // {
        //    // var svgQrCode = new SvgQRCode(qrCodeData);
        //     var svgString = svgQrCode.GetGraphic((int)size);
        //     return System.Text.Encoding.UTF8.GetBytes(svgString);
        // }

        using var qrCode = new PngByteQRCode(qrCodeData);
        return qrCode.GetGraphic((int)size / 10); // Pixel per module
    }

    // public string GenerateQrCodeSvg(string data, QrSize size = QrSize.Medium)
    // {
    //     using var qrGenerator = new QRCodeGenerator();
    //     using var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
    //     using var qrCode = new SvgQRCode(qrCodeData);
        
    //     return qrCode.GetGraphic((int)size, "#000000", "#FFFFFF");
    // }
}