using System;
using System.Collections.Generic;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

namespace KoboldTools.Feedback.Pivotal {
    [Serializable]
    public class Story {
        [JsonProperty("id", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Id;

        [JsonProperty("name")]
        public string Name;

        [JsonProperty("description")]
        public string Description;

        [JsonProperty("labels", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<Label> Labels;

        [JsonProperty("story_type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public StoryType StoryType;

        [JsonProperty("current_state")]
        [JsonConverter(typeof(StringEnumConverter))]
        public StoryState CurrentState;

        [JsonProperty("requested_by_id", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int RequestedById;

        [JsonProperty("owner_ids", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<int> OwnerIds;

        [JsonProperty("url", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Url;
    }
}