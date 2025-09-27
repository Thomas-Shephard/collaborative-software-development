using Jahoot.Core;

namespace Jahoot.Tests;

public class TestClassTests
{

    [Test]
    public void TestClass_TestMethod_Returns1()
    {
        Assert.That((new TestClass()).TestMethod(), Is.EqualTo(1));
    }
}
