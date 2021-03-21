namespace Thoth.Models
{
    public class Route
    {
        public string Endpoint { get; set; }
        public Destination Destination { get; set; }
        public Cache Cache { get; set; }
    }
}