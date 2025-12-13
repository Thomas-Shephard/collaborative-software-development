namespace Jahoot.WebApi.Tests;

[SetUpFixture]
public class GlobalTestSetup
{
    [OneTimeTearDown]
    public async Task RunAfterAnyTests()
    {
        await TestDatabaseFactory.DisposeAsync();
    }
}
