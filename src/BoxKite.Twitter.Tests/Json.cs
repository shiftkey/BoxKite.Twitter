using System;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;

namespace BoxKite.Twitter.Tests
{
    public static class Json
    {
        public static async Task<string> FromFile(string path)
        {
            var folder = Package.Current.InstalledLocation;
            var filePath = Path.Combine(folder.Path, path);
            var file = await StorageFile.GetFileFromPathAsync(filePath);
            var openFile = await file.OpenReadAsync();
            var reader = new StreamReader(openFile.AsStreamForRead());
            return await reader.ReadToEndAsync();
        }
    }
}
