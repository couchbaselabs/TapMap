using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace TapMapWeb.Models
{
    public class Brewery : ModelBase
    {
        [JsonProperty("name")]
        public string Name { get; set; }        

        public override string Type
        {
            get { return "brewery"; }
        }
    }
}