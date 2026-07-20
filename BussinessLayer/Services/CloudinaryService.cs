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
                var isImage = file.ContentType.StartsWith("image/");

                if (isVideo)
                {
                    var uploadParams = new VideoUploadParams()
                    {
                        File = new FileDescription(file.FileName, stream),
                        Folder = folder
                    };
                    uploadResult = await _cloudinary.UploadLargeAsync(uploadParams);
                }
                else if (isImage)
                {
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(file.FileName, stream),
                        Folder = folder
                    };
                    uploadResult = await _cloudinary.UploadLargeAsync(uploadParams);
                }
                else
                {
                    // Dùng RawUploadParams cho các file tài liệu (pdf, docx, zip, ...)
                    var uploadParams = new RawUploadParams()
                    {
                        File = new FileDescription(file.FileName, stream),
                        Folder = folder
                    };
                    uploadResult = await _cloudinary.UploadLargeAsync(uploadParams);
                }
            }

            if (uploadResult.Error != null)
                throw new Exception(uploadResult.Error.Message);

            return uploadResult.SecureUrl.ToString();
        }

        public async Task DeleteFileAsync(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl)) return;

            try
            {
                var uri = new Uri(fileUrl);
                var segments = uri.Segments;
                // segments looks like: ["/", "yikqzell/", "raw/", "upload/", "v1784571155/", "documents/", "tpytz9ql2ywhwj5rx3xc.txt"]
                
                var uploadIndex = Array.IndexOf(segments, "upload/");
                if (uploadIndex >= 0 && uploadIndex + 2 < segments.Length)
                {
                    // Lấy tất cả các segment sau version (v1784571155/)
                    var publicIdSegments = segments.Skip(uploadIndex + 2).Select(s => s.TrimEnd('/'));
                    var publicId = string.Join("/", publicIdSegments);
                    
                    var isRaw = segments.Contains("raw/");
                    var isVideo = segments.Contains("video/");
                    var resourceType = isRaw ? ResourceType.Raw : (isVideo ? ResourceType.Video : ResourceType.Image);
                    
                    if (resourceType != ResourceType.Raw) 
                    {
                        // Image and Video public_id thường không có extension
                        var lastDotIndex = publicId.LastIndexOf('.');
                        if (lastDotIndex > 0)
                        {
                            publicId = publicId.Substring(0, lastDotIndex);
                        }
                    }

                    var deletionParams = new DeletionParams(publicId)
                    {
                        ResourceType = resourceType
                    };
                    await _cloudinary.DestroyAsync(deletionParams);
                }
            }
            catch (Exception)
            {
                // Bỏ qua lỗi xóa nếu URL không hợp lệ hoặc file không tồn tại
            }
        }
    }
}
