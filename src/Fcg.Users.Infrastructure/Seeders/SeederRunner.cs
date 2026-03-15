using System.Linq;

namespace Fcg.Users.Infrastructure.Seeders;

public sealed class SeederRunner : ISeederRunner
{
    private readonly IEnumerable<IDataSeeder> _seeders;

    public SeederRunner(IEnumerable<IDataSeeder> seeders)
    {
        _seeders = seeders;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var ordered = _seeders.OrderBy(s => s.Order).ToList();
        foreach (var seeder in ordered)
            await seeder.SeedAsync(cancellationToken).ConfigureAwait(false);
    }
}
