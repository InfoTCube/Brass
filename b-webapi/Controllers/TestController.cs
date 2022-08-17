using System;
using System.Collections.Generic;
using System.Linq;
using Brass.Controllers;
using Brass.Controllers.Parameters;
using Bwebapi.Entities;

namespace Bwebapi.Controllers;

[ApiController("[controller]")]
public static class WeatherController
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    [Get("GetWeatherForecast")]
    public static Result<WeatherForecast[]> Get()
    {
        return Results.Ok(Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateTime.Now.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray());
    }
}