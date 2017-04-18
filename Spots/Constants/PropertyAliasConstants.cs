using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Spots.Constants {
    public class PropertyAliasConstants {
        
        public const string Name = "Name";
        public const string Category = "Category";
        public const string Description = "Description";
        public const string Latitude = "Latitude";
        public const string Longitude = "Longitude";
        public const string Image = "Image";
        public const string CheckIns = "checkIns";
        public const string LastCheckInDate = "lastCheckInDate";

        //Info properties
        public const string OptimalWindSpeed = "optimalWindSpeed";
        public const string OptimalWindDirection = "optimalWindDirection";
        public const string OptimalWaterHeight = "optimalWaterHeight";

        public const string WeatherUrl = "weatherUrl";

        //Social properties
        public const string FacebookUrl = "facebookUrl";
        public const string WebsiteUrl = "websiteUrl";

    }

    public class DocumentTypeAliasConstants {

        public const string Spots = "Spots";
        public const string Spot = "Spot";
    }

    public class SpotCategories{
        public const string Kite = "Kite";
        public const string Cable = "Cable";
    }
}