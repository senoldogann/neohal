using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace NeoHal.Desktop.Converters;

/// <summary>
/// Decimal değerleri Türkçe locale ile uyumlu şekilde string'e ve geri çevirir.
/// Two-way binding için güvenli dönüşüm sağlar.
/// </summary>
public class DecimalToStringConverter : IValueConverter
{
    public static DecimalToStringConverter Instance { get; } = new();
    
    // Locale-independent parse için kullanılacak
    private static readonly CultureInfo TurkishCulture = new("tr-TR");
    private static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is decimal decimalValue)
        {
            // Görüntüleme için Türkçe format kullan (1.234,56)
            var format = parameter?.ToString() ?? "N2";
            return decimalValue.ToString(format, TurkishCulture);
        }
        return "0,00";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string strValue && !string.IsNullOrWhiteSpace(strValue))
        {
            // Boşlukları temizle (binlik ayraç olarak kullanılabilir)
            strValue = strValue.Replace(" ", "").Trim();
            
            // Önce Türkçe format dene (virgül ondalık, nokta binlik)
            if (decimal.TryParse(strValue, NumberStyles.Number, TurkishCulture, out var result))
            {
                return result;
            }
            
            // Sonra invariant format dene (nokta ondalık)
            if (decimal.TryParse(strValue, NumberStyles.Number, InvariantCulture, out result))
            {
                return result;
            }
            
            // Son çare: sadece rakamları al
            strValue = strValue.Replace(",", ".").Replace(" ", "");
            // Birden fazla nokta varsa son noktayı ondalık say
            var parts = strValue.Split('.');
            if (parts.Length > 2)
            {
                strValue = string.Join("", parts[..^1]) + "." + parts[^1];
            }
            
            if (decimal.TryParse(strValue, NumberStyles.Number, InvariantCulture, out result))
            {
                return result;
            }
        }
        return 0m;
    }
}

/// <summary>
/// Integer değerleri string'e ve geri çevirir.
/// </summary>
public class IntToStringConverter : IValueConverter
{
    public static IntToStringConverter Instance { get; } = new();
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int intValue)
        {
            return intValue.ToString();
        }
        return "0";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string strValue && int.TryParse(strValue.Replace(" ", ""), out var result))
        {
            return result;
        }
        return 0;
    }
}
