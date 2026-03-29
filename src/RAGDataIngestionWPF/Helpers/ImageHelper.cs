// Build Date: 2026/03/29
// Solution: File
// Project:   RAGDataIngestionWPF
// File:         ImageHelper.cs
// Author: Kyle L. Crowder
// Build Num: 051951



using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows.Media.Imaging;




namespace RAGDataIngestionWPF.Helpers;





public static class ImageHelper
{

    [return: NotNull]
    public static BitmapImage ImageFromAssetsFile(string fileName)
    {
        Uri imageUri = new($"pack://application:,,,/Assets/{fileName}");
        BitmapImage image = new(imageUri);
        return image;
    }








    [return: NotNull]
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