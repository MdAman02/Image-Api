using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ImageApi.RequestModels;
using ImageApi.ResponseModels;
using ImageApi.Services;

namespace ImageApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private readonly IImageService _imageService;
        public ImageController(IImageService service)
        {
            _imageService = service;
        }

        [HttpPost()]
        public async Task<IActionResult> DownloadImage (ImageDownloadRequestModel request)
        {
            if (request.MaxDownloadAtOnce < 1)
                return BadRequest();
            var data = await _imageService.DownloadAndStoreImage(request);
            return Ok(data);
        }

        [HttpGet("{imageName}")]
        public async Task<IActionResult> GetImage ([FromRoute] string imageName)
        {
            var imageBase64 = _imageService.GetImageByName(imageName);
            return Ok(imageBase64);
        }
    }
}
