using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Enyim.Caching.Memcached;
using TapMapWeb.Extensions;     

namespace TapMapWeb.Models
{
    public class TapRepository : RepositoryBase<Tap>
    {
        public override ulong Create(Tap model)
        {
            var beerKey = model.Beer.Name.Replace(" ", "_");
            var placeKey = model.Place.PlaceId.Replace(" ", "_");

            model.Id = string.Concat(beerKey, "_", placeKey, "_", DateTime.Now.Ticks);
            var result = _Client.CasJson(StoreMode.Add, BuildKey(model), model);
            return result.Result ? result.Cas : 0;
        }

        public IEnumerable<Tap> GetTaps()
        {
            foreach (var item in View("all_taps").Limit(20).Descending(true))
            {
                yield return Get(item.ItemId);
            }
        }

		public IEnumerable<Tap> GetTaps(string boundingBox)
		{
			foreach (var item in SpatialView("by_location").BoundingBox(boundingBox))
			{
				yield return Get(item.Id);
			}
		}
    }
}