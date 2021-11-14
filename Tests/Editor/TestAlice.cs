namespace DependencyInjection.Tests.Editor
{
    public class TestAlice : ITestAlice
    {
        public string Foo()
        {
            return string.Empty;
        }
    }

    public interface ITestAlice
    {
        string Foo();
    }
}