using Jahoot.Core.Attributes;
using NUnit.Framework;

namespace Jahoot.Core.Tests.Attributes;

[TestFixture]
public class StrongPasswordAttributeTests
{
    private StrongPasswordAttribute _attribute;

    [SetUp]
    public void Setup()
    {
        _attribute = new StrongPasswordAttribute();
    }

    [Test]
    public void IsValid_Null_ReturnsFalse()
    {
        string? password = null;

        bool result = _attribute.IsValid(password);

        Assert.That(result, Is.False);
    }

    [Test]
    public void IsValid_NonString_ReturnsFalse()
    {
        object password = 123;

        bool result = _attribute.IsValid(password);

        Assert.That(result, Is.False);
    }

    [TestCase("Short1!", ExpectedResult = false, Description = "Too short")]
    [TestCase("nouppercase1!", ExpectedResult = false, Description = "No uppercase")]
    [TestCase("NOLOWERCASE1!", ExpectedResult = false, Description = "No lowercase")]
    [TestCase("NoDigit!", ExpectedResult = false, Description = "No digit")]
    [TestCase("Valid1Password", ExpectedResult = true, Description = "Valid")]
    [TestCase("AnotherValid1!", ExpectedResult = true, Description = "Valid with symbol")]
    public bool IsValid_Scenarios(string password)
    {
        return _attribute.IsValid(password);
    }
}
