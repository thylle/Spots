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


namespace Spots.Controllers
{
    [EnableCors(origins: "http://spotsApp.local", headers: "*", methods: "*")]
    public class SpotsController : UmbracoApiController {

        public IEnumerable<Spot> GetAllSpots (int parentId = 1054) {

            UmbracoHelper helper = new UmbracoHelper(UmbracoContext);
            
            var response = helper.TypedContent(parentId)
                .Children
                .Select(obj => new Spot() {
                    Id = obj.Id,
                    Name = obj.GetPropertyValue<string>(PropertyAliasConstants.Name),
                    Description = obj.GetPropertyValue<string>(PropertyAliasConstants.Description),
                    Latitude = obj.GetPropertyValue<string>(PropertyAliasConstants.Latitude),
                    Longitude = obj.GetPropertyValue<string>(PropertyAliasConstants.Longitude),
                    Distance = null,
                    DrivingDistance = null,
                    DrivingDuration = null,
                    Image = "img/demo/kitespot-" + obj.Index() + ".jpg"
                });

            return response;
        }

        // GET: Spots/Details/5
        public Spot GetSpotById (int spotId) {

            Spot currentSpot = new Spot();
            UmbracoHelper helper = new UmbracoHelper(UmbracoContext);

            var response = helper.Content(spotId);

            currentSpot.Id = spotId;
            currentSpot.Name = response.GetPropertyValue<string>(PropertyAliasConstants.Name);
            currentSpot.Description = response.GetPropertyValue<string>(PropertyAliasConstants.Description);
            currentSpot.Latitude = response.GetPropertyValue<string>(PropertyAliasConstants.Latitude);
            currentSpot.Longitude = response.GetPropertyValue<string>(PropertyAliasConstants.Longitude);
            currentSpot.Distance = null;
            currentSpot.DrivingDistance = null;
            currentSpot.DrivingDuration = null;
            currentSpot.Image = "img/demo/kitespot-0.jpg";

            return currentSpot;
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
