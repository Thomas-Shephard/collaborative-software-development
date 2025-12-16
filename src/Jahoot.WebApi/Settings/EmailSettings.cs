using System.Diagnostics.CodeAnalysis;

namespace Jahoot.WebApi.Settings;

[SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Global")]
public class EmailSettings
{
    public required string Host { get; set; }
    public required int Port { get; set; }
    public required string User { get; set; }
    public required string Password { get; set; }
    public required string FromEmail { get; set; }
    public required string FromName { get; set; }
}
