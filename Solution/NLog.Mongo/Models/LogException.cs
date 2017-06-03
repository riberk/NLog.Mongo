namespace NLog.Mongo.Models
{
    public class LogException
    {
        public string Message { get; set; }

        public string BaseMessage { get; set; }

        public string Text { get; set; }

        public string Type { get; set; }

        public string Stack { get; set; }

        public int? ErrorCode { get; set; }

        public string Source { get; set; }

        public string MethodName { get; set; }

        public string ModuleName { get; set; }

        public string ModuleVersion { get; set; }
    }
}