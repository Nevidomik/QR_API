using Qr_API.Requests;
using Qr_API.Results;
using Qr_API.Services;

namespace Qr_API.Factories;

public enum QrStrategyType
{
    Default,
    Advanced,
    Mobile,
    Enterprise
}

public interface IQrStrategyFactory
{
    IQrStrategy CreateStrategy(QrStrategyType type);
    IQrStrategy CreateStrategy(QrRequest request); // Smart factory based on request
}

public class QrStrategyFactory : IQrStrategyFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<QrStrategyFactory> _logger;
    private readonly IQrGenerationService _qrService;

    public QrStrategyFactory(IServiceProvider serviceProvider, ILogger<QrStrategyFactory> logger, IQrGenerationService qrService)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _qrService = qrService;
    }

    public IQrStrategy CreateStrategy(QrStrategyType type)
    {
        _logger.LogInformation($"Creating strategy of type: {type}");
        
        return type switch
        {
            QrStrategyType.Default => new DefaultQrStrategy(_qrService, _serviceProvider.GetService<ILogger<DefaultQrStrategy>>()),
            QrStrategyType.Advanced => new AdvancedQrStrategy(_qrService, _serviceProvider.GetService<ILogger<AdvancedQrStrategy>>()),
            QrStrategyType.Mobile => new MobileQrStrategy(_qrService, _serviceProvider.GetService<ILogger<MobileQrStrategy>>()),
            QrStrategyType.Enterprise => new EnterpriseQrStrategy(_qrService, _serviceProvider.GetService<ILogger<EnterpriseQrStrategy>>()),
            _ => throw new ArgumentException($"Unknown strategy type: {type}")
        };
    }

    public IQrStrategy CreateStrategy(QrRequest request)
    {
        // Smart factory - chooses strategy based on request content
        if (string.IsNullOrEmpty(request.Data))
            return CreateStrategy(QrStrategyType.Default);
        
        if (request.Data.StartsWith("http"))
            return CreateStrategy(QrStrategyType.Advanced);
        
        if (request.Data.Length > 100)
            return CreateStrategy(QrStrategyType.Enterprise);
        
        return CreateStrategy(QrStrategyType.Mobile);
    }
}