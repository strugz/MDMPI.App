using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MDMPI.App.Core.Common.Services
{
    public class ImageService
    {
        public static string SaveImageToDirectory(byte[] imageBytes, string fileName, string directoryPath)
        {
            if (imageBytes == null || imageBytes.Length == 0)
                throw new ArgumentException("Image data is empty.", nameof(imageBytes));

            // Ensure the directory exists
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            // Build the full file path
            var filePath = Path.Combine(directoryPath, fileName);

            // Save the image bytes to the file
            File.WriteAllBytes(filePath, imageBytes);

            return filePath; // Return the path for reference/storage
        }

        // Ensures the images directory exists and returns its path
        public static string GetImageDirectory()
        {
            var directoryPath = Path.Combine(Environment.CurrentDirectory, "Images");
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);
            return directoryPath;
        }
    }
}
