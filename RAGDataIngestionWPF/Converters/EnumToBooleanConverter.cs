// 2026/03/10
//  Solution: RAGDataIngestionWPF
//  Project:   RAGDataIngestionWPF
//  File:         EnumToBooleanConverter.cs
//   Author: Kyle L. Crowder



using System.Globalization;
using System.Windows.Data;




namespace RAGDataIngestionWPF.Converters;





public class EnumToBooleanConverter : IValueConverter
{
    public Type EnumType { get; set; }








    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (parameter is string enumString)
        {
            if (Enum.IsDefined(EnumType, value))
            {
                var enumValue = Enum.Parse(EnumType, enumString);

                return enumValue.Equals(value);
            }
        }

        return false;
    }








    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return parameter is string enumString ? Enum.Parse(EnumType, enumString) : null;

    }
}