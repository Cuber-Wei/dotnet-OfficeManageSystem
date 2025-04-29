using System.Globalization;
using System.Diagnostics;

namespace OfficeMgtAdmin.Converters
{
    public class MultiplyConverter : IMultiValueConverter
    {
        public object? Convert(object?[] values, Type targetType, object? parameter, CultureInfo culture)
        {
            Debug.WriteLine($"Values: {string.Join(", ", values.Select(v => $"{v?.GetType()}, {v}"))}");
            
            if (values.Length < 2 || values[0] == null || values[1] == null)
            {
                return 0m;
            }

            if (values[0] is int num && values[1] is decimal price)
            {
                var result = num * price;
                Debug.WriteLine($"Result: {result}");
                return result;
            }

            return 0m;
        }

        public object[]? ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 