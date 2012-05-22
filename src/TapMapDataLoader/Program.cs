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

namespace TapMapDataLoader
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				var config = new CouchbaseClientConfiguration();
				config.Urls.Add(new Uri("http://localhost:8091/pools/default"));
				config.Bucket = "beernique";
				config.BucketPassword = "b33rs";

				var client = new CouchbaseClient(config);

				var root = Path.Combine(Environment.CurrentDirectory, @"..\..\..\..\beer-sample");
				import(client, "brewery", Path.Combine(root, "breweries"));
				import(client, "beer", Path.Combine(root, "beer"));

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
	}
}
