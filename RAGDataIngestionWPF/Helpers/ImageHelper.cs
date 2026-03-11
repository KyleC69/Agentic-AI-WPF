// Build Date: 2026/03/11
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         ImageHelper.cs
// Author: Kyle L. Crowder
// Build Num: 105609



using System.IO;
using System.Windows.Media.Imaging;




namespace RAGDataIngestionWPF.Helpers;





public static class ImageHelper
{

    public static BitmapImage ImageFromAssetsFile(string fileName)
    {
        Uri imageUri = new($"pack://application:,,,/Assets/{fileName}");
        BitmapImage image = new(imageUri);
        return image;
    }








    public static BitmapImage ImageFromString(string data)
    {
        BitmapImage image = new();
        var binaryData = Convert.FromBase64String(data);
        image.BeginInit();
        image.StreamSource = new MemoryStream(binaryData);
        image.EndInit();
        return image;
    }
}