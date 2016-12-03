using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Serialization;
using Spots.ViewModels;
using Umbraco.Core;

namespace Spots.Services
{
    public static class WeatherInfoService
    {
        private const string ApiUrl = "http://www.yr.no/place/Denmark/Central_Jutland/Borresknob/forecast.xml";

        public static WeatherData GetWeatherDescription()
        {
            var cacheHelper = new CacheHelper();

            //var cachedValue = (WeatherData)cacheHelper.RuntimeCache.GetCacheItem("weather_data", GetWeatherFeed, TimeSpan.FromMinutes(60));
            //if (cachedValue != null)
            //{
            //    return cachedValue;
            //}

            return null;
        }

        public static WeatherData GetWeatherFeed(string weatherUrl)
        {
            if (!string.IsNullOrWhiteSpace(weatherUrl))
            {
                using (WebClient client = new WebClient()) {

                    var weatherResult = new WeatherData();

                    var currentTime = DateTime.Now;
                    var serializer = new XmlSerializer(typeof(weatherdata));
                    client.Headers["User-Agent"] =
                    "Mozilla/4.0 (Compatible; Windows NT 5.1; MSIE 6.0) " +
                    "(compatible; MSIE 6.0; Windows NT 5.1; " +
                    ".NET CLR 1.1.4322; .NET CLR 2.0.50727)";

                    var result = client.DownloadString(weatherUrl);

                    var reader = new StringReader(result);
                    var resultingMessage = (weatherdata)serializer.Deserialize(reader);

                    if (resultingMessage != null && resultingMessage.forecast != null && resultingMessage.forecast.tabular != null) {
                        var times = resultingMessage.forecast.tabular;
                        if (times.Any()) {
                            if (times.Any(x => x.from < currentTime && x.to > currentTime)) {
                                var time = times.First(x => x.from < currentTime && x.to > currentTime);

                                if (time != null && time.symbol != null && !string.IsNullOrWhiteSpace(time.symbol.name)) {
                                    weatherResult.WeatherDescription = time.symbol.name;
                                }

                                if (time != null && time.temperature != null) {
                                    weatherResult.Degrees = time.temperature.value;
                                }

                                if (time != null && time.windDirection != null) {
                                    weatherResult.WindDirection = time.windDirection.code;
                                }

                                if (time != null && time.windDirection != null) {
                                    weatherResult.WindDirectionDeg = time.windDirection.deg;
                                }

                                if (time != null && time.windSpeed != null) {
                                    weatherResult.WindSpeed = time.windSpeed.mps;
                                }
                            }
                            else {
                                var time = times.OrderBy(x => x.from).First();

                                if (time.symbol != null && !string.IsNullOrWhiteSpace(time.symbol.name)) {
                                    weatherResult.WeatherDescription = time.symbol.name;
                                }

                                if (time.temperature != null) {
                                    weatherResult.Degrees = time.temperature.value;
                                }

                                if (time.windDirection != null) {
                                    weatherResult.WindDirection = time.windDirection.code;
                                }

                                if (time.windDirection != null) {
                                    weatherResult.WindDirectionDeg = time.windDirection.deg;
                                }

                                if (time.windSpeed != null) {
                                    weatherResult.WindSpeed = time.windSpeed.mps;
                                }
                            }

                        }
                    }

                    return weatherResult;
                }
            }

            return null;
        }
    }
}