// Build Date: 2026/03/15
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         ImageHelper.cs
// Author: Kyle L. Crowder
// Build Num: 091010



using System.IO;
using System.Windows.Media.Imaging;

using JetBrains.Annotations;




namespace RAGDataIngestionWPF.Helpers;





public static class ImageHelper
{

    [NotNull]
    public static BitmapImage ImageFromAssetsFile(string fileName)
    {
        Uri imageUri = new($"pack://application:,,,/Assets/{fileName}");
        BitmapImage image = new(imageUri);
        return image;
    }








    [NotNull]
    public static BitmapImage ImageFromString([NotNull] string data)
    {
        BitmapImage image = new();
        var binaryData = Convert.FromBase64String(data);
        image.BeginInit();
        image.StreamSource = new MemoryStream(binaryData);
        image.EndInit();
        return image;
    }
}