using Microsoft.EntityFrameworkCore;

namespace Qr_API.DbContext;

public class QrDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public QrDbContext(DbContextOptions<QrDbContext> options) : base(options) { }
    public DbSet<QrCode> QrCodes { get; set; }
}

public class QrCode
{
    public int Id { get; set; }
    public string Data { get; set; }
    public bool Processed { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? QrImageBase64 { get; set; } // Зберігаємо QR-код як Base64
    public string? ErrorMessage { get; set; }
    public string? StrategyUsed { get; set; }
}