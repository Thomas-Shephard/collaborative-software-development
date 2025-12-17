using Jahoot.Core.Attributes;

namespace Jahoot.Core.Tests.Attributes;

public class EnsureUniqueElementsAttributeTests
{
    private EnsureUniqueElementsAttribute _attribute;

    [SetUp]
    public void Setup()
    {
        _attribute = new EnsureUniqueElementsAttribute();
    }

    [Test]
    public void IsValid_NullValue_ReturnsTrue()
    {
        object? value = null;

        bool result = _attribute.IsValid(value);

        Assert.That(result, Is.True);
    }

    [Test]
    public void IsValid_NotIEnumerable_ReturnsTrue()
    {
        object value = 12345;

        bool result = _attribute.IsValid(value);

        Assert.That(result, Is.True);
    }

    [Test]
    public void IsValid_EmptyList_ReturnsTrue()
    {
        List<int> list = [];

        bool result = _attribute.IsValid(list);

        Assert.That(result, Is.True);
    }

    [Test]
    public void IsValid_ListWithUniqueElements_ReturnsTrue()
    {
        List<int> list = [1, 2, 3];

        bool result = _attribute.IsValid(list);

        Assert.That(result, Is.True);
    }

    [Test]
    public void IsValid_ListWithDuplicateElements_ReturnsFalse()
    {
        List<int> list = [1, 2, 1];

        bool result = _attribute.IsValid(list);

        Assert.That(result, Is.False);
    }

    [Test]
    public void IsValid_ListWithDuplicateStrings_ReturnsFalse()
    {
        List<string> list = ["a", "b", "a"];

        bool result = _attribute.IsValid(list);

        Assert.That(result, Is.False);
    }

    [Test]
    public void FormatErrorMessage_ReturnsCorrectMessage()
    {
        const string name = "SubjectIds";
        const string expectedMessage = $"The list {name} must not contain duplicate elements.";

        string result = _attribute.FormatErrorMessage(name);

        Assert.That(result, Is.EqualTo(expectedMessage));
    }
}
