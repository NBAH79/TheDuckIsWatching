using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheDuckIsWatching.Utils;

public class IndexToGradientConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int index && index >= 0 && index < CONST.Colors.Count)
        {
            return MakeBrush(CONST.Colors[index]);
        }
        return Microsoft.Maui.Graphics.Colors.Transparent;
    }

    //без проверок, оно используется на странице карточки
    public LinearGradientBrush MakeBrush(Color color) =>
        new LinearGradientBrush
        {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(1, 0), // Слева направо
            GradientStops = new GradientStopCollection
                {
                    new GradientStop(color, 0),
                    new GradientStop( Microsoft.Maui.Graphics.Colors.Transparent, 1)
                }
        };

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}
