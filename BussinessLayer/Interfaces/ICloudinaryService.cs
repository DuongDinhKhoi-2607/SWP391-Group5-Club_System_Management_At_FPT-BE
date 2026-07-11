using Microsoft.AspNetCore.Http;

namespace BussinessLayer.Interfaces
{
    public interface ICloudinaryService
    {
        Task<string> UploadFileAsync(IFormFile file, string folder);
    }
}
