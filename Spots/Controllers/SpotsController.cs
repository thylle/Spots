using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using Spots.Constants;
using Spots.Models;
using Umbraco.Web;
using Umbraco.Web.WebApi;
using System.Web.Http.Cors;
using Spots.HelperClasses;
using umbraco;
using umbraco.cms.businesslogic.web;
using umbraco.cms.helpers;


namespace Spots.Controllers
{
    public class SpotsController : UmbracoApiController {

        public IEnumerable<Spot> GetAllSpots (){

            var helper = new UmbracoHelper(UmbracoContext);
            
            var spotsContainer = helper.TypedContentAtRoot().DescendantsOrSelf("Spots").FirstOrDefault();

            var response = helper.TypedContent(spotsContainer.Id)
                .Children
                .Select(obj => new Spot() {
                    Id = obj.Id,
                    Name = obj.GetPropertyValue<string>(PropertyAliasConstants.Name),
                    Category = obj.GetPropertyValue<string>(PropertyAliasConstants.Category) ?? "",
                    Description = obj.GetPropertyValue<string>(PropertyAliasConstants.Description),
                    Latitude = obj.GetPropertyValue<string>(PropertyAliasConstants.Latitude),
                    Longitude = obj.GetPropertyValue<string>(PropertyAliasConstants.Longitude),
                    Image = obj.GetPropertyValue<string>(PropertyAliasConstants.Image) != null 
                        ? Umbraco.TypedMedia(obj.GetPropertyValue<string>(PropertyAliasConstants.Image)).Url 
                        : "/resources/images/no-image.jpg",
                    Distance = null,
                    DrivingDistance = null,
                    DrivingDuration = null,
                    LastCheckInDate = obj.GetPropertyValue<DateTime>(PropertyAliasConstants.LastCheckInDate),
                    CheckIns =  obj.GetPropertyValue<int>(PropertyAliasConstants.CheckIns)
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
            currentSpot.Distance = null;
            currentSpot.DrivingDistance = null;
            currentSpot.DrivingDuration = null;
            currentSpot.Image = response.GetPropertyValue<string>(PropertyAliasConstants.Image) != null
                ? Umbraco.TypedMedia(response.GetPropertyValue<string>(PropertyAliasConstants.Image)).Url 
                : "/resources/images/no-image.jpg";
            currentSpot.LastCheckInDate = response.GetPropertyValue<DateTime>(PropertyAliasConstants.LastCheckInDate);
            currentSpot.CheckIns = response.GetPropertyValue<int>(PropertyAliasConstants.CheckIns);

            //If the last time the last check-in was made is NOT today, we reset it
            if (Convert.ToDateTime(currentSpot.LastCheckInDate).ToShortDateString() != today) {
                currentSpot.CheckIns = 0;
            }
            
            return currentSpot;
        }


        // POST: /Umbraco/Api/Spots/CheckIn?spotId=1055
        public void CheckIn (int spotId) {

    		try{
    			var contentService = Services.ContentService;
	            var currentSpot = contentService.GetById(spotId);

                var checkIns = currentSpot.GetValue<int>(PropertyAliasConstants.CheckIns);
                var lastCheckInDate = currentSpot.GetValue<DateTime>(PropertyAliasConstants.LastCheckInDate).ToShortDateString();
    		    var today = DateTime.Today.ToShortDateString();

                //If the last time the last check-in was made is today, we add a new check-in
                if (lastCheckInDate == today) {
                    checkIns = checkIns + 1;
                }
                else{
                    checkIns = 1;
                }

                currentSpot.SetValue(PropertyAliasConstants.CheckIns, checkIns);
                currentSpot.SetValue(PropertyAliasConstants.LastCheckInDate, DateTime.Now);

	        	contentService.SaveAndPublishWithStatus(currentSpot);
    		}
    		catch (Exception ex){
    		}
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
