using DependencyInjection.Runtime;

namespace DependencyInjection.Tests.Editor
{
    public class TestCharlie
    {
        [Inject] public TestAlice Alice { get; set; }
        [Inject] public TestBob Bob;
    }
}