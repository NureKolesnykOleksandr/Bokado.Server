using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace Bokado.Server.Services
{
    public class FileService
    {
        private readonly Cloudinary _cloudinary;

        public FileService()
        {
            var cloudName = Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME") ?? "ddg24rxjf";
            var apiKey    = Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY") ?? "952218146615112";
            var apiSecret = Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET") ?? "fBSDjOtQbkZ0GNcE3DSve1BHO98";

            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);
        }

        public async Task<string> SaveFileAsync(
            IFormFile file,
            string destinationFolder,
            string[] allowedExtensions,
            string fileNamePrefix = "")
        {
            var fileExtension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(fileExtension))
            {
                throw new ArgumentException("Invalid file format. Allowed formats: " + string.Join(", ", allowedExtensions));
            }

            using var stream = file.OpenReadStream();

            var uploadParams = new RawUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                PublicId = $"{fileNamePrefix}_{DateTime.UtcNow.Ticks}",
                Folder = destinationFolder.Replace("\\", "/").TrimStart('/'),
            };

            // Якщо це зображення — використовуємо ImageUploadParams
            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            if (imageExtensions.Contains(fileExtension))
            {
                var imageParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    PublicId = $"{fileNamePrefix}_{DateTime.UtcNow.Ticks}",
                    Folder = destinationFolder.Replace("\\", "/").TrimStart('/'),
                };
                var imageResult = await _cloudinary.UploadAsync(imageParams);
                return imageResult.SecureUrl.ToString();
            }

            var result = await _cloudinary.UploadAsync(uploadParams);
            return result.SecureUrl.ToString();
        }
    }
}
