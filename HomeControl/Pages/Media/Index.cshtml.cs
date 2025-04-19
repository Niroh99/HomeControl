using HomeControl.Attributes;
using HomeControl.Helpers;
using HomeControl.Views.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;

namespace HomeControl.Pages.Media
{
    [MenuPage(null, "Media", "/Media/Index")]
    public class IndexModel : PageModel
    {
        public const string PageUrl = "/Media";

        public const string DirectoryPathRouteDataKey = "MediaDirectoryPath";
        public const string NavigateDirectoryRouteDataKey = "MediaNavigateDirectory";

        public static readonly string BasePath = $"/media/{Environment.UserName}";

        public bool BasePathExists { get => Directory.Exists(BasePath); }

        public DirectoryInfo _baseDirectory;
        public DirectoryInfo BaseDirectory
        {
            get
            {
                _baseDirectory ??= new DirectoryInfo(BasePath);

                return _baseDirectory;
            }
        }

        public bool HasMedia { get => Directories.Count > 0 || Files.Count > 0; }

        public string CurrentDirectoryPath
        {
            get
            {
                if (Request.Query.ContainsKey(DirectoryPathRouteDataKey)) return Request.Query[DirectoryPathRouteDataKey];

                return "";
            }
        }

        public bool HasCurrentDirectoryPath { get => !string.IsNullOrEmpty(CurrentDirectoryPath); }

        public bool CurrentDirectoryExists { get => Directory.Exists(Path.Combine(BasePath, CurrentDirectoryPath)); }

        public bool MediaSelected { get => HasCurrentDirectoryPath && CurrentDirectoryExists; }

        public bool DirectorySelected
        {
            get
            {
                if (CurrentDirectoryExists && MediaSelected) return !IsParentBaseDirectory(CurrentDirectory);

                return false;
            }
        }

        private DirectoryInfo _currentDirectory;
        public DirectoryInfo CurrentDirectory
        {
            get
            {
                _currentDirectory ??= new DirectoryInfo(Path.Combine(BasePath, CurrentDirectoryPath));

                return _currentDirectory;
            }
        }

        public List<DirectoryInfo> Directories { get; } = new List<DirectoryInfo>();

        public List<FileInfo> Files { get; } = new List<FileInfo>();

        private readonly IFileProvider _fileProvider;
        private static readonly FileExtensionContentTypeProvider _fileExtensionContentTypeProvider = FileHelper.CreateFileExtensionContentTypeProvider();
        private static readonly string[] DisplayableMimeTypeCategoriess = ["video/", "audio/", "image/", "text/"];
        private static readonly string[] DisplayableMimeTypes = ["application/pdf"];
        private static readonly string[] NonDisplayableMimeTypes = ["text/x-uuencode"];

        public IndexModel(IFileProvider fileProvider)
        {
            _fileProvider = fileProvider;
        }

        public IActionResult OnGet()
        {
            if (!BasePathExists) return null;

            var directoryPath = CurrentDirectoryPath;

            if (Request.Query.ContainsKey(NavigateDirectoryRouteDataKey))
            {
                directoryPath = Path.Combine(directoryPath, Request.Query[NavigateDirectoryRouteDataKey]);

                return Redirect($"{PageUrl}?{DirectoryPathRouteDataKey}={System.Net.WebUtility.UrlEncode(directoryPath)}");
            }

            foreach (var directoryContent in _fileProvider.GetDirectoryContents(directoryPath).OrderBy(x => x.IsDirectory))
            {
                if (directoryContent.IsDirectory) Directories.Add(new DirectoryInfo(directoryContent.PhysicalPath));
                else Files.Add(new FileInfo(directoryContent.PhysicalPath));
            }

            return null;
        }

        public IEnumerable<Breadcrumb> GetBreadcrumbs()
        {
            var directoryPath = CurrentDirectoryPath;

            if (string.IsNullOrEmpty(directoryPath)) yield break;

            var directory = CurrentDirectory;

            bool first = true;

            while (true)
            {
                directoryPath = Path.GetRelativePath(BasePath, directory.FullName);

                yield return new Breadcrumb(directory.Name, first ? null : $"{PageUrl}?{DirectoryPathRouteDataKey}={directoryPath}", "h3", "breadcrumb-a");

                if (first) first = false;

                if (IsParentBaseDirectory(directory)) break;

                directory = directory.Parent;
            }
        }

        public bool CanBrowserDisplayFile(FileInfo file)
        {
            if (!_fileExtensionContentTypeProvider.TryGetContentType(file.FullName, out var contentType)) return false;

            if (NonDisplayableMimeTypes.Contains(contentType)) return false;

            if (DisplayableMimeTypes.Contains(contentType)) return true;

            if (DisplayableMimeTypeCategoriess.Any(contentType.StartsWith)) return true;

            return false;
        }

        public string BuildFileQuery(string filename)
        {
            return Path.Combine(PageUrl, CurrentDirectoryPath, filename);
        }

        public string FileSizeDisplay(long byteCount, string format = "n2")
        {
            if (byteCount > 1000000000) return $"{((decimal)byteCount / 1000000000).ToString(format)} GB";

            if (byteCount > 1000000) return $"{((decimal)byteCount / 1000000).ToString(format)} MB";

            if (byteCount > 1000) return $"{((decimal)byteCount / 1000).ToString(format)} KB";

            return $"{((decimal)byteCount / 1000).ToString(format)} B";
        }

        public IActionResult OnPostDeleteFile(string directoryPath, string filename)
        {
            var filepath = Path.Combine(directoryPath, filename);

            var fileInfo = _fileProvider.GetFileInfo(filepath);

            if (fileInfo.Exists) System.IO.File.Delete(fileInfo.PhysicalPath);

            return Redirect($"{PageUrl}?{DirectoryPathRouteDataKey}={directoryPath}");
        }

        public IActionResult OnPostCreateDirectory(string directoryPath, string directory)
        {
            var newDirectoryPath = Path.Combine(directoryPath, directory);

            Directory.CreateDirectory(Path.Combine(BasePath, newDirectoryPath));

            return Redirect($"{PageUrl}?{DirectoryPathRouteDataKey}={newDirectoryPath}");
        }

        public IActionResult OnPostRenameDirectory(string directoryPath, string newDirectoryName)
        {
            var currentDirectory = Path.GetFullPath(Path.Combine(BasePath, directoryPath));

            var parentDirectory = Directory.GetParent(currentDirectory);

            var newDirectoryPath = Path.Combine(parentDirectory.FullName, newDirectoryName);

            Directory.Move(currentDirectory, newDirectoryPath);

            return Redirect($"{PageUrl}?{DirectoryPathRouteDataKey}={Path.GetRelativePath(BasePath, newDirectoryPath)}");
        }

        private bool IsParentBaseDirectory(DirectoryInfo directory)
        {
            return directory.Parent.FullName == BaseDirectory.FullName;
        }
    }
}