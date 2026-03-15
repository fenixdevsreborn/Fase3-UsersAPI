using System.Diagnostics;

namespace Fcg.Users.Api.Observability;

/// <summary>ActivitySource padronizado para o serviço Users API. Nome e versão usados em OpenTelemetry.</summary>
public static class FcgActivitySource
{
    public const string Name = "Fcg.Users.Api";
    public const string Version = "1.0.0";

    private static readonly ActivitySource Source = new(Name, Version);

    public static ActivitySource Instance => Source;
}
