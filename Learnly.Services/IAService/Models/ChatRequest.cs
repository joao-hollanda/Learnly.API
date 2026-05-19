using System.Collections.Generic;
using Newtonsoft.Json;

namespace consumindoIA.Domain
{
    public class ChatRequest
    {
        public string model { get; set; } = "";
        public List<Message> messages { get; set; } = new();
        public double temperature { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<Tool> tools { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object tool_choice { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? max_tokens { get; set; } = 15000;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? parallel_tool_calls { get; set; }
    }
}