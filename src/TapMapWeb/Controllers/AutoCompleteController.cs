using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TapMapWeb.Models;

namespace TapMapWeb.Controllers
{
    public class AutoCompleteController : ControllerBase
    {
        public BeerRepository BeerRepository { get; set; }
		public BreweryRepository BreweryRepository { get; set; }

        public AutoCompleteController()
        {
            BeerRepository = new BeerRepository();
			BreweryRepository = new BreweryRepository();
        }

		public ActionResult Breweries(string term)
		{
			return Json(BreweryRepository.GetBreweries(term)
					.Select(b => new { label = b.Name, id = b.Id }), JsonRequestBehavior.AllowGet);
		}

        public ActionResult Beers(string brewery, string term)
        {
            return Json(BeerRepository.GetBeers(brewery, term)
					.Where(b => b.Name.StartsWith(term, StringComparison.CurrentCultureIgnoreCase))
					.Select(b => new { label = b.Name, id = b.Id}),JsonRequestBehavior.AllowGet);
        }		
    }
}