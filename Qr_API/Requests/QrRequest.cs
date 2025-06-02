namespace Qr_API.Requests;

public class QrRequest
{
    public string Data { get; set; }
    public QrSize Size { get; set; } = QrSize.Medium;
    public QrFormat Format { get; set; } = QrFormat.PNG;
}

public enum QrSize
{
    Small = 100,
    Medium = 200,
    Large = 400,
    ExtraLarge = 800
}

public enum QrFormat
{
    PNG,
    SVG,
    Base64
}