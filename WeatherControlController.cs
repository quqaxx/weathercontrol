using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using AssettoServer.Server.Weather;
using AssettoServer.Shared.Weather;
using Microsoft.AspNetCore.Mvc;

namespace WeatherControlPlugin;

public class ApplyRequest
{
    public string? Time { get; set; }             // "HH:mm"
    public string? WeatherType { get; set; }      // /cspweather listesindeki ad
    public int? Transition { get; set; }          // saniye
    public float? RainIntensity { get; set; }     // 0-1
    public float? RainWetness { get; set; }       // 0-1
    public float? RainWater { get; set; }         // 0-1
}

[ApiController]
public class WeatherControlController : ControllerBase
{
    private readonly WeatherManager _weatherManager;
    private readonly WeatherControlConfiguration _config;

    public WeatherControlController(WeatherManager weatherManager, WeatherControlConfiguration config)
    {
        _weatherManager = weatherManager;
        _config = config;
    }

    private bool Authorized()
    {
        if (string.IsNullOrEmpty(_config.ApiKey)) return false;
        var given = Request.Headers["X-Api-Key"].ToString();
        var a = Encoding.UTF8.GetBytes(given);
        var b = Encoding.UTF8.GetBytes(_config.ApiKey);
        return a.Length == b.Length && CryptographicOperations.FixedTimeEquals(a, b);
    }

    [HttpGet("/wcontrol/state")]
    public IActionResult State()
    {
        if (!Authorized()) return Unauthorized(new { error = "Gecersiz API anahtari." });
        var w = _weatherManager.CurrentWeather;
        return Ok(new
        {
            weatherTypes = Enum.GetNames<WeatherFxType>(),
            rain = new { intensity = w.RainIntensity, wetness = w.RainWetness, water = w.RainWater }
        });
    }

    [HttpPost("/wcontrol/apply")]
    public IActionResult Apply([FromBody] ApplyRequest req)
    {
        if (!Authorized()) return Unauthorized(new { error = "Gecersiz API anahtari." });

        // Once dogrula, sonra uygula: hata varsa hicbir sey degismesin.
        int? seconds = null;
        if (!string.IsNullOrWhiteSpace(req.Time))
        {
            if (!DateTime.TryParseExact(req.Time, "H:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var t))
                return BadRequest(new { error = "Saat HH:mm bicimde olmali." });
            seconds = (int)t.TimeOfDay.TotalSeconds;
        }

        WeatherFxType? type = null;
        if (!string.IsNullOrWhiteSpace(req.WeatherType))
        {
            if (!Enum.TryParse(req.WeatherType, true, out WeatherFxType parsed))
                return BadRequest(new { error = $"'{req.WeatherType}' gecerli bir WeatherFX tipi degil." });
            type = parsed;
        }

        foreach (var v in new[] { req.RainIntensity, req.RainWetness, req.RainWater })
            if (v is < 0 or > 1) return BadRequest(new { error = "Yagmur degerleri 0 ile 1 arasinda olmali." });

        var applied = new List<string>();
        if (seconds.HasValue)
        {
            _weatherManager.SetTime(seconds.Value);
            applied.Add("time");
        }
        if (type.HasValue)
        {
            _weatherManager.SetCspWeather(type.Value, Math.Clamp(req.Transition ?? 10, 0, 300));
            applied.Add("weather");
        }
        if (req.RainIntensity.HasValue || req.RainWetness.HasValue || req.RainWater.HasValue)
        {
            var w = _weatherManager.CurrentWeather;
            if (req.RainIntensity.HasValue) w.RainIntensity = req.RainIntensity.Value;
            if (req.RainWetness.HasValue) w.RainWetness = req.RainWetness.Value;
            if (req.RainWater.HasValue) w.RainWater = req.RainWater.Value;
            _weatherManager.SendWeather();
            applied.Add("rain");
        }

        return Ok(new { ok = true, applied });
    }
}
