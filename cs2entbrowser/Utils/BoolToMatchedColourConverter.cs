using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cs2entbrowser.Utils;

class BoolToMatchedColourConverter : IValueConverter
{
    private static string MATCHED_COLOUR = "#337";
    private static string NONMATCHED_COLOUR = "transparent";

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            if(parameter is string text)
            {
                Debug.WriteLine("buh: " + text);
                if (text.Trim().Length == 0)
                    return NONMATCHED_COLOUR;
            }
            
            return boolValue ? MATCHED_COLOUR : NONMATCHED_COLOUR;
        }
        return NONMATCHED_COLOUR;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string stringValue)
        {
            return stringValue.Equals(MATCHED_COLOUR, StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }
}
