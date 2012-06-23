using System;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel;

namespace BoxKite.Twitter.Tests
{
    public class BaseContext
    {
        public static async Task<string> GetTestData(string filePath)
        {
            var folder = Package.Current.InstalledLocation;
            var file = await folder.GetFileAsync(filePath);
            var openFile = await file.OpenReadAsync();
            var reader = new StreamReader(openFile.AsStreamForRead());
            return  await reader.ReadToEndAsync();
        }
    }
}