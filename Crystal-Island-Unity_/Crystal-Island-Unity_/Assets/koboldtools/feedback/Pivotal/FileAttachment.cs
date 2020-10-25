using System;
using System.Collections.Generic;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

namespace KoboldTools.Feedback.Pivotal {
	[Serializable]
	public class FileAttachment {
		[JsonProperty("id")]
		public int Id;

		[JsonProperty("filename")]
		public string Filename;

		[JsonProperty("uploader_id")]
		public int UploaderId;

		[JsonProperty("thumbnailable")]
		public bool Thumbnailable;

		[JsonProperty("height", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int Height;

		[JsonProperty("width", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int Width;

		[JsonProperty("size")]
		public int Size;

		[JsonProperty("download_url")]
		public string DownloadUrl;

		[JsonProperty("content_type")]
		public string ContentType; 

		[JsonProperty("uploaded")]
		public bool Uploaded;

		[JsonProperty("big_url", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string BigUrl;

		[JsonProperty("thumbnail_url", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string ThumbnailUrl;

		[JsonProperty("kind", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string Kind;
	}
}