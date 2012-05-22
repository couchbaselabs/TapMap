using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace TapMapWeb.Models
{
    public class Place
    {
        [JsonProperty("title")]
        public string Title { get; set; }

		[JsonProperty("placeId")]
		public string PlaceId { get; set; }

        [JsonProperty("lat")]
        public decimal Lat { get; set; }

        [JsonProperty("long")]
        public decimal Long { get; set; }

        
    }
}