using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TapMapWeb.Models;
using TapMapWeb.Session;

namespace TapMapWeb.Controllers
{
	public class TapsController : ControllerBase
	{
		public BeerRepository BeerRepository { get; set; }
		public TapRepository TapRepository { get; set; }

		public TapsController()
		{
			BeerRepository = new BeerRepository();
			TapRepository = new TapRepository();
		}

		[HttpGet]
		public ActionResult Create()
		{
			return View();
		}

		//
		// GET: /Tap/
		[HttpPost]
		public ActionResult Create(string place, string placeTitle, string beerId, string comment)
        {
            if (place == "" || beerId == "" || comment == "") return Content("FAIL");
            
			var placeData = place.Split('|');
			if (placeData.Length != 3) return Content("FAIL");

            var beer = BeerRepository.Get(beerId);

            var tap = new Tap
            {
                Beer = beer,
                Place = new Place { 
					Lat = Convert.ToDecimal(placeData[0]), 
					Long = Convert.ToDecimal(placeData[1]), 
					PlaceId = placeData[2],
					Title = placeTitle
				},
                Username = SessionUser.Current.Username,
                Timestamp = DateTime.Now,
                Comment = comment
            };

            TapRepository.Create(tap);
            return Content("TAPPED");
        }
		
		public ActionResult List(string center, string bbox, string zoom)
		{
			var taps = TapRepository.GetTaps(bbox);
			ViewBag.Center = center;
			ViewBag.Zoom = zoom;
			return View(taps);
		}
	}
}
