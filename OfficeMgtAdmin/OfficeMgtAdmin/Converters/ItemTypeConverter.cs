using System.Globalization;

namespace OfficeMgtAdmin.Converters
{
    public class ItemTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int type)
            {
                return type switch
                {
                    0 => "纸张",
                    1 => "文具",
                    2 => "刀具",
                    3 => "单据",
                    4 => "礼品",
                    5 => "其它",
                    _ => "未知类型"
                };
            }
            return "未知类型";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 