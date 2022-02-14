using ImageProcessor.Models;
using ImageProcessor.Services;
using Microsoft.AspNetCore.Mvc;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IWebHostEnvironment;
using System.Diagnostics;

namespace ImageProcessor.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHostingEnvironment _hostingEnvironment;

        private readonly ImageProcessorService _ImageProcessorService;

        public HomeController(ILogger<HomeController> logger,
             IHostingEnvironment hostingEnvironment,
            ImageProcessorService imageProcessorService)
        {
            _logger = logger;
            _hostingEnvironment = hostingEnvironment;
            _ImageProcessorService = imageProcessorService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("upload")]
        public IActionResult FetchResizedImage(IFormFile file, int width , int height)
        {
            _ImageProcessorService.Resize(file, width , height);
            return Ok("hello");
        }

        [HttpPost("upload/{type}")]
        public async Task<IActionResult> FetchResizedImageByApplication(IFormFile file, ApplicationTypeEnum type)
        {
            var path  = await _ImageProcessorService.ResizeByApplication(file,type);
            return Ok(path);
        }

        [HttpGet("image/{id}")]
        public async Task<IActionResult> GetImageData(int id)
        {
                var data =  await _ImageProcessorService.FetchImageByID(id);
                return Ok(data);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}