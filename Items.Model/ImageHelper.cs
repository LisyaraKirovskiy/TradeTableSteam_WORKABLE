using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace testgraph;

public static class ImageHelper
{
    public static async Task<Bitmap?> LoadFromWeb(Uri url)
    {
        using var httpClient = new HttpClient();
        try
        {
            var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadAsByteArrayAsync();
            return new Bitmap(new MemoryStream(data));
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"An error occurred while downloading image '{url}' : {ex.Message}");
            return null;
        }
    }
    public static Bitmap LoadFromResource(Uri resourceUri)
    {
        string url = resourceUri.ToString();
        var uri = new Uri(url);
        using (var stream = AssetLoader.Open(uri))
        {
            return new Bitmap(stream); // Загрузка без кэширования
        }
    }
}