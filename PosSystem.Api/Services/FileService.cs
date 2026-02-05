using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using PosSystem.Application.Interfaces.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PosSystem.Api.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _env;

        public FileService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<bool> DeleteFileAsync(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                    return false;

                // Convert URL path to physical file path
                var fileName = Path.GetFileName(filePath);
                var fullPath = Path.Combine(_env.WebRootPath, "images", "products", fileName);

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                // Log the exception in a real application
                return false;
            }
        }

        public async Task<string> SaveFileAsync(byte[] fileData, string fileName, string folderPath, string allowedExtensions = ".jpg,.jpeg,.png,.webp")
        {
            try
            {
                var extension = Path.GetExtension(fileName).ToLower();
                var allowedExts = allowedExtensions.Split(',');

                if (Array.IndexOf(allowedExts, extension) < 0)
                    throw new ArgumentException("Invalid file format");

                // Ensure full directory path exists
                var fullFolderPath = Path.Combine(_env.WebRootPath, folderPath);
                Directory.CreateDirectory(fullFolderPath);

                // Create unique filename
                var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(fullFolderPath, uniqueFileName);

                // Save file
                await File.WriteAllBytesAsync(filePath, fileData);

                // Create image URL
                return $"/{folderPath}/{uniqueFileName}";
            }
            catch (Exception)
            {
                // Log the exception in a real application
                throw;
            }
        }
    }
}