using HomeControl.Helpers;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace HomeControl.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MediaController : Controller
    {
        [HttpPost]
        [Route(nameof(UploadFile))]
        [RequestSizeLimit(10L * 1024 * 1024 * 1024)] // 10 GB
        public async Task<IActionResult> UploadFile()
        {
            var request = HttpContext.Request;

            var directoryPath = HttpContext.Request.Query["directoryPath"];

            if (!request.HasFormContentType || !MediaTypeHeaderValue.TryParse(request.ContentType, out var mediaTypeHeader) || string.IsNullOrEmpty(mediaTypeHeader.Boundary.Value))
            {
                return new UnsupportedMediaTypeResult();
            }

            var boundary = HeaderUtilities.RemoveQuotes(mediaTypeHeader.Boundary.Value).Value;

            var reader = new MultipartReader(boundary, request.Body)
            {
                BodyLengthLimit = long.MaxValue,
            };

            var section = await reader.ReadNextSectionAsync();

            while (section != null)
            {
                var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition);

                if (hasContentDispositionHeader && contentDisposition.DispositionType.Equals("form-data") && !string.IsNullOrEmpty(contentDisposition.FileName.Value))
                {
                    var saveToPath = Path.Combine(Pages.Media.IndexModel.BasePath, directoryPath, contentDisposition.FileName.Value);

                    using (var targetStream = System.IO.File.Create(saveToPath))
                    {
                        await section.Body.CopyToAsync(targetStream);
                    }

                    return RedirectToPage("/Media/Index", new RouteValueDictionary() { { Pages.Media.IndexModel.DirectoryPathRouteDataKey, directoryPath } });
                }

                section = await reader.ReadNextSectionAsync();
            }

            return BadRequest("No files data in the request.");
        }
    }
}