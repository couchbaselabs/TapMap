using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TapMapWeb.Models
{
    public class BeerRepository : RepositoryBase<Beer>
    {
        public IEnumerable<Beer> GetBeers(string brewery)
        {
			foreach (var item in View("by_brewery").Key(brewery).Limit(10))
            {
                yield return Get(item.ItemId);
            }
        }
    }
}