namespace Fcg.Users.Infrastructure.Seeders;

public interface IDataSeeder
{
    int Order { get; }
    Task SeedAsync(CancellationToken cancellationToken = default);
}
