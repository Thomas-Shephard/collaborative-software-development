using Jahoot.WebApi.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Jahoot.WebApi.Tests.Extensions;

public class ServiceCollectionExtensionsTests
{
    private Mock<IServiceCollection> _servicesMock;

    [SetUp]
    public void Setup()
    {
        _servicesMock = new Mock<IServiceCollection>();
    }

    // --- Helper Classes for Testing ---
    public class TestSettings
    {
        public string SomeString { get; set; } = string.Empty;
        public int SomeInt { get; set; }
        public bool SomeBool { get; set; }
    }

    public class TestSnakeCaseSettings
    {
        public string ThisIsCamelCase { get; set; } = string.Empty;
    }

    public class TestSettingsWithReadOnly
    {
        public string Writeable { get; set; } = string.Empty;
        public string ReadOnly => "Can't touch this";
    }

    // --- Tests for AddAndConfigure ---

    [Test]
    public void AddAndConfigure_ValidConfiguration_RegistersSettings()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>
        {
            {"MySettings:SomeString", "TestValue"},
            {"MySettings:SomeInt", "123"},
            {"MySettings:SomeBool", "true"}
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        // Act
        var result = _servicesMock.Object.AddAndConfigure<TestSettings>(configuration, "MySettings");

        // Assert
        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.SomeString, Is.EqualTo("TestValue"));
            Assert.That(result.SomeInt, Is.EqualTo(123));
            Assert.That(result.SomeBool, Is.True);
        }

        // Verify that AddSingleton was called with the settings object
        _servicesMock.Verify(s => s.Add(It.Is<ServiceDescriptor>(d => 
            d.ServiceType == typeof(TestSettings) && 
            d.Lifetime == ServiceLifetime.Singleton && 
            d.ImplementationInstance == result)), Times.Once);
    }

    [Test]
    public void AddAndConfigure_MissingSection_ThrowsInvalidOperationException()
    {
        // Arrange
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>()) // Empty config
            .Build();

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => 
            _servicesMock.Object.AddAndConfigure<TestSettings>(configuration, "NonExistentSection"));
        
        Assert.That(ex.Message, Is.EqualTo("NonExistentSection is not configured."));
    }

    // --- Tests for AddAndConfigureFromEnv ---

    [Test]
    public void AddAndConfigureFromEnv_ValidEnvVars_RegistersSettings()
    {
        // Arrange
        var prefix = "TESTAPP";
        var inMemorySettings = new Dictionary<string, string?>
        {
            {$"{prefix}_SOME_STRING", "EnvValue"},
            {$"{prefix}_SOME_INT", "456"},
            {$"{prefix}_SOME_BOOL", "false"}
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        // Act
        var result = _servicesMock.Object.AddAndConfigureFromEnv<TestSettings>(configuration, prefix);

        // Assert
        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.SomeString, Is.EqualTo("EnvValue"));
            Assert.That(result.SomeInt, Is.EqualTo(456));
            Assert.That(result.SomeBool, Is.False);
        }

        _servicesMock.Verify(s => s.Add(It.Is<ServiceDescriptor>(d => 
            d.ServiceType == typeof(TestSettings) && 
            d.Lifetime == ServiceLifetime.Singleton && 
            d.ImplementationInstance == result)), Times.Once);
    }

    [Test]
    public void AddAndConfigureFromEnv_MissingEnvVar_ThrowsInvalidOperationException()
    {
        // Arrange
        var prefix = "TESTAPP";
        // Only provide one value, missing others
        var inMemorySettings = new Dictionary<string, string?>
        {
            {$"{prefix}_SOME_STRING", "EnvValue"}
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        // Act & Assert
        // It iterates properties. SomeInt is the next one likely to fail or implicit failure depending on iteration order.
        // Actually, logic iterates all properties.
        var ex = Assert.Throws<InvalidOperationException>(() => 
            _servicesMock.Object.AddAndConfigureFromEnv<TestSettings>(configuration, prefix));

        Assert.That(ex.Message, Does.Contain("Configuration value"));
        Assert.That(ex.Message, Does.Contain("is required"));
    }

    [Test]
    public void AddAndConfigureFromEnv_TypeConversionFailure_ThrowsInvalidOperationException()
    {
        // Arrange
        var prefix = "TESTAPP";
        var inMemorySettings = new Dictionary<string, string?>
        {
            {$"{prefix}_SOME_STRING", "EnvValue"},
            {$"{prefix}_SOME_INT", "NotAnInteger"}, // Invalid int
            {$"{prefix}_SOME_BOOL", "true"}
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => 
            _servicesMock.Object.AddAndConfigureFromEnv<TestSettings>(configuration, prefix));

        Assert.That(ex.Message, Does.Contain("Failed to convert configuration value"));
    }

    [Test]
    public void AddAndConfigureFromEnv_IgnoresReadOnlyProperties()
    {
        // Arrange
        var prefix = "TESTAPP";
        var inMemorySettings = new Dictionary<string, string?>
        {
            {$"{prefix}_WRITEABLE", "WriteMe"}
            // No env var provided for ReadOnly property
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        // Act
        var result = _servicesMock.Object.AddAndConfigureFromEnv<TestSettingsWithReadOnly>(configuration, prefix);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Writeable, Is.EqualTo("WriteMe"));
        Assert.That(result.ReadOnly, Is.EqualTo("Can't touch this"));
    }

    [Test]
    public void AddAndConfigureFromEnv_SnakeCaseMapping()
    {
        // Arrange
        var prefix = "TESTAPP";
        // Property is ThisIsCamelCase -> maps to THIS_IS_CAMEL_CASE
        var inMemorySettings = new Dictionary<string, string?>
        {
            {$"{prefix}_THIS_IS_CAMEL_CASE", "MappedCorrectly"}
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        // Act
        var result = _servicesMock.Object.AddAndConfigureFromEnv<TestSnakeCaseSettings>(configuration, prefix);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ThisIsCamelCase, Is.EqualTo("MappedCorrectly"));
    }
}
