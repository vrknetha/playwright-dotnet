namespace PlaywrightDemo.Infrastructure.Fixtures
{
    public interface ITestFixture
    {
        string FixtureKey { get; }
        Type FixtureType { get; }
    }
}