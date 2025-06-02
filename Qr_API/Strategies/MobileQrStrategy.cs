using Qr_API.Requests;
using Qr_API.Results;
using Qr_API.Services;

namespace Qr_API.Factories;

public class MobileQrStrategy : IQrStrategy
{
    private readonly IQrGenerationService _qrService;
    private readonly ILogger<MobileQrStrategy> _logger;

    public MobileQrStrategy(IQrGenerationService qrService, ILogger<MobileQrStrategy> logger)
    {
        _qrService = qrService;
        _logger = logger;
    }

    public QrResult Process(QrRequest request)
    {
        try
        {
            _logger.LogInformation("Processing QR with Mobile strategy");

            if (string.IsNullOrEmpty(request.Data))
            {
                return new QrResult 
                { 
                    Success = false, 
                    ErrorMessage = "Data cannot be empty",
                    StrategyUsed = nameof(MobileQrStrategy)
                };
            }

            if (request.Data.Length > 50)
            {
                return new QrResult 
                { 
                    Success = false, 
                    ErrorMessage = "Data too long for mobile optimization (max 50 characters)",
                    StrategyUsed = nameof(MobileQrStrategy)
                };
            }

            // Mobile-optimized: smaller size by default
            var size = request.Size == QrSize.Medium ? QrSize.Small : request.Size;
            var qrImageBase64 = _qrService.GenerateQrCodeBase64(request.Data, size, request.Format);
            
            return new QrResult 
            { 
                Success = true,
                QrImageBase64 = qrImageBase64,
                StrategyUsed = nameof(MobileQrStrategy)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Mobile strategy processing");
            return new QrResult 
            { 
                Success = false, 
                ErrorMessage = ex.Message,
                StrategyUsed = nameof(MobileQrStrategy)
            };
        }
    }
}