using AuthService.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthService.UnitTests;

public abstract class TestBase
{
    protected static CancellationToken Ct => TestContext.Current.CancellationToken;

    protected static DataContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new DataContext(options);
    }
}