using Qr_API.Requests;
using Qr_API.Results;
using Qr_API.Services;

namespace Qr_API.Factories;

public class EnterpriseQrStrategy : IQrStrategy
{
    private readonly IQrGenerationService _qrService;
    private readonly ILogger<EnterpriseQrStrategy> _logger;

    public EnterpriseQrStrategy(IQrGenerationService qrService, ILogger<EnterpriseQrStrategy> logger)
    {
        _qrService = qrService;
        _logger = logger;
    }

    public QrResult Process(QrRequest request)
    {
        try
        {
            _logger.LogInformation($"Processing QR with Enterprise strategy for data length: {request.Data?.Length}");

            // Complex enterprise validation
            if (string.IsNullOrEmpty(request.Data))
            {
                return new QrResult 
                { 
                    Success = false, 
                    ErrorMessage = "Data cannot be empty",
                    StrategyUsed = nameof(EnterpriseQrStrategy)
                };
            }

            if (request.Data.Length <= 10)
            {
                return new QrResult 
                { 
                    Success = false, 
                    ErrorMessage = "Enterprise data must be longer than 10 characters",
                    StrategyUsed = nameof(EnterpriseQrStrategy)
                };
            }

            if (request.Data.ToLower().Contains("test"))
            {
                return new QrResult 
                { 
                    Success = false, 
                    ErrorMessage = "Test data is not allowed in enterprise mode",
                    StrategyUsed = nameof(EnterpriseQrStrategy)
                };
            }

            // Enterprise: use larger size by default
            var size = request.Size == QrSize.Medium ? QrSize.Large : request.Size;
            var qrImageBase64 = _qrService.GenerateQrCodeBase64(request.Data, size, request.Format);
            
            return new QrResult 
            { 
                Success = true,
                QrImageBase64 = qrImageBase64,
                StrategyUsed = nameof(EnterpriseQrStrategy)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Enterprise strategy processing");
            return new QrResult 
            { 
                Success = false, 
                ErrorMessage = ex.Message,
                StrategyUsed = nameof(EnterpriseQrStrategy)
            };
        }
    }
}