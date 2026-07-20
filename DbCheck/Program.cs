using System;
using System.IO;
using System.Text;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace DbCheck
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var acc = new Account("yikqzell", "828932477884394", "dXPJQUxc35AbbArgmThxiE3QjQ4");
                var cloudinary = new Cloudinary(acc);
                
                var testContent = Encoding.UTF8.GetBytes("test file content");
                using var stream = new MemoryStream(testContent);
                var uploadParams = new RawUploadParams()
                {
                    File = new FileDescription("test.txt", stream),
                    Folder = "test"
                };
                
                var result = cloudinary.Upload(uploadParams);
                if (result.Error != null) {
                    Console.WriteLine("Error: " + result.Error.Message);
                } else {
                    Console.WriteLine("Success: " + result.SecureUrl);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
            }
        }
    }
}
