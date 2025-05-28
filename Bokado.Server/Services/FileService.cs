namespace Bokado.Server.Services
{
    public class FileService
    {
        public async Task<string> SaveFileAsync(
    IFormFile file,
    string destinationFolder,
    string[] allowedExtensions,
    string fileNamePrefix = "")
        {
            Directory.CreateDirectory(destinationFolder);

            var fileExtension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(fileExtension))
            {
                throw new ArgumentException("Invalid file format. Allowed formats: " + string.Join(", ", allowedExtensions));
            }

            var uniqueFileName = $"{DateTime.UtcNow.Ticks}_{fileNamePrefix}{fileExtension}";
            var filePath = Path.Combine(destinationFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return uniqueFileName;
        }
    }
}
