namespace Fcg.Users.Infrastructure.Seeders;

public interface ISeederRunner
{
    Task RunAsync(CancellationToken cancellationToken = default);
}
