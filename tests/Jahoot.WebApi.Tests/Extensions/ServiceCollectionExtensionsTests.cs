using System.Diagnostics.CodeAnalysis;
using Jahoot.WebApi.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Jahoot.WebApi.Tests.Extensions;

public class ServiceCollectionExtensionsTests
{
    private const string Prefix = "TEST_APP";

    private Mock<IServiceCollection> _servicesMock;

    [SetUp]
    public void Setup()
    {
        _servicesMock = new Mock<IServiceCollection>();
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    private class TestSettings
    {
        public required string SomeString { get; set; }
        public int SomeInt { get; set; }
        public bool SomeBool { get; set; }
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    private class TestSnakeCaseSettings
    {
        public required string ThisIsCamelCase { get; set; }
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    private class TestSettingsWithReadOnly
    {
        public required string Writeable { get; set; }
        [SuppressMessage("Performance", "CA1822:Mark members as static")]
        // ReSharper disable once MemberCanBeMadeStatic.Local
        public string ReadOnly => "Read only property";
    }

    [Test]
    public void AddAndConfigure_ValidConfiguration_RegistersSettings()
    {
        Dictionary<string, string?> inMemorySettings = new()
        {
            {"MySettings:SomeString", "TestValue"},
            {"MySettings:SomeInt", "123"},
            {"MySettings:SomeBool", "true"}
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        TestSettings result = _servicesMock.Object.AddAndConfigure<TestSettings>(configuration, "MySettings");

        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.SomeString, Is.EqualTo("TestValue"));
            Assert.That(result.SomeInt, Is.EqualTo(123));
            Assert.That(result.SomeBool, Is.True);
        }

        _servicesMock.Verify(s => s.Add(It.Is<ServiceDescriptor>(d =>
            d.ServiceType == typeof(TestSettings) &&
            d.Lifetime == ServiceLifetime.Singleton &&
            d.ImplementationInstance == result)), Times.Once);
    }

    [Test]
    public void AddAndConfigure_MissingSection_ThrowsInvalidOperationException()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        InvalidOperationException? ex = Assert.Throws<InvalidOperationException>(() => _servicesMock.Object.AddAndConfigure<TestSettings>(configuration, "NonExistentSection"));
        Assert.That(ex.Message, Is.EqualTo("NonExistentSection is not configured."));
    }

    [Test]
    public void AddAndConfigureFromEnv_ValidEnvVars_RegistersSettings()
    {
        Dictionary<string, string?> inMemorySettings = new()
        {
            {$"{Prefix}_SOME_STRING", "EnvValue"},
            {$"{Prefix}_SOME_INT", "456"},
            {$"{Prefix}_SOME_BOOL", "false"}
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        TestSettings result = _servicesMock.Object.AddAndConfigureFromEnv<TestSettings>(configuration, Prefix);

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
        Dictionary<string, string?> inMemorySettings = new()
        {
            {$"{Prefix}_SOME_STRING", "EnvValue"}
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        InvalidOperationException? ex = Assert.Throws<InvalidOperationException>(() => _servicesMock.Object.AddAndConfigureFromEnv<TestSettings>(configuration, Prefix));
        Assert.That(ex.Message, Does.Contain("Configuration value"));
        Assert.That(ex.Message, Does.Contain("is required"));
    }

    [Test]
    public void AddAndConfigureFromEnv_TypeConversionFailure_ThrowsInvalidOperationException()
    {
        Dictionary<string, string?> inMemorySettings = new()
        {
            {$"{Prefix}_SOME_STRING", "EnvValue"},
            {$"{Prefix}_SOME_INT", "NotAnInteger"},
            {$"{Prefix}_SOME_BOOL", "true"}
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        InvalidOperationException? ex = Assert.Throws<InvalidOperationException>(() => _servicesMock.Object.AddAndConfigureFromEnv<TestSettings>(configuration, Prefix));
        Assert.That(ex.Message, Does.Contain("Failed to convert configuration value"));
    }

    [Test]
    public void AddAndConfigureFromEnv_IgnoresReadOnlyProperties()
    {
        Dictionary<string, string?> inMemorySettings = new()
        {
            {$"{Prefix}_WRITEABLE", "WriteMe"}
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        TestSettingsWithReadOnly result = _servicesMock.Object.AddAndConfigureFromEnv<TestSettingsWithReadOnly>(configuration, Prefix);

        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Writeable, Is.EqualTo("WriteMe"));
            Assert.That(result.ReadOnly, Is.EqualTo("Read only property"));
        }
    }

    [Test]
    public void AddAndConfigureFromEnv_SnakeCaseMapping()
    {
        Dictionary<string, string?> inMemorySettings = new()
        {
            {$"{Prefix}_THIS_IS_CAMEL_CASE", "MappedCorrectly"}
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        TestSnakeCaseSettings result = _servicesMock.Object.AddAndConfigureFromEnv<TestSnakeCaseSettings>(configuration, Prefix);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ThisIsCamelCase, Is.EqualTo("MappedCorrectly"));
    }
}
