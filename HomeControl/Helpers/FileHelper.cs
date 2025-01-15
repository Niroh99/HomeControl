using Microsoft.AspNetCore.StaticFiles;

namespace HomeControl.Helpers
{
    public class FileHelper
    {
        public const string ApplicationOctedStreamContentType = "application/octet-stream";
        public const string PlaintTextMimeType = "text/plain";

        public static FileExtensionContentTypeProvider CreateFileExtensionContentTypeProvider()
        {
            var fileExtensionContentTypeProvider = new FileExtensionContentTypeProvider();

            fileExtensionContentTypeProvider.Mappings[".ini"] = PlaintTextMimeType;
            fileExtensionContentTypeProvider.Mappings[".conf"] = PlaintTextMimeType;

            return fileExtensionContentTypeProvider;
        }
    }
}