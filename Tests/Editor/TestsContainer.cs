using DependencyInjection.Runtime;
using DependencyInjection.Tests.Editor;
using Moq;
using NUnit.Framework;
using UnityEngine;

namespace DependencyInjection
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
            container.RegisterSingleton<TestAlice>();
            container.RegisterSingleton<TestBob>();
            container.RegisterSingleton<TestCharlie>();

            var obj = container.Get<TestCharlie>();

            Assert.NotNull(obj);
            Assert.NotNull(obj.Alice);
            Assert.NotNull(obj.Bob);

            Assert.AreEqual(typeof(TestAlice), obj.Alice.GetType());
            Assert.AreEqual(typeof(TestBob), obj.Bob.GetType());
        }

        [Test]
        public void TestInvalid()
        {
            var container = new Container
            {
                AutoResolve = false
            };

            // invalid cast, different namespaces
            container.Register<ILogger, Logger>();
            var obj = container.Get<Castle.Core.Logging.ILogger>();
            Assert.Null(obj);
        }

        [Test]
        public void TestInjectionNested()
        {
            var container = new Container();
            container.RegisterSingleton<TestAlice>();
            container.RegisterSingleton<TestBob>();
            container.RegisterSingleton<TestCharlie>();
            container.RegisterSingleton<TestDave>();

            var obj = container.Get<TestDave>();

            Assert.NotNull(obj);
            Assert.NotNull(obj.Charlie);
            Assert.NotNull(obj.Charlie.Alice);
            Assert.NotNull(obj.Charlie.Bob);
        }

        [Test]
        public void TestInjectionInterface()
        {
            var container = new Container();
            container.Register<ITestAlice, TestAlice>();
            container.RegisterSingleton<TestEric>();

            var obj = container.Get<TestEric>();

            Assert.NotNull(obj);
            Assert.NotNull(obj.Alice);
        }

        [Test]
        public void TestRegister()
        {
            var container = new Container();
            container.RegisterSingleton<TestAlice>();
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
        public void TestScope()
        {
            var container = new Container();
            var a1 = container.Get<TestAlice>();
            var a2 = container.Get<TestAlice>();
            Assert.That(a1, Is.Not.EqualTo(a2));

            container.RegisterSingleton<TestBob>();
            var b1 = container.Get<TestBob>();
            var b2 = container.Get<TestBob>();
            Assert.That(b1, Is.EqualTo(b2));

            container.Register<ITestEric, TestEric>();
            var c1 = container.Get<ITestEric>();
            var c2 = container.Get<ITestEric>();
            Assert.That(c1, Is.Not.EqualTo(c2));

            container.RegisterSingleton<ITestEric, TestEric>();
            var d1 = container.Get<ITestEric>();
            var d2 = container.Get<ITestEric>();
            Assert.That(d1, Is.EqualTo(d2));
        }

        [Test]
        public void TestVerify()
        {
            var container = new Container();
            container.RegisterSingleton<TestAlice>();
            container.RegisterSingleton<TestBob>();
            container.RegisterSingleton<TestCharlie>();
            container.RegisterSingleton<TestDave>();

            Assert.True(container.Verify());
        }

        [Test]
        public void TestMoq()
        {
            var alice = new Mock<ITestAlice>();
            alice.Setup(t => t.Foo())
                .Returns("foo");

            var container = new Container();
            container.RegisterObjectToInterface<ITestAlice, TestAlice>(alice.Object);

            var obj = container.Get<TestEric>();
            Assert.NotNull(obj);
            Assert.NotNull(obj.Alice);
            Assert.AreEqual("foo", obj.Alice.Foo());
        }
    }
}