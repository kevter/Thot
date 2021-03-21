namespace Thoth.Models
{
    public class Request
    {
        public string Method { get; set; }
        public string ContentType { get; set; }
        public string Endpoint { get; set; }
        public string Authorization { get; set; }
        public string Body { get; set; }
    }
}