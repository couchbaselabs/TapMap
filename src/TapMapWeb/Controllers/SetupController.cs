using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Couchbase;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Enyim.Caching.Memcached;
using System.Net;
using System.Configuration;

namespace TapMapWeb.Controllers
{
    public class SetupController : Controller
    {
        //
        // GET: /Setup/
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(string setupKey)
        {
			try
			{
				if (setupKey != ConfigurationManager.AppSettings["TapMapSetupKey"])
				{
					throw new ApplicationException("Invalid TapMapSetupKey.  Please review the appSetting \"TapMapSetupKey\" in Web.config");
				}

				var client = new CouchbaseClient();

				var root = Path.Combine(Environment.CurrentDirectory, Server.MapPath("~/App_Data"));
				import(client, "brewery", Path.Combine(root, "breweries"));
				import(client, "beer", Path.Combine(root, "beers"));
				import(client, "user", Path.Combine(root, "users"));
				import(client, "tap", Path.Combine(root, "taps"));
				
				createViewFromFile(Path.Combine(root, @"views\UserViews.json"), "users");
				createViewFromFile(Path.Combine(root, @"views\BreweryViews.json"), "breweries");
				createViewFromFile(Path.Combine(root, @"views\BeerViews.json"), "beers");
				createViewFromFile(Path.Combine(root, @"views\TapViews.json"), "taps");

				ViewBag.Message = "Successfully loaded views and data";
				ViewBag.MessageColor = "green";
			}
			catch (Exception ex)
			{
				ViewBag.Message = ex.Message;
				ViewBag.MessageColor = "red";
			}

			return View();
        }

		private void import(CouchbaseClient client, string type, string directory)
		{
			var dir = new DirectoryInfo(directory);
			foreach (var file in dir.GetFiles())
			{
				if (file.Extension != ".json") continue;
				Console.WriteLine("Adding {0}", file);

				var json = System.IO.File.ReadAllText(file.FullName);
				var key = file.Name.Replace(file.Extension, "");
				json = Regex.Replace(json.Replace(key, "LAZY"), "\"_id\":\"LAZY\",", "");
				var jObj = JsonConvert.DeserializeObject(json) as JObject;
				jObj.Add("type", type);

				if (type == "beer")
				{
					jObj["brewery"] = "brewery_" + jObj["brewery"].ToString().Replace(" ", "_");
				}

				var storeResult = client.Store(StoreMode.Set, key, jObj.ToString());
				Console.WriteLine(storeResult);
			}
		}

		private static void createViewFromFile(string viewFile, string docName)
		{
			Console.WriteLine("Status for DELETE {0}: {1}", docName, request(viewFile, docName, "DELETE"));
			Console.WriteLine("Status for GET {0}: {1}", docName, request(viewFile, docName, "PUT"));
		}

		private static string request(string viewFile, string docName, string verb)
		{
			var viewContent = System.IO.File.ReadAllText(viewFile);
			byte[] arr = System.Text.Encoding.UTF8.GetBytes(viewContent);
			var request = (HttpWebRequest)HttpWebRequest.Create("http://localhost:8091/couchBase/beernique/_design/" + docName);
			request.Method = verb;
			request.ContentType = "application/json";
			request.ContentLength = arr.Length;
			var dataStream = request.GetRequestStream();
			dataStream.Write(arr, 0, arr.Length);
			dataStream.Close();
			var response = (HttpWebResponse)request.GetResponse();
			return response.StatusCode.ToString();
		}

	}
}
