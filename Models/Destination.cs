using System.Collections.Generic;

namespace Thoth.Models
{
    public class Destination
    {
        public string Path { get; set; }
        public bool RequiresAuthentication { get; set; }
        public List<string> HttpMethod { get; set; }
    }
}