using DependencyInjection.Runtime;

namespace DependencyInjection.Tests.Editor
{
    public class TestEric : ITestEric
    {
        [Inject] public ITestAlice Alice;
    }

    public interface ITestEric
    {
    }
}