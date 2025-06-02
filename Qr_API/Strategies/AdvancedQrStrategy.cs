using Qr_API.Requests;
using Qr_API.Results;
using Qr_API.Services;

namespace Qr_API.Factories;

public class AdvancedQrStrategy : IQrStrategy
{
    private readonly IQrGenerationService _qrService;
    private readonly ILogger<AdvancedQrStrategy> _logger;

    public AdvancedQrStrategy(IQrGenerationService qrService, ILogger<AdvancedQrStrategy> logger)
    {
        _qrService = qrService;
        _logger = logger;
    }

    public QrResult Process(QrRequest request)
    {
        try
        {
            _logger.LogInformation("Processing QR with Advanced strategy");

            // Advanced validation for URLs
            if (!Uri.TryCreate(request.Data, UriKind.Absolute, out var uri) || 
                (uri.Scheme != "http" && uri.Scheme != "https"))
            {
                return new QrResult 
                { 
                    Success = false, 
                    ErrorMessage = "Invalid URL format. Only HTTP/HTTPS URLs are supported",
                    StrategyUsed = nameof(AdvancedQrStrategy)
                };
            }

            var qrImageBase64 = _qrService.GenerateQrCodeBase64(request.Data, request.Size, request.Format);
            
            return new QrResult 
            { 
                Success = true,
                QrImageBase64 = qrImageBase64,
                StrategyUsed = nameof(AdvancedQrStrategy)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Advanced strategy processing");
            return new QrResult 
            { 
                Success = false, 
                ErrorMessage = ex.Message,
                StrategyUsed = nameof(AdvancedQrStrategy)
            };
        }
    }
}