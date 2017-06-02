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
                .Descendants()
                .Where(x => x.GetPropertyValue<bool>("umbracoNaviHide") == false && x.DocumentTypeAlias != DocumentTypeAlias.SpotFolder)
                .Select(obj => new Spot() {
                    Id = obj.Id,
                    Name = obj.GetPropertyValue<string>(PropertyAlias.Name),
                    Category = obj.GetPropertyValue<string>(PropertyAlias.Category) ?? "",
                    Description = obj.GetPropertyValue<string>(PropertyAlias.Description),
                    Latitude = obj.GetPropertyValue<string>(PropertyAlias.Latitude),
                    Longitude = obj.GetPropertyValue<string>(PropertyAlias.Longitude),
                    GoogleMapsLink = "https://maps.google.com/?q=" + obj.GetPropertyValue<string>(PropertyAlias.Latitude) + "," + obj.GetPropertyValue<string>(PropertyAlias.Longitude),
                    Image = obj.GetPropertyValue<string>(PropertyAlias.Image) != null
                        ? Umbraco.TypedMedia(obj.GetPropertyValue<string>(PropertyAlias.Image)).Url
                        : "/resources/images/no-image.jpg",
                    Distance = CalculateDistance(currentPosition, obj.GetPropertyValue<double>(PropertyAlias.Latitude), obj.GetPropertyValue<double>(PropertyAlias.Longitude)),
                    OptimalWindSpeed = obj.GetPropertyValue<string>(PropertyAlias.OptimalWindSpeed),
                    OptimalWindDirection = obj.GetPropertyValue<string>(PropertyAlias.OptimalWindDirection),
                    OptimalWindDirectionList = obj.GetPropertyValue<string>(PropertyAlias.OptimalWindDirectionList),
                    //Weather = WeatherInfoService.GetWeatherFeed(obj.GetPropertyValue<string>(PropertyAlias.WeatherUrl))
                    IsSpotOptimal = IsSpotOptimal(
                        obj.GetPropertyValue<string>(PropertyAlias.Category),
                        obj.GetPropertyValue<string>(PropertyAlias.OptimalWindSpeed),
                        obj.GetPropertyValue<string>(PropertyAlias.OptimalWindDirectionList),
                        WeatherInfoService.GetWeatherFeed(obj.GetPropertyValue<string>(PropertyAlias.WeatherUrl)))
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
            currentSpot.Name = response.GetPropertyValue<string>(PropertyAlias.Name);
            currentSpot.Category = response.GetPropertyValue<string>(PropertyAlias.Category);
            currentSpot.Description = response.GetPropertyValue<string>(PropertyAlias.Description);
            currentSpot.Latitude = response.GetPropertyValue<string>(PropertyAlias.Latitude);
            currentSpot.Longitude = response.GetPropertyValue<string>(PropertyAlias.Longitude);
            currentSpot.GoogleMapsLink = "https://maps.google.com/?q=" + response.GetPropertyValue<string>(PropertyAlias.Latitude) + "," + response.GetPropertyValue<string>(PropertyAlias.Longitude);
            currentSpot.Image = response.GetPropertyValue<string>(PropertyAlias.Image) != null
                ? Umbraco.TypedMedia(response.GetPropertyValue<string>(PropertyAlias.Image)).Url
                : "/resources/images/no-image.jpg";
            currentSpot.LastCheckInDate = response.GetPropertyValue<DateTime>(PropertyAlias.LastCheckInDate);
            currentSpot.CheckIns = response.GetPropertyValue<int>(PropertyAlias.CheckIns);

            //Weather Properties
            currentSpot.OptimalWindSpeed = response.GetPropertyValue<string>(PropertyAlias.OptimalWindSpeed);
            currentSpot.OptimalWindDirection = response.GetPropertyValue<string>(PropertyAlias.OptimalWindDirection);
            currentSpot.Weather = WeatherInfoService.GetWeatherFeed(response.GetPropertyValue<string>(PropertyAlias.WeatherUrl));
            currentSpot.WeatherUrl = !string.IsNullOrWhiteSpace(response.GetPropertyValue<string>(PropertyAlias.WeatherUrl))
                ? response.GetPropertyValue<string>(PropertyAlias.WeatherUrl).ToString().Replace("forecast.xml", "")
                : "http://www.yr.no/";

            //Calculate if spot is optimal
            currentSpot.IsSpotOptimal = IsSpotOptimal(currentSpot.Category, currentSpot.OptimalWindSpeed, currentSpot.OptimalWindDirection, currentSpot.Weather);


            //Social Properties
            currentSpot.FacebookUrl = response.GetPropertyValue<string>(PropertyAlias.FacebookUrl);
            currentSpot.WebsiteUrl = response.GetPropertyValue<string>(PropertyAlias.WebsiteUrl);


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

                var checkIns = currentSpot.GetValue<int>(PropertyAlias.CheckIns);
                var lastCheckInDate = currentSpot.GetValue<DateTime>(PropertyAlias.LastCheckInDate).ToShortDateString();
                var today = DateTime.Today.ToShortDateString();

                //If the last time the last check-in was made is today, we add a new check-in
                if (lastCheckInDate == today) {
                    checkIns = checkIns + 1;
                }
                else {
                    checkIns = 1;
                }

                currentSpot.SetValue(PropertyAlias.CheckIns, checkIns);
                currentSpot.SetValue(PropertyAlias.LastCheckInDate, DateTime.Now);

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
                var checkIns = currentSpot.GetValue<int>(PropertyAlias.CheckIns);

                //If the last time the last check-in was made is today, we remove a new check-in
                if (checkIns > 0) {
                    checkIns = checkIns - 1;
                }

                currentSpot.SetValue(PropertyAlias.CheckIns, checkIns);

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

        public bool IsWindDirectionOptimal (string optimalWindDirectionList, WeatherData weather) {

            if (!string.IsNullOrWhiteSpace(optimalWindDirectionList) && weather != null){
                var splittedList = optimalWindDirectionList.ToUpper().Split(',');
                var currentWind = weather.WindDirection.ToUpper();

                if (splittedList.Any(t => t == currentWind)){
                    return true;
                }
            }

            return false;
        }

        public bool IsSpotOptimal(string category, string optimalWindSpeed, string optimalWindDirectionList, WeatherData weather){

            var speedIsOptimal = IsWindSpeedOptimal(optimalWindSpeed, weather);
            var directionIsOptimal = IsWindDirectionOptimal(optimalWindDirectionList, weather);

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
