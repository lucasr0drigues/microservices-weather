﻿using CloudWeather.Report.Config;
using CloudWeather.Report.DataAccess;
using CloudWeather.Report.Models;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace CloudWeather.Report.BusinessLogic
{
    /// <summary>
    /// Aggregates data from multiple external sources to build a weather report
    /// </summary>
    public interface IWeatherAggregator
    {
        /// <summary>
        /// builds and returns a weekly weather report.
        /// persists WeeklyWeatherReport data
        /// </summary>
        /// <param name="zip"></param>
        /// <param name="days"></param>
        /// <returns></returns>
        public Task<WeatherReport> BuildWeeklyReport(string zip, int days);
    }

    public class WeatherReportAggregator : IWeatherAggregator
    {
        private readonly IHttpClientFactory _http;
        private readonly ILogger<WeatherReportAggregator> _logger;
        private readonly WeatherDataConfig _weatherDataConfig;
        private readonly WeatherReportDbContext _db;

        public WeatherReportAggregator(IHttpClientFactory http, ILogger<WeatherReportAggregator> logger, IOptions<WeatherDataConfig> weatherDataConfig, WeatherReportDbContext db)
        {
            _http = http;
            _logger = logger;
            _weatherDataConfig = weatherDataConfig.Value;
            _db = db;
        }

        public async Task<WeatherReport> BuildWeeklyReport(string zip, int days)
        {
            var httpClient = _http.CreateClient();

            var precipData = await FetchPrecipitationData(httpClient, zip, days);
            var totalSnow = GetTotalSnow(precipData);
            var totalRain = GetTotalRain(precipData);
            _logger.LogInformation($"zip: {zip} over last {days} days - total snow: {totalSnow}, total rain: {totalRain}");

            var tempData = await FetchTemperatureData(httpClient, zip, days);
            var averageHighTemp = tempData.Average(t => t.TempHighF);
            var averageLowTemp = tempData.Average(t => t.TempLowF);
            _logger.LogInformation($"zip: {zip} over last {days} days - low temp: {averageLowTemp}, high temp: {averageHighTemp}");

            var weatherReport = new WeatherReport
            {
                AverageHighF = Math.Round(averageHighTemp, 1),
                AverageLowF = Math.Round(averageLowTemp, 1),
                RainfallTotalInches = totalRain,
                SnowTotalInches = totalSnow,
                ZipCode = zip,
                CreatedOn = DateTime.UtcNow,
            };

            _db.Add(weatherReport);
            await _db.SaveChangesAsync();

            return weatherReport;
        }

        private static decimal GetTotalRain(List<PrecipitationModel> precipData)
        {
            var totalSnow = precipData
                .Where(p => p.WeatherType == "rain")
                .Sum(p => p.AmountInches);
            return Math.Round(totalSnow, 1);
        }

        private static decimal GetTotalSnow(List<PrecipitationModel> precipData)
        {
            var totalSnow = precipData
                .Where(p => p.WeatherType == "snow")
                .Sum(p => p.AmountInches);
            return Math.Round(totalSnow, 1);
        }

        private async Task<List<TemperatureModel>> FetchTemperatureData(HttpClient httpClient, string zip, int days)
        {
            var endpoint = BuildTemperatureServiceEndpoint(zip, days);
            var temperatureRecords = await httpClient.GetAsync(endpoint);
            var jsonSerializerOptioons = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var temperatureData = await temperatureRecords.Content.ReadFromJsonAsync<List<TemperatureModel>>(jsonSerializerOptioons);
            return temperatureData ?? new List<TemperatureModel>();
        }

        private string BuildTemperatureServiceEndpoint(string zip, int days)
        {
            var tempServiceProtocol = _weatherDataConfig.TempDataProtocol;
            var tempServiceHost = _weatherDataConfig.TempDataHost;
            var tempServicePort = _weatherDataConfig.TempDataPort;
            return $"{tempServiceProtocol}://{tempServiceHost}:{tempServicePort}/observation/{zip}?days={days}";
        }

        private async Task<List<PrecipitationModel>> FetchPrecipitationData(HttpClient httpClient, string zip, int days)
        {
            var endpoint = BuildPrecipitationServiceEndpoint(zip, days);
            var precipRecords = await httpClient.GetAsync(endpoint);
            var jsonSerializerOptioons = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var precipData = await precipRecords.Content.ReadFromJsonAsync<List<PrecipitationModel>>(jsonSerializerOptioons);
            return precipData ?? new List<PrecipitationModel>();
        }

        private string BuildPrecipitationServiceEndpoint(string zip, int days)
        {
            var precipServiceProtocol = _weatherDataConfig.PrecipDataProtocol;
            var precipServiceHost = _weatherDataConfig.PrecipDataHost;
            var precipServicePort = _weatherDataConfig.PrecipDataPort;
            return $"{precipServiceProtocol}://{precipServiceHost}:{precipServicePort}/observation/{zip}?days={days}";
        }
    }
}
