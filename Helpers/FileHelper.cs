using System;
using System.IO;
using System.Linq;
using System.Web;

namespace UniManage.Helpers
{
    public static class FileHelper
    {
        private static readonly string[] AllowedExtensions = { ".pdf", ".doc", ".docx", ".pptx", ".ppt", ".xlsx", ".xls", ".txt", ".jpg", ".jpeg", ".png", ".gif", ".zip", ".rar" };
        private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

        public static string SaveFile(HttpPostedFileBase file, string subFolder)
        {
            if (file == null || file.ContentLength == 0) return null;

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!AllowedExtensions.Contains(extension))
                throw new Exception("Invalid file type.");

            if (file.ContentLength > MaxFileSize)
                throw new Exception("File size exceeds 10MB limit.");

            string uploadsPath = HttpContext.Current.Server.MapPath("~/Uploads/" + subFolder);
            if (!Directory.Exists(uploadsPath))
                Directory.CreateDirectory(uploadsPath);

            string sanitizedFileName = SanitizeFileName(file.FileName);
            string uniqueFileName = $"{Guid.NewGuid()}_{sanitizedFileName}";
            string filePath = Path.Combine(uploadsPath, uniqueFileName);

            file.SaveAs(filePath);

            return "/Uploads/" + subFolder + "/" + uniqueFileName;
        }

        public static void DeleteFile(string virtualPath)
        {
            if (string.IsNullOrEmpty(virtualPath)) return;

            string physicalPath = HttpContext.Current.Server.MapPath("~" + virtualPath);
            if (File.Exists(physicalPath))
            {
                File.Delete(physicalPath);
            }
        }

        private static string SanitizeFileName(string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = new string(fileName
                .Where(c => !invalidChars.Contains(c))
                .ToArray());
            return sanitized.Replace(" ", "-");
        }
    }
}
