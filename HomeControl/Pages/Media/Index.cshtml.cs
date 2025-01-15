using HomeControl.Helpers;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.FileProviders;
using Microsoft.Net.Http.Headers;

namespace HomeControl.Pages.Media
{
    public class IndexModel : MenuPageModel
    {
        public const string MenuItem = "Media";
        public const string PageUrl = "/Media";

        public const string DirectoryPathRouteDataKey = "MediaDirectoryPath";
        public const string NavigateDirectoryRouteDataKey = "MediaNavigateDirectory";

        public const string BasePath = "/media";

        public bool BasePathExists { get => Directory.Exists(BasePath); }

        public bool HasMedia { get => Directories.Count > 0 || Files.Count > 0; }

        public bool DirectoryExists { get => Directory.Exists(Path.Combine(BasePath, GetDirectoryPath())); }

        public List<DirectoryInfo> Directories { get; } = new List<DirectoryInfo>();

        public List<FileInfo> Files { get; } = new List<FileInfo>();

        private IFileProvider _fileProvider;
        private static readonly FileExtensionContentTypeProvider _fileExtensionContentTypeProvider = FileHelper.CreateFileExtensionContentTypeProvider();
        private static readonly string[] DisplayableMimeTypeCategoriess = ["video/", "audio/", "image/", "text/"];
        private static readonly string[] NonDisplayableMimeTypes = ["text/x-uuencode"];

        public IndexModel(IFileProvider fileProvider) : base(MenuItem)
        {
            _fileProvider = fileProvider;
        }

        public override IActionResult OnGet()
        {
            base.OnGet();

            if (!BasePathExists) return null;

            var directoryPath = GetDirectoryPath();

            if (Request.Query.ContainsKey(NavigateDirectoryRouteDataKey))
            {
                directoryPath = Path.Combine(directoryPath, Request.Query[NavigateDirectoryRouteDataKey]);

                return Redirect($"/Media?{DirectoryPathRouteDataKey}={directoryPath}");
            }

            foreach (var directoryContent in _fileProvider.GetDirectoryContents(directoryPath).OrderBy(x => x.IsDirectory))
            {
                if (directoryContent.IsDirectory) Directories.Add(new DirectoryInfo(directoryContent.PhysicalPath));
                else Files.Add(new FileInfo(directoryContent.PhysicalPath));
            }

            return null;
        }

        public string GetDisplayPath()
        {
            var directoryPath = GetDirectoryPath();

            if (string.IsNullOrEmpty(directoryPath)) return "Media";

            return directoryPath;
        }

        public string BuildNavigateQuery(string directory)
        {
            var directoryPath = GetDirectoryPath();

            return $"{PageUrl}?{DirectoryPathRouteDataKey}={directoryPath}&{NavigateDirectoryRouteDataKey}={directory}";
        }

        public bool CanBrowserDisplayFile(FileInfo file)
        {
            if (!_fileExtensionContentTypeProvider.TryGetContentType(file.FullName, out var contentType)) return false;

            if (NonDisplayableMimeTypes.Contains(contentType)) return false;

            if (DisplayableMimeTypeCategoriess.Any(contentType.StartsWith)) return true;

            return false;
        }

        public string BuildFileQuery(string filename)
        {
            return Path.Combine(PageUrl, GetDirectoryPath(), filename);
        }

        public string GetDirectoryPath()
        {
            if (Request.Query.ContainsKey(DirectoryPathRouteDataKey)) return Request.Query[DirectoryPathRouteDataKey];

            return "";
        }

        public IActionResult OnPostDeleteFile(string directoryPath, string filename)
        {
            var filepath = Path.Combine(directoryPath, filename);

            var fileInfo = _fileProvider.GetFileInfo(filepath);

            if (fileInfo.Exists) System.IO.File.Delete(fileInfo.PhysicalPath);

            return Redirect($"/Media?{DirectoryPathRouteDataKey}={directoryPath}");
        }
    }
}