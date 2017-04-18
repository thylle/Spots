using System;
using System.Collections.Generic;
using System.Linq;
using Spots.Constants;
using Spots.Models;
using Umbraco.Web;
using Umbraco.Web.WebApi;
using Spots.Services;
using System.Device.Location;
using Spots.ViewModels;


namespace Spots.Controllers {
    public class SpotsController : UmbracoApiController {

        public IEnumerable<Spot> GetAllSpots (double lat, double lon) {

            var helper = new UmbracoHelper(UmbracoContext);
            var spotsContainer = helper.TypedContentAtRoot().DescendantsOrSelf("Spots").FirstOrDefault();
            GeoCoordinate currentPosition = null;

            currentPosition = new GeoCoordinate(Convert.ToDouble(lat), Convert.ToDouble(lon));

            var response = helper.TypedContent(spotsContainer.Id)
                .Descendants(DocumentTypeAliasConstants.Spot)
                .Where("Visible")
                .Select(obj => new Spot() {
                    Id = obj.Id,
                    Name = obj.GetPropertyValue<string>(PropertyAliasConstants.Name),
                    Category = obj.GetPropertyValue<string>(PropertyAliasConstants.Category) ?? "",
                    Description = obj.GetPropertyValue<string>(PropertyAliasConstants.Description),
                    Latitude = obj.GetPropertyValue<string>(PropertyAliasConstants.Latitude),
                    Longitude = obj.GetPropertyValue<string>(PropertyAliasConstants.Longitude),
                    GoogleMapsLink = "https://maps.google.com/?q=" + obj.GetPropertyValue<string>(PropertyAliasConstants.Latitude) + "," + obj.GetPropertyValue<string>(PropertyAliasConstants.Longitude),
                    Image = obj.GetPropertyValue<string>(PropertyAliasConstants.Image) != null
                        ? Umbraco.TypedMedia(obj.GetPropertyValue<string>(PropertyAliasConstants.Image)).Url
                        : "/resources/images/no-image.jpg",
                    Distance = CalculateDistance(currentPosition, obj.GetPropertyValue<double>(PropertyAliasConstants.Latitude), obj.GetPropertyValue<double>(PropertyAliasConstants.Longitude)),
                    OptimalWindSpeed = obj.GetPropertyValue<string>(PropertyAliasConstants.OptimalWindSpeed),
                    OptimalWindDirection = obj.GetPropertyValue<string>(PropertyAliasConstants.OptimalWindDirection),
                    Weather = WeatherInfoService.GetWeatherFeed(obj.GetPropertyValue<string>(PropertyAliasConstants.WeatherUrl)),
                    IsSpotOptimal = IsSpotOptimal(
                        obj.GetPropertyValue<string>(PropertyAliasConstants.Category), 
                        obj.GetPropertyValue<string>(PropertyAliasConstants.OptimalWindSpeed),
                        obj.GetPropertyValue<string>(PropertyAliasConstants.OptimalWindDirection),
                        WeatherInfoService.GetWeatherFeed(obj.GetPropertyValue<string>(PropertyAliasConstants.WeatherUrl)))
                });

            return response;
        }

        // GET: Umbraco/Api/Spots/GetSpotById?spotId=1055
        public Spot GetSpotById (int spotId) {

            Spot currentSpot = new Spot();
            UmbracoHelper helper = new UmbracoHelper(UmbracoContext);

            var today = DateTime.Today.ToShortDateString();
            var response = helper.Content(spotId);

            currentSpot.Id = spotId;
            currentSpot.Name = response.GetPropertyValue<string>(PropertyAliasConstants.Name);
            currentSpot.Category = response.GetPropertyValue<string>(PropertyAliasConstants.Category);
            currentSpot.Description = response.GetPropertyValue<string>(PropertyAliasConstants.Description);
            currentSpot.Latitude = response.GetPropertyValue<string>(PropertyAliasConstants.Latitude);
            currentSpot.Longitude = response.GetPropertyValue<string>(PropertyAliasConstants.Longitude);
            currentSpot.GoogleMapsLink = "https://maps.google.com/?q=" + response.GetPropertyValue<string>(PropertyAliasConstants.Latitude) + "," + response.GetPropertyValue<string>(PropertyAliasConstants.Longitude);
            currentSpot.Image = response.GetPropertyValue<string>(PropertyAliasConstants.Image) != null
                ? Umbraco.TypedMedia(response.GetPropertyValue<string>(PropertyAliasConstants.Image)).Url
                : "/resources/images/no-image.jpg";
            currentSpot.LastCheckInDate = response.GetPropertyValue<DateTime>(PropertyAliasConstants.LastCheckInDate);
            currentSpot.CheckIns = response.GetPropertyValue<int>(PropertyAliasConstants.CheckIns);

            //Weather Properties
            currentSpot.OptimalWindSpeed = response.GetPropertyValue<string>(PropertyAliasConstants.OptimalWindSpeed);
            currentSpot.OptimalWindDirection = response.GetPropertyValue<string>(PropertyAliasConstants.OptimalWindDirection);
            currentSpot.Weather = WeatherInfoService.GetWeatherFeed(response.GetPropertyValue<string>(PropertyAliasConstants.WeatherUrl));
            currentSpot.WeatherUrl = !string.IsNullOrWhiteSpace(response.GetPropertyValue<string>(PropertyAliasConstants.WeatherUrl))
                ? response.GetPropertyValue<string>(PropertyAliasConstants.WeatherUrl).ToString().Replace("forecast.xml", "")
                : "http://www.yr.no/";

            //Calculate if spot is optimal
            currentSpot.IsSpotOptimal = IsSpotOptimal(currentSpot.Category, currentSpot.OptimalWindSpeed, currentSpot.OptimalWindDirection, currentSpot.Weather);


            //Social Properties
            currentSpot.FacebookUrl = response.GetPropertyValue<string>(PropertyAliasConstants.FacebookUrl);
            currentSpot.WebsiteUrl = response.GetPropertyValue<string>(PropertyAliasConstants.WebsiteUrl);


            //If the last time the last check-in was made is NOT today, we reset it
            if (Convert.ToDateTime(currentSpot.LastCheckInDate).ToShortDateString() != today) {
                currentSpot.CheckIns = 0;
            }

            return currentSpot;
        }


