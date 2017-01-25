using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Spots.Services;
using Spots.ViewModels;
using Umbraco.Core.Models;

namespace Spots.Models {
    public class Spot {

        public int Id
        {
            get; set;
        }
        public string Name
        {
            get; set;
        }
        public string Category
        {
            get; set;
        }

        public string Description
        {
            get; set;
        }

        public string Image
        {
            get; set;
        }

        public string Latitude
        {
            get; set;
        }

        public string Longitude
        {
            get; set;
        }

        public double Distance
        {
            get; set;
        }

        public string DrivingDistance
        {
            get; set;
        }

        public string DrivingDuration
        {
            get; set;
        }

        public int CheckIns
        {
            get; set;
        }

        public DateTime? LastCheckInDate
        {
            get; set;
        }

        public string OptimalWindSpeed
        {
            get; set;
        }
        public string OptimalWindDirection
        {
            get; set;
        }
        public string OptimalWaterHeight
        {
            get; set;
        }

        public string WeatherUrl
        {
            get; set;
        }

        public WeatherData Weather
        {
            get; set;
        }
    }
}