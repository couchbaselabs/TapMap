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
using Couchbase.Configuration;
using TapMapWeb.Filters;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;

namespace TapMapWeb.Controllers
{
	[SessionFilter]
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
				import(client, "brewery", root, "breweries.zip");
				import(client, "beer", root, "beers.zip");
				import(client, "user", root, "users.zip");
				import(client, "tap", root, "taps.zip");
				
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

		private void import(CouchbaseClient client, string type, string rootDir, string zipFile)
		{
			unzipDataFiles(rootDir, zipFile);

			var dataFilesPath = Path.Combine(rootDir, zipFile.Replace(".zip", ""));
			var dir = new DirectoryInfo(dataFilesPath);
			foreach (var file in dir.GetFiles("*.json"))
			{								
				var json = System.IO.File.ReadAllText(file.FullName);
				var key = file.Name.Replace(file.Extension, "");
				
				var jObj = JsonConvert.DeserializeObject(json) as JObject;
				jObj.Remove("_id");
				jObj.Add("type", type);

				if (type == "beer")
				{
				    jObj["brewery"] = "brewery_" + jObj["brewery"].ToString().Replace(" ", "_");
				}

				var storeResult = client.Store(StoreMode.Set, key, jObj.ToString());				
			}
		}

		private void unzipDataFiles(string rootDir, string zipFile)
		{
			var zipFilePath = Path.Combine(rootDir, zipFile);
			var unzippedDirName = zipFile.Replace(".zip", "");
			var fs = System.IO.File.OpenRead(zipFilePath);
			var zf = new ZipFile(fs);

			foreach (ZipEntry entry in zf)
			{
				if (entry.IsDirectory)
				{
					var directoryName = Path.Combine(rootDir, entry.Name);
					if (!Directory.Exists(directoryName)) Directory.CreateDirectory(directoryName);
					continue;
				}

				var entryFileName = entry.Name;

				var buffer = new byte[4096];
				var zipStream = zf.GetInputStream(entry);

				var unzippedFilePath = Path.Combine(rootDir, entryFileName);

				using (var fsw = System.IO.File.Create(unzippedFilePath))
				{
					StreamUtils.Copy(zipStream, fsw, buffer);
				}

			}

			zf.IsStreamOwner = true;
			zf.Close();

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
			var requestUri = getBaseUri().AbsoluteUri + "couchBase/beernique/_design/" + docName;
			var request = (HttpWebRequest)HttpWebRequest.Create(requestUri);
			request.Method = verb;
			request.ContentType = "application/json";
			request.ContentLength = arr.Length;
			var dataStream = request.GetRequestStream();
			dataStream.Write(arr, 0, arr.Length);
			dataStream.Close();
			var response = (HttpWebResponse)request.GetResponse();
			return response.StatusCode.ToString();
		}

		private static Uri getBaseUri()
		{
			var config = (CouchbaseClientSection)ConfigurationManager.GetSection("couchbase");
			var firstUri = config.Servers.Urls.ToUriCollection().First();
			return new UriBuilder(firstUri.Scheme, firstUri.Host, firstUri.Port).Uri;
		}

	}
}
