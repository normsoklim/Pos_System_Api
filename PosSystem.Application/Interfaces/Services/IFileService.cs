 
using System;  
using System.Threading.Tasks;  
  
namespace PosSystem.Application.Interfaces.Services  
{  
    public interface IFileService  
    {  
        Task<bool> DeleteFileAsync(string filePath);  
        Task<string> SaveFileAsync(byte[] fileData, string fileName, string folderPath, string allowedExtensions = ".jpg,.jpeg,.png,.webp");  
    }  
} 
