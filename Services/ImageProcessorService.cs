using ImageProcessor.Models;
using Microsoft.AspNetCore.Hosting.Server;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Web;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IWebHostEnvironment;
using ImageModel = ImageProcessor.Models.ImageModel;

namespace ImageProcessor.Services
{

    public interface IImageProcessorService
    {
        Task<ImageModel> Resize(IFormFile file, int width, int height);
        Task<ImageModel> FetchImageByID(int id);
    }
    public class ImageProcessorService : IImageProcessorService
    {

        private IHostingEnvironment _env;

        public ImageProcessorService(IHostingEnvironment env)
        {
            _env = env;
        }
        //Modifying image using customize width and height
        public async Task<ImageModel> Resize(IFormFile file, int width = 0, int height = 0)
        {
            var imageModel = new ImageModel();
            var currentPath = file.FileName;

            var webRoot = _env.ContentRootPath;

            var targetPathToSave = $"{_env.ContentRootPath}wwwroot\\uploadedImages";

            var pathToFile = System.IO.Path.Combine(webRoot, currentPath);

            var originalWidth = 0;
            var originalHeight = 0;

            using (var originalImage = System.Drawing.Image.FromStream(file.OpenReadStream()))
            {
                originalWidth = originalImage.Width;
                originalHeight = originalImage.Height;
                width = width != 0 ? width : originalWidth;
                height = height != 0 ? height : originalHeight;
            }

            using (var imagestream = new FileStream(pathToFile, FileMode.Open, FileAccess.Read))
            using (var Originalimage = new Bitmap(imagestream))
            {
                var resizedImage = new Bitmap(width, height);
                using (var graphics = Graphics.FromImage(resizedImage))
                {
                    graphics.CompositingMode = CompositingMode.SourceCopy;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.CompositingQuality = CompositingQuality.HighSpeed;


                    float brightness = 1.0f;
                    float contrast = 2.0f;

                    float adjustedBrightness = brightness - 1.0f;
                    float[][] ptsArray ={
                    new float[] {contrast, 0, 0, 0, 0},
                    new float[] {0, contrast, 0, 0, 0},
                    new float[] {0, 0, contrast, 0, 0},
                    new float[] {0, 0, 0, 1.0f, 0},
                    new float[] {adjustedBrightness, adjustedBrightness, adjustedBrightness, 0, 1}};


                    ImageAttributes imageAttributes = new ImageAttributes();
                    imageAttributes.ClearColorMatrix();
                    imageAttributes.SetColorMatrix(new ColorMatrix(ptsArray), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                    graphics.DrawImage(Originalimage, new Rectangle(0, 0, width, height)
                    , 0, 0, originalWidth, originalHeight,
                    GraphicsUnit.Pixel, imageAttributes);

                    var newFilePath = $"{targetPathToSave}\\{Path.GetFileNameWithoutExtension(file.FileName)}_{width}x{height}.png";

                    imagestream.Dispose();
                    using (SqlConnection conn = new SqlConnection("Server=localhost;Database=master;Trusted_Connection=True;"))
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand("insert into image_master(Name,FileType,FileTypeExtension,FileSize,OriginalWidth," +
                            " OriginalHeight,TargetWidth,TargetHeight,SourcePath,TargetPath,UploadedOn)values" +
                            "(@imageName,'png','png',@imageSize,@originalWidth,@originalHeight,@targetWidth,@targetHeight,@currentPath,@newFilePath,@date)", conn);
                        cmd.Parameters.AddWithValue("@imageName", $"{Path.GetFileNameWithoutExtension(file.FileName)}_{ width}x{ height}.png)");
                        cmd.Parameters.AddWithValue("@imageSize", file.Length);
                        cmd.Parameters.AddWithValue("@originalWidth", Originalimage.Width);
                        cmd.Parameters.AddWithValue("@originalHeight", Originalimage.Height);
                        cmd.Parameters.AddWithValue("@targetHeight", height);
                        cmd.Parameters.AddWithValue("@targetWidth", width);
                        cmd.Parameters.AddWithValue("@currentPath", currentPath);
                        cmd.Parameters.AddWithValue("@newFilePath", newFilePath);
                        cmd.Parameters.AddWithValue("@date", DateTime.Now);

                        cmd.ExecuteNonQuery();
                        conn.Close();
                    }

                    imageModel.Name = $"{Path.GetFileNameWithoutExtension(file.FileName)}_{ width}x{ height}.png)";
                    imageModel.TargetWidth = width;
                    imageModel.TargetHeight = height;
                    imageModel.TargetPath = newFilePath;
                    resizedImage.Save(newFilePath, ImageFormat.Png);
                }
            }
            return imageModel;
        }

        //Modifying image using the predefined application type
        public async Task<ImageModel> ResizeByApplication(IFormFile file, ApplicationTypeEnum type)
        {
            var imageModel = new ImageModel();

            switch (type)
            {
                case ApplicationTypeEnum.Thumbnail:
                    imageModel = await Resize(file, 128, 128);
                    break;
                case ApplicationTypeEnum.Small:
                    imageModel = await Resize(file, 512, 512);
                    break;
                case ApplicationTypeEnum.Medium:
                    imageModel = await Resize(file, 1024, 1024);
                    break;
                case ApplicationTypeEnum.Large:
                    imageModel = await Resize(file, 2048, 2048);
                    break;
            }
            return imageModel;
        }

        //Retrieving Image Data
        public async Task<ImageModel> FetchImageByID(int id)
        {
            ImageModel imageData = new ImageModel();

            using (SqlConnection conn = new SqlConnection("Server=localhost;Database=master;Trusted_Connection=True;"))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM image_master WHERE imageId = " + id, conn);

                SqlDataReader sqlDataReader = await cmd.ExecuteReaderAsync();

                if (sqlDataReader.HasRows)
                {
                    while (sqlDataReader.Read())
                    {
                        imageData.Name = sqlDataReader.GetString(1);
                        imageData.TargetWidth = sqlDataReader.GetInt32(7);
                        imageData.TargetHeight = sqlDataReader.GetInt32(8);
                        imageData.TargetPath = sqlDataReader.GetString(10);
                        imageData.SourcePath = sqlDataReader.GetString(9);
                    }
                }

                conn.Close();
            }
            return imageData;
        }
    }
}