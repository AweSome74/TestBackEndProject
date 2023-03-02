using System.Globalization;

namespace MyBackEndApp.Utils
{
    public class ValueConverter
    {
        public static float? TryGetFloatValue(string value)
        {
            try
            {
                PrepareStringValue(ref value, out NumberFormatInfo numberFormat);
                return float.Parse(value, numberFormat);
            }
            catch
            {
                return null;
            }
        }
        public static int? TryGetIntValue(string value)
        {
            try
            {
                return int.Parse(value);
            }
            catch
            {
                return null;
            }
        }

        private static void PrepareStringValue(ref string stringValue, out NumberFormatInfo numberFormat)
        {
            numberFormat = CultureInfo.CurrentCulture.NumberFormat;
            char separator = numberFormat.NumberDecimalSeparator[0];
            if (!stringValue.Contains(separator))
                stringValue = stringValue.Replace(separator == ',' ? '.' : ',', separator);
        }
    }
}
