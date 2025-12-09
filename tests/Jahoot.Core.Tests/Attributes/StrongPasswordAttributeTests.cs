using Jahoot.Core.Attributes;

namespace Jahoot.Core.Tests.Attributes;

public class StrongPasswordAttributeTests
{
    private StrongPasswordAttribute _attribute;

    [SetUp]
    public void Setup()
    {
        _attribute = new StrongPasswordAttribute();
    }

    [Test]
    public void IsValid_PasswordTooShort_ReturnsFalse()
    {
        const string password = "Short1!"; // Length 7

        bool result = _attribute.IsValid(password);

        Assert.That(result, Is.False);
    }

    [Test]
    public void IsValid_PasswordNoUppercase_ReturnsFalse()
    {
        const string password = "password1!"; // Length 10, no uppercase

        bool result = _attribute.IsValid(password);

        Assert.That(result, Is.False);
    }

    [Test]
    public void IsValid_PasswordNoLowercase_ReturnsFalse()
    {
        const string password = "PASSWORD1!"; // Length 10, no lowercase

        bool result = _attribute.IsValid(password);

        Assert.That(result, Is.False);
    }

    [Test]
    public void IsValid_PasswordNoDigit_ReturnsFalse()
    {
        const string password = "Password!!"; // Length 10, no digit

        bool result = _attribute.IsValid(password);

        Assert.That(result, Is.False);
    }

    [Test]
    public void IsValid_ValidPassword_ReturnsTrue()
    {
        const string password = "Password1!"; // Length 10, valid

        bool result = _attribute.IsValid(password);

        Assert.That(result, Is.True);
    }

    [Test]
    public void IsValid_ValidPasswordNoSpecialCharacters_ReturnsTrue()
    {
        const string password = "Password123"; // Length 11, valid, no special characters

        bool result = _attribute.IsValid(password);

        Assert.That(result, Is.True);
    }

    [Test]
    public void IsValid_NullValue_ReturnsFalse()
    {
        object? password = null;

        bool result = _attribute.IsValid(password);

        Assert.That(result, Is.False);
    }

    [Test]
    public void IsValid_NotStringValue_ReturnsFalse()
    {
        object value = 12345;

        bool result = _attribute.IsValid(value);

        Assert.That(result, Is.False);
    }

    [Test]
    public void ErrorMessage_IsCorrect()
    {
        const string expectedErrorMessage = "Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, and one number.";

        string? actualErrorMessage = _attribute.ErrorMessage;

        Assert.That(actualErrorMessage, Is.EqualTo(expectedErrorMessage));
    }
}
