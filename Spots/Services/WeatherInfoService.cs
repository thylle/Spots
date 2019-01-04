using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Serialization;
using Spots.ViewModels;
using Umbraco.Core;

namespace Spots.Services {
    public static class WeatherInfoService {
        private const string ApiUrl = "http://www.yr.no/place/Denmark/Central_Jutland/Borresknob/forecast.xml";

        public static WeatherData GetWeatherDescription () {
            var cacheHelper = new CacheHelper();

            //var cachedValue = (WeatherData)cacheHelper.RuntimeCache.GetCacheItem("weather_data", GetWeatherFeed, TimeSpan.FromMinutes(60));
            //if (cachedValue != null)
            //{
            //    return cachedValue;
            //}

            return null;
        }

        public static List<WeatherData> GetWeatherFeed (string weatherUrl) {
            if (!string.IsNullOrWhiteSpace(weatherUrl)) {
                using (WebClient client = new WebClient()) {
                    var correctedUrl = weatherUrl.Replace("forecast.xml", "") + "forecast.xml";

                    var weatherResultList = new List<WeatherData>();

                    var currentTime = DateTime.Now;
                    var serializer = new XmlSerializer(typeof(weatherdata));
                    client.Headers["User-Agent"] =
                    "Mozilla/4.0 (Compatible; Windows NT 5.1; MSIE 6.0) " +
                    "(compatible; MSIE 6.0; Windows NT 5.1; " +
                    ".NET CLR 1.1.4322; .NET CLR 2.0.50727)";

                    var result = client.DownloadString(correctedUrl);

                    var reader = new StringReader(result);
                    var resultingMessage = (weatherdata)serializer.Deserialize(reader);

                    if (resultingMessage != null && resultingMessage.forecast != null && resultingMessage.forecast.tabular != null) {
                        var times = resultingMessage.forecast.tabular;

                        if (times.Any()) {
                            weatherResultList.AddRange(
                                from item in times
                                where item != null && item.@from.Day == currentTime.Day
                                select new WeatherData() {
                                    WeatherDescription = item.symbol.name,
                                    Degrees = item.temperature.value,
                                    WindDirection = item.windDirection.code,
                                    WindDirectionDeg = item.windDirection.deg,
                                    WindSpeed = item.windSpeed.mps
                                });
                        }
                    }

                    return weatherResultList;
                }
            }

            return null;
        }
    }
}