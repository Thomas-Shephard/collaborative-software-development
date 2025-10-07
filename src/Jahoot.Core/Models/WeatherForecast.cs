namespace Jahoot.Core.Models;

public class WeatherForecast
{
    public DateOnly Date { get; set; }

    public int TemperatureC { get; set; }

    public int TemperatureF => 32 + (int)(TemperatureC * (float)9 / 5);

    public string? Summary { get; set; }
}
