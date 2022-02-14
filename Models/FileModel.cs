namespace ImageProcessor.Models
{
    public class FileModel
    {
        public IFormFile File { get; set; }
        public ApplicationTypeEnum type { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
