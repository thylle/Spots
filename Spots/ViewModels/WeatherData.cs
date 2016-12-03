namespace Spots.ViewModels
{
    public class WeatherData
    {

        public string WeatherDescription { get; set; }

        public double Degrees { get; set; }

        public string WindDirection { get; set; }

        public decimal WindDirectionDeg { get; set; }

        public decimal WindSpeed { get; set; }

        public string WeatherIcon
        {
            get
            {
                //URL FOR ICON NAMES: http://om.yr.no/forklaring/symbol/
                switch (WeatherDescription) {
                    case "Clear sky":
                    case "Fair":
                        return "ion-ios-sunny";

                    case "Partly cloudy":
                        return "ion-ios-partlysunny";

                    case "Fog":
                    case "Cloudy":
                        return "ion-ios-cloudy";

                    case "Rain":
                    case "Light rain":
                    case "Heavy rain":
                    case "Light rain showers":
                    case "Rain showers":
                    case "Heavy rain showers":
                        return "ion-ios-rainy";

                    case "Light rain and thunder":
                    case "Rain and thunder":
                    case "Heavy rain and thunder":
                        return "ion-ios-thunderstorm";

                    case "Snow":
                    case "Sleet":
                    case "Heavy snow":
                        return "ion-ios-snowy";

                    default:
                        return "ion-ios-partlysunny";
                }


            }
        }

    }
}