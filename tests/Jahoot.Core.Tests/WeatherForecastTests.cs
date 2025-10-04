using Jahoot.Core.Models;

namespace Jahoot.Core.Tests;

public class WeatherForecastTests
{
    [Test]
    public void TemperatureF_CalculatedCorrectly()
    {
        WeatherForecast weatherForecast = new()
        {
            TemperatureC = 0
        };

        int tempF = weatherForecast.TemperatureF;

        Assert.That(tempF, Is.EqualTo(32));
    }

    [Test]
    public void TemperatureF_CalculatedCorrectly_WithNegativeValue()
    {
        WeatherForecast weatherForecast = new()
        {
            TemperatureC = -20
        };

        int tempF = weatherForecast.TemperatureF;

        Assert.That(tempF, Is.EqualTo(-4));
    }
}
