using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace consumindoIA.Domain
{
    public class Message
    {
        public string role { get; set; } = "";
        public string? content { get; set; }

        public string? name { get; set; }
        public string? tool_call_id { get; set; }
        public List<ToolCall> tool_calls { get; set; }
    }
}