        // POST: /Umbraco/Api/Spots/CheckIn?spotId=1055
        public void CheckIn (int spotId) {

            try {
                var contentService = Services.ContentService;
                var currentSpot = contentService.GetById(spotId);

                var checkIns = currentSpot.GetValue<int>(PropertyAliasConstants.CheckIns);
                var lastCheckInDate = currentSpot.GetValue<DateTime>(PropertyAliasConstants.LastCheckInDate).ToShortDateString();
                var today = DateTime.Today.ToShortDateString();

                //If the last time the last check-in was made is today, we add a new check-in
                if (lastCheckInDate == today) {
                    checkIns = checkIns + 1;
                }
                else {
                    checkIns = 1;
                }

                currentSpot.SetValue(PropertyAliasConstants.CheckIns, checkIns);
                currentSpot.SetValue(PropertyAliasConstants.LastCheckInDate, DateTime.Now);

                contentService.SaveAndPublishWithStatus(currentSpot);
            }
            catch (Exception ex) {
            }
        }


        // POST: /Umbraco/Api/Spots/CheckIn?spotId=1055
        public void CheckOut (int spotId) {

            try {
                var contentService = Services.ContentService;
                var currentSpot = contentService.GetById(spotId);
                var checkIns = currentSpot.GetValue<int>(PropertyAliasConstants.CheckIns);

                //If the last time the last check-in was made is today, we remove a new check-in
                if (checkIns > 0) {
                    checkIns = checkIns - 1;
                }

                currentSpot.SetValue(PropertyAliasConstants.CheckIns, checkIns);

                contentService.SaveAndPublishWithStatus(currentSpot);
            }
            catch (Exception ex) {
            }
        }

        //Calculate Distance based on current position and the spots lat and lng.
        public double CalculateDistance (GeoCoordinate currentPosition, double lat, double lon) {
            if (currentPosition != null && lat != 0 && lon != 0) {
                var distance = Math.Round(
                currentPosition.GetDistanceTo(
                    new GeoCoordinate(
                        Convert.ToDouble(lat),
                        Convert.ToDouble(lon)
                        )
                    ) / 1000);

                return distance;
            }

            return Double.NaN;
        }

        
        public bool IsWindSpeedOptimal (string optimalWindSpeed, WeatherData weather){

            if (!string.IsNullOrWhiteSpace(optimalWindSpeed) && weather != null){
                int optimalWindSpeedAsInt = Int32.Parse(optimalWindSpeed);
                var currentWindSpeed = weather.WindSpeed;

                if (currentWindSpeed >= optimalWindSpeedAsInt) {
                    return true;
                }
            }

            return false;
        }

        public bool IsWindDirectionOptimal (string optimalWindDirection, WeatherData weather) {

            if (!string.IsNullOrWhiteSpace(optimalWindDirection) && weather != null) {

                if (optimalWindDirection.ToUpper() == weather.WindDirection.ToUpper()) {
                    return true;
                }
            }

            return false;
        }

        public bool IsSpotOptimal(string category, string optimalWindSpeed, string optimalWindDirection, WeatherData weather){

            var speedIsOptimal = IsWindSpeedOptimal(optimalWindSpeed, weather);
            var directionIsOptimal = IsWindDirectionOptimal(optimalWindDirection, weather);

            //If category is "kite" && speed and direction is optimal
            if (category == SpotCategories.Kite && speedIsOptimal && directionIsOptimal){
                return true;
            }

            //If category is "cable" && speed is NOT optimal and direction is optimal 
            if (category == SpotCategories.Cable && !speedIsOptimal && directionIsOptimal){
                return true;
            }

            return false;
        }

        public string GetImage (string image) {

            return "";
        }


        //// GET: Spots/Create
        //public ActionResult Create()
        //{
        //    return View();
        //}

        //// POST: Spots/Create
        //[HttpPost]
        //public ActionResult Create(FormCollection collection)
        //{
        //    try
        //    {
        //        // TODO: Add insert logic here

        //        return RedirectToAction("Index");
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}

        //// GET: Spots/Edit/5
        //public ActionResult Edit(int id)
        //{
        //    return View();
        //}

        //// POST: Spots/Edit/5
        //[HttpPost]
        //public ActionResult Edit(int id, FormCollection collection)
        //{
        //    try
        //    {
        //        // TODO: Add update logic here

        //        return RedirectToAction("Index");
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}

        //// GET: Spots/Delete/5
        //public ActionResult Delete(int id)
        //{
        //    return View();
        //}

        //// POST: Spots/Delete/5
        //[HttpPost]
        //public ActionResult Delete(int id, FormCollection collection)
        //{
        //    try
        //    {
        //        // TODO: Add delete logic here

        //        return RedirectToAction("Index");
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}


    }
}
