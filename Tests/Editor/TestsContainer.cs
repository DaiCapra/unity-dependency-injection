using DependencyInjection.Runtime;
using NUnit.Framework;

namespace DependencyInjection.Tests.Editor
{
    public class TestsContainer
    {
        [Test]
        public void TestAutoResolve()
        {
            var container = new Container()
            {
                AutoResolve = true
            };

            var obj = container.Get<TestCharlie>();
            Assert.NotNull(obj);
            Assert.NotNull(obj.Alice);
            Assert.NotNull(obj.Bob);
        }

        [Test]
        public void TestInjection()
        {
            var container = new Container();
            container.Register<TestAlice>();
            container.Register<TestBob>();
            container.Register<TestCharlie>();

            var obj = container.Get<TestCharlie>();

            Assert.NotNull(obj);
            Assert.NotNull(obj.Alice);
            Assert.NotNull(obj.Bob);

            Assert.AreEqual(typeof(TestAlice), obj.Alice.GetType());
            Assert.AreEqual(typeof(TestBob), obj.Bob.GetType());
        }

        [Test]
        public void TestInjectionNested()
        {
            var container = new Container();
            container.Register<TestAlice>();
            container.Register<TestBob>();
            container.Register<TestCharlie>();
            container.Register<TestDave>();

            var obj = container.Get<TestDave>();

            Assert.NotNull(obj);
            Assert.NotNull(obj.Charlie);
            Assert.NotNull(obj.Charlie.Alice);
            Assert.NotNull(obj.Charlie.Bob);
        }

        [Test]
        public void TestRegister()
        {
            var container = new Container();
            container.Register<TestAlice>();
            TestAlice obj = container.Get<TestAlice>();

            Assert.NotNull(obj);
            Assert.AreEqual(typeof(TestAlice), obj.GetType());
        }

        [Test]
        public void TestRegisterInterface()
        {
            var container = new Container();
            container.Register<ITestAlice, TestAlice>();
            ITestAlice obj = container.Get<ITestAlice>();

            Assert.NotNull(obj);
        }

        [Test]
        public void TestVerify()
        {
            var container = new Container();
            container.Register<TestAlice>();
            container.Register<TestBob>();
            container.Register<TestCharlie>();
        }
    }
}