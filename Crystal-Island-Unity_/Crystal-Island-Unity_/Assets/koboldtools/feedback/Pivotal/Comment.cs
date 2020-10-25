using System;
using System.Collections.Generic;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

namespace KoboldTools.Feedback.Pivotal {
	[Serializable]
	public class Comment {
		[JsonProperty("text")]
		public string Text;

		[JsonProperty("file_attachments", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public List<FileAttachment> FileAttachments;
	}
}