namespace Wrblo.SAL.Core.Implementation.Services;

public sealed class GoogleOptions
{
    public string ClientId { get; set; } = string.Empty;

    public string[] ClientIds { get; set; } = [];

    public IReadOnlyCollection<string> GetAudienceIds() =>
        ClientIds
            .Append(ClientId)
            .Where(clientId => !string.IsNullOrWhiteSpace(clientId))
            .Select(clientId => clientId.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToArray();
}