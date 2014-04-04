<%@ WebHandler Language="C#" Class="Handler" %>
/**
 * tinymce.gzip.ashx
 *
 * Copyright, Moxiecode Systems AB
 * Released under LGPL License.
 *
 * License: http://tinymce.moxiecode.com/license
 * Contributing: http://tinymce.moxiecode.com/contributing
 *
 * This file compresses the TinyMCE JavaScript using GZip and
 * enables the browser to do two requests instead of one for each .js file.
 *
 * It's a good idea to use the diskcache option since it reduces the servers workload.
 */

using System;
using System.Web;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

public class Handler : IHttpHandler {
	private HttpResponse Response;
	private HttpRequest Request;	
	private HttpServerUtility Server;

	public void ProcessRequest(HttpContext context) {
		this.Response = context.Response;
		this.Request = context.Request;
		this.Server = context.Server;
		this.StreamGzipContents();
	}

	public bool IsReusable {
		get {
			return false;
		}
	}

	#region private

	private void StreamGzipContents() {
		string cacheKey = "", cacheFile = "", content = "", enc, suffix, cachePath;
		string[] plugins, languages, themes;
		bool diskCache, supportsGzip, isJS, compress, core;
		int i, x, expiresOffset;
		GZipStream gzipStream;
		Encoding encoding = Encoding.GetEncoding("windows-1252");
		byte[] buff;

		// Get input
		plugins = GetParam("plugins", "").Split(',');
		languages = GetParam("languages", "").Split(',');
		themes = GetParam("themes", "").Split(',');
		diskCache = GetParam("diskcache", "") == "true";
		isJS = GetParam("js", "") == "true";
		compress = GetParam("compress", "true") == "true";
		core = GetParam("core", "true") == "true";
		suffix = GetParam("suffix", ".min");
		cachePath = Server.MapPath("."); // Cache path, this is where the .gz files will be stored
		expiresOffset = 10; // Cache for 10 days in browser cache

		// Custom extra javascripts to pack
		string[] custom = {/*
			"some custom .js file",
			"some custom .js file"
		*/};

		// Set response headers
		Response.ContentType = "text/javascript";
		Response.Charset = "UTF-8";
		Response.Buffer = false;

		// Setup cache
		Response.Cache.SetExpires(DateTime.Now.AddDays(expiresOffset));
		Response.Cache.SetCacheability(HttpCacheability.Public);
		Response.Cache.SetValidUntilExpires(false);

		// Vary by all parameters and some headers
		Response.Cache.VaryByHeaders["Accept-Encoding"] = true;
		Response.Cache.VaryByParams["theme"] = true;
		Response.Cache.VaryByParams["language"] = true;
		Response.Cache.VaryByParams["plugins"] = true;
		Response.Cache.VaryByParams["lang"] = true;
		Response.Cache.VaryByParams["index"] = true;

		// Setup cache info
		if (diskCache) {
			cacheKey = GetParam("plugins", "") + GetParam("languages", "") + GetParam("themes", "");

			for (i = 0; i < custom.Length; i++)
				cacheKey += custom[i];

			cacheKey = MD5(cacheKey);

			if (compress)
				cacheFile = cachePath + "/tinymce.gzip-" + cacheKey + ".gz";
			else
				cacheFile = cachePath + "/tinymce.gzip-" + cacheKey + ".js";
		}

		// Check if it supports gzip
		enc = Regex.Replace("" + Request.Headers["Accept-Encoding"], @"\s+", "").ToLower();
		supportsGzip = enc.IndexOf("gzip") != -1 || Request.Headers["---------------"] != null;
		enc = enc.IndexOf("x-gzip") != -1 ? "x-gzip" : "gzip";

		// Use cached file disk cache
		if (diskCache && supportsGzip && File.Exists(cacheFile)) {
			Response.AppendHeader("Content-Encoding", enc);
			Response.WriteFile(cacheFile);
			return;
		}

		// Add core
		if (core) {
			// Set base URL for where tinymce is loaded from
			String uri = Request.Url.AbsolutePath;
			uri = uri.Substring(0, uri.LastIndexOf('/'));
			content += "var tinymce={base:'" + uri + "',suffix:'.min'};";
			content += GetFileContents("tinymce." + suffix + ".js");
		}

		// Add core languages
		for (x = 0; x < languages.Length; x++)
			content += GetFileContents("langs/" + languages[x] + ".js");

		// Add themes
		for (i = 0; i < themes.Length; i++) {
			content += GetFileContents("themes/" + themes[i] + "/theme." + suffix + ".js");

			for (x = 0; x < languages.Length; x++)
				content += GetFileContents("themes/" + themes[i] + "/langs/" + languages[x] + ".js");
		}

		// Add plugins
		for (i = 0; i < plugins.Length; i++) {
			content += GetFileContents("plugins/" + plugins[i] + "/plugin." + suffix + ".js");

			for (x = 0; x < languages.Length; x++)
				content += GetFileContents("plugins/" + plugins[i] + "/langs/" + languages[x] + ".js");
		}

		// Add custom files
		for (i = 0; i < custom.Length; i++)
			content += GetFileContents(custom[i]);

		// Generate GZIP'd content
		if (supportsGzip) {
			if (compress)
				Response.AppendHeader("Content-Encoding", enc);

			if (diskCache && cacheKey != "") {
				// Gzip compress
				if (compress) {
					using (Stream fileStream = File.Create(cacheFile)) {
						gzipStream = new GZipStream(fileStream, CompressionMode.Compress, true);
						buff = encoding.GetBytes(content.ToCharArray());
						gzipStream.Write(buff, 0, buff.Length);
						gzipStream.Close();
					}
				} else {
					using (StreamWriter sw = File.CreateText(cacheFile)) {
						sw.Write(content);
					}
				}

				// Write to stream
				Response.WriteFile(cacheFile);
			} else {
				gzipStream = new GZipStream(Response.OutputStream, CompressionMode.Compress, true);
				buff = encoding.GetBytes(content.ToCharArray());
				gzipStream.Write(buff, 0, buff.Length);
				gzipStream.Close();
			}
		} else
			Response.Write(content);
	}

	private string GetParam(string name, string def) {
		string value = !String.IsNullOrEmpty(Request.QueryString[name]) ? "" + Request.QueryString[name] : def;

		return Regex.Replace(value, @"[^0-9a-zA-Z\\-_,]+", "");
	}

	private string GetFileContents(string path) {
		try {
			string content;

			path = Server.MapPath(path);

			if (!File.Exists(path))
				return "";

			StreamReader sr = new StreamReader(path);
			content = sr.ReadToEnd();
			sr.Close();

			return content;
		} catch (Exception ex) {
			// Ignore any errors
		}

		return "";
	}

	private string MD5(string str) {
		MD5 md5 = new MD5CryptoServiceProvider();
		byte[] result = md5.ComputeHash(Encoding.ASCII.GetBytes(str));
		str = BitConverter.ToString(result);

		return str.Replace("-", "");
	}

	#endregion
}