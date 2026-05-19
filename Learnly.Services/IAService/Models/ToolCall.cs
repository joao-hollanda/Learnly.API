namespace consumindoIA.Domain
{
    public class ToolCall
    {
        public string id { get; set; }
        public string type { get; set; }
        public FunctionCall function { get; set; }
    }

    public class FunctionCall
    {
        public string name { get; set; }
        public string arguments { get; set; }
    }
}
