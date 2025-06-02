namespace Qr_API.Results;

public class QrResult
{
    public bool Success { get; set; }
    public string? QrImageBase64 { get; set; }
    public string? ErrorMessage { get; set; }
    public string? StrategyUsed { get; set; }
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}