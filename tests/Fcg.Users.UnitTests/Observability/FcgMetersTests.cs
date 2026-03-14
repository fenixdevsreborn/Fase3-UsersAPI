using Fcg.Users.Api.Observability;
using Xunit;

namespace Fcg.Users.UnitTests.Observability;

public class FcgMetersTests
{
    [Fact]
    public void Constructor_CreatesMeterWithName()
    {
        var meters = new FcgMeters("Fcg.Users.Api.Test");

        Assert.NotNull(meters.Meter);
        Assert.Equal("Fcg.Users.Api.Test", meters.Meter.Name);
    }

    [Fact]
    public void RecordUserCreated_DoesNotThrow()
    {
        var meters = new FcgMeters("Test");
        meters.RecordUserCreated();
    }

    [Fact]
    public void RecordUserDeleted_DoesNotThrow()
    {
        var meters = new FcgMeters("Test");
        meters.RecordUserDeleted();
    }

    [Fact]
    public void RecordException_DoesNotThrow()
    {
        var meters = new FcgMeters("Test");
        meters.RecordException("InvalidOperationException", 500);
    }
}
