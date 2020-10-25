using System;
using System.Collections.Generic;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

namespace KoboldTools.Feedback.Pivotal {
	[Serializable]
	public class Label {
		[JsonProperty("id", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int Id;
		
		[JsonProperty("name")]
		public string Name;
	}
}