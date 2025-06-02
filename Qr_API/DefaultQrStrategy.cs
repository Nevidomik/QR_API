using Qr_API.Requests;
using Qr_API.Results;
using Qr_API.Services;

namespace Qr_API;

public interface IQrStrategy
{
    QrResult Process(QrRequest request);
}

public class DefaultQrStrategy : IQrStrategy
{
    private readonly IQrGenerationService _qrService;
    private readonly ILogger<DefaultQrStrategy> _logger;

    public DefaultQrStrategy(IQrGenerationService qrService, ILogger<DefaultQrStrategy> logger)
    {
        _qrService = qrService;
        _logger = logger;
    }

    public QrResult Process(QrRequest request)
    {
        try
        {
            _logger.LogInformation("Processing QR with Default strategy");
            
            if (string.IsNullOrEmpty(request.Data))
            {
                return new QrResult 
                { 
                    Success = false, 
                    ErrorMessage = "Data cannot be empty",
                    StrategyUsed = nameof(DefaultQrStrategy)
                };
            }

            var qrImageBase64 = _qrService.GenerateQrCodeBase64(request.Data, request.Size, request.Format);
            
            return new QrResult 
            { 
                Success = true,
                QrImageBase64 = qrImageBase64,
                StrategyUsed = nameof(DefaultQrStrategy)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Default strategy processing");
            return new QrResult 
            { 
                Success = false, 
                ErrorMessage = ex.Message,
                StrategyUsed = nameof(DefaultQrStrategy)
            };
        }
    }
}