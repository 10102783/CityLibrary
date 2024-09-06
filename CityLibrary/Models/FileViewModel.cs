namespace CityLibrary.Models
{
    public class FileViewModel
    {
        public IFormFile File { get; set; }
        public IEnumerable<string> Files { get; set; }
    }
}
