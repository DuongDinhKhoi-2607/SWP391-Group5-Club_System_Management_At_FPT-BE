using BussinessLayer.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace BussinessLayer.Services
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IConfiguration config)
        {
            var acc = new Account(
                config["CloudinarySettings:CloudName"],
                config["CloudinarySettings:ApiKey"],
                config["CloudinarySettings:ApiSecret"]
            );

            _cloudinary = new Cloudinary(acc);
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folder)
        {
            if (file.Length == 0)
                throw new Exception("File không có dữ liệu.");

            var uploadResult = new RawUploadResult();

            using (var stream = file.OpenReadStream())
            {
                var isVideo = file.ContentType.StartsWith("video/");

                if (isVideo)
                {
                    var uploadParams = new VideoUploadParams()
                    {
                        File = new FileDescription(file.FileName, stream),
                        Folder = folder
                    };
                    uploadResult = await _cloudinary.UploadAsync(uploadParams);
                }
                else
                {
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(file.FileName, stream),
                        Folder = folder
                    };
                    uploadResult = await _cloudinary.UploadAsync(uploadParams);
                }
            }

            if (uploadResult.Error != null)
                throw new Exception(uploadResult.Error.Message);

            return uploadResult.SecureUrl.ToString();
        }
    }
}
