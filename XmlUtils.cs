using System;
using System.Globalization;
using System.Numerics;
using System.Text;
using System.Xml.Linq;

namespace Extreme.Core
{
    public static class XmlUtils
    {
        /// <summary>
        /// Can read Infinity, return Decimal.MaxValue
        /// </summary>
        /// <param name="xelem"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static decimal AttributeAsDecimal(this XElement xelem, string name)
        {
            var value = xelem.Attribute(name).Value;

            if (value == @"Infinity")
                return Decimal.MaxValue;

            return decimal.Parse(value, NumberStyles.Currency, CultureInfo.InvariantCulture);
        }

        public static decimal? AttributeAsDecimalOrNull(this XElement xelem, string name)
        {
            var attr = xelem.Attribute(name);
            if (attr == null)
                return null;

            var value = attr.Value;

            if (value == @"Infinity")
                return Decimal.MaxValue;

            return decimal.Parse(value, NumberStyles.Currency, CultureInfo.InvariantCulture);
        }


        public static float AttributeAsFloat(this XElement xelem, string name)
        {
            return float.Parse(xelem.Attribute(name).Value, NumberStyles.Float, CultureInfo.InvariantCulture);
        }

        public static float? AttributeAsFloatOrNull(this XElement xelem, string name)
        {
            var attr = xelem.Attribute(name);

            if (attr != null)
                return float.Parse(attr.Value, NumberStyles.Float, CultureInfo.InvariantCulture);

            return null;
        }
        public static double? AttributeAsDoubleOrNull(this XElement xelem, string name)
        {
            var attr = xelem.Attribute(name);

            if (attr != null)
                return double.Parse(attr.Value, NumberStyles.Float, CultureInfo.InvariantCulture);

            return null;
        }

        public static bool? AttributeAsBoolOrNull(this XElement xelem, string name)
        {
            var attr = xelem.Attribute(name);

            if (attr != null)
                return bool.Parse(attr.Value);

            return null;
        }


        public static double AttributeAsDouble(this XElement xelem, string name)
        {
            return double.Parse(xelem.Attribute(name).Value, NumberStyles.Float, CultureInfo.InvariantCulture);
        }

        public static double ValueAsDouble(this XElement xelem)
        {
            return double.Parse(xelem.Value, NumberStyles.Float, CultureInfo.InvariantCulture);
        }
        public static long ValueAsLong(this XElement xelem)
        {
            return long.Parse(xelem.Value, NumberStyles.Integer, CultureInfo.InvariantCulture);
        }
        public static int ValueAsInt32(this XElement xelem)
        {
            return Int32.Parse(xelem.Value, NumberStyles.Integer, CultureInfo.InvariantCulture);
        }


        public static int AttributeAsInt(this XElement xelem, string name)
        {
            return int.Parse(xelem.Attribute(name).Value, NumberStyles.Integer, CultureInfo.InvariantCulture);
        }

        public static long AttributeAsLong(this XElement xelem, string name)
        {
            return long.Parse(xelem.Attribute(name).Value, NumberStyles.Integer, CultureInfo.InvariantCulture);
        }

        public static long? AttributeAsLongOrNull(this XElement xelem, string name)
        {
            var attr = xelem.Attribute(name);

            if (attr == null)
                return null;

            return long.Parse(attr.Value, NumberStyles.Integer, CultureInfo.InvariantCulture);
        }

        public static int AttributeAsIntOrZero(this XElement xelem, string name)
        {
            var attr = xelem.Attribute(name);

            if (attr == null)
                return 0;

            return int.Parse(attr.Value, NumberStyles.Integer, CultureInfo.InvariantCulture);
        }

        public static int? AttributeAsIntOrNull(this XElement xelem, string name)
        {
            var attr = xelem.Attribute(name);

            if (attr != null)
                return int.Parse(attr.Value, NumberStyles.Integer, CultureInfo.InvariantCulture);

            return null;
        }

        public static string AttributeAsStringOrEmpty(this XElement xelem, string name)
        {
            var attr = xelem.Attribute(name);

            return attr == null ? "" : attr.Value;
        }

        public static XElement ToXElement(this Complex value, string name)
        {
            return new XElement(name, 
                new XAttribute("re", value.Real),
                new XAttribute("im", value.Imaginary));
        }

        public static Complex? ElementAsComplexOrNull(this XElement xelem, string name)
        {
            var elem = xelem.Element(name);

            if (elem == null)
                return null;

            var re = elem.AttributeAsDoubleOrNull("re");
            var im = elem.AttributeAsDoubleOrNull("im");

            if (re == null || im == null)
                return null;

            return new Complex(re.Value, im.Value);
        }

        public static int? ElementAsIntOrNull(this XElement xelem, string name)
        {
            var attr = xelem.Element(name);

            if (attr != null)
                return int.Parse(attr.Value);

            return null;
        }

        public static Complex ElementAsComplex(this XElement xelem, string name)
        {
            var elem = xelem.Element(name);

            var re = elem.AttributeAsDouble("re");
            var im = elem.AttributeAsDouble("im");

            return new Complex(re, im);
        }

        public static double? ElementAsDoubleOrNull(this XElement xelem, string name)
        {
            var elem = xelem.Element(name);

            if (elem == null)
                return null;

            return double.Parse(elem.Value, NumberStyles.Float, CultureInfo.InvariantCulture);
        }


        public static int AsInt(this XAttribute xatrib)
        {
            return int.Parse(xatrib.Value, NumberStyles.Integer, CultureInfo.InvariantCulture);
        }

        public static string ToString(float[] values)
        {
            var sb = new StringBuilder();

            foreach (var value in values)
                sb.Append($"{value} ");

            return sb.ToString();
        }
    }
}
