using System.Globalization;

namespace OfficeMgtAdmin.Converters
{
    public class ItemTypeConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
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
                    _ => "未知"
                };
            }
            return "未知";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string typeStr)
            {
                return typeStr switch
                {
                    "办公用品" => 0,
                    "电子设备" => 1,
                    "刀具" => 2,
                    "单据" => 3,
                    "礼品" => 4,
                    "其他" => 5,
                    _ => 5
                };
            }
            return 0;
        }
    }
} 