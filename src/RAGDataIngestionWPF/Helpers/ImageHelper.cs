// Build Date: 2026/04/06
// Solution: AgenticAIWPF
// Project:   AgenticAIWPF
// File:         ImageHelper.cs
// Author: Kyle L. Crowder
// Build Num: 212932



using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows.Media.Imaging;




namespace AgenticAIWPF.Helpers;





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