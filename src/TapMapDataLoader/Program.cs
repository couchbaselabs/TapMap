using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using InflectorNet = Inflector.Net.Inflector;
using Couchbase;
using Enyim.Caching.Memcached;
using Couchbase.Configuration;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using System.Net;

namespace TapMapDataLoader
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				var client = new CouchbaseClient();

				var root = Path.Combine(Environment.CurrentDirectory, @"..\..\..\..\beer-sample");
				import(client, "brewery", Path.Combine(root, "breweries"));
				import(client, "beer", Path.Combine(root, "beer"));
				
				createViewFromFile(@"Views\UserViews.json", "users");
				createViewFromFile(@"Views\BreweryViews.json", "breweries");
				createViewFromFile(@"Views\BeerViews.json", "beers");
				createViewFromFile(@"Views\TapViews.json", "taps");

			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}

		private static void import(CouchbaseClient client, string type, string directory)
		{
			var dir = new DirectoryInfo(directory);
			foreach (var file in dir.GetFiles())
			{
				if (file.Extension != ".json") continue;
				Console.WriteLine("Adding {0}", file);

				var json = File.ReadAllText(file.FullName);
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
			var viewContent = File.ReadAllText(viewFile);
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
