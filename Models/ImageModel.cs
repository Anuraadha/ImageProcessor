namespace ImageProcessor.Models
{
    public class ImageModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? FileType { get; set; }
        public string? FileTypeExtension { get; set; }
        public int FileSize { get; set; }
        public int OriginalWidth { get; set; }
        public int OriginalHeight { get; set; }
        public int TargetWidth { get; set; }
        public int TargetHeight { get; set; }
        public string SourcePath { get; set; }
        public string TargetPath { get; set; }
        public DateTimeOffset UploadedOn { get; set; }
    }
}
