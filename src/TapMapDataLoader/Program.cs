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
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;

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
				import(client, "brewery", root, "breweries.zip");
				import(client, "beer", root, "beers.zip");
				import(client, "user", root, "users.zip");
				import(client, "tap", root, "taps.zip");
				
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

		private static void import(CouchbaseClient client, string type, string rootDir, string zipFile)
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
				Console.WriteLine("Store result for {0}: {1}", file.Name, storeResult);
			}
		}

		private static void unzipDataFiles(string rootDir, string zipFile)
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
				Console.WriteLine("Unzipping {0}", entryFileName);

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
