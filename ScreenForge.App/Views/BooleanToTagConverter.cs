using System;
using Avalonia.Data.Converters;

namespace ScreenForge.App.Views;

public class BooleanToTagConverter : IValueConverter
{
    public object? Convert(object? value, Type t, object? p, System.Globalization.CultureInfo c)
        => value is true ? "recording" : null;
    public object? ConvertBack(object? v, Type t, object? p, System.Globalization.CultureInfo c)
        => throw new NotSupportedException();
}
