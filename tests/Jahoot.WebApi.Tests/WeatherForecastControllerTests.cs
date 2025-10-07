using Jahoot.Core.Models;
using Jahoot.WebApi.Controllers;
using Microsoft.Extensions.Logging.Abstractions;

namespace Jahoot.WebApi.Tests;

public class WeatherForecastControllerTests
{
    [Test]
    public void Get_ReturnsFiveWeatherForecasts()
    {
        NullLogger<WeatherForecastController> logger = new();
        WeatherForecastController controller = new(logger);

        IEnumerable<WeatherForecast> result = controller.Get().ToArray();

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<IEnumerable<WeatherForecast>>());
        Assert.That(result.Count(), Is.EqualTo(5));
    }
}
