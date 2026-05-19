using System.Collections.Generic;

namespace consumindoIA.Domain
{
    public class Tool
    {
        public string type { get; set; } = "function";
        public ToolFunction function { get; set; }
    }

    public class ToolFunction
    {
        public string name { get; set; }
        public string description { get; set; }
        public object parameters { get; set; }
    }
}
