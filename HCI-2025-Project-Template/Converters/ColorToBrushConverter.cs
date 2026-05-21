using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


    using System;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Media;

    namespace HCI_2025_Project_Template.Converters
    {
        public class ColorToBrushConverter : IValueConverter
        {
          
            
                public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
                {
                    if (value == null)
                        return Brushes.Transparent;

                    return (SolidColorBrush)(new BrushConverter().ConvertFrom(value));
                }

                public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
                {
                    return null;
                }
            }
        }
    

