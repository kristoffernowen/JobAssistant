namespace JobAssistant.Application.Common.Entities;

public sealed class ImportWindow
{
    public Guid Id { get; set; }

    public DateTime FromUtc { get; set; }

    public DateTime ToUtc { get; set; }

    public string Location { get; set; } = string.Empty;

    public DateTime CreatedUtc { get; set; }
}
