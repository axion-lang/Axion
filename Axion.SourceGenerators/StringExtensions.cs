using System;
using System.Text;

namespace Axion.SourceGenerators {
    public static class StringExtensions {
        internal static string Indent(this string value, int size) {
            var strArray = value.Split(Environment.NewLine.ToCharArray());
            var sb = new StringBuilder();
            for (var i = 0; i < strArray.Length - 1; i++) {
                if (!string.IsNullOrWhiteSpace(strArray[i]))
                    sb.Append(new string(' ', size)).Append(strArray[i]).AppendLine();
            }
            if (!string.IsNullOrWhiteSpace(strArray[strArray.Length - 1]))
                sb.Append(new string(' ', size)).Append(strArray[strArray.Length - 1]);
            return sb.ToString();
        }

        internal static string ToUsing(this string value) {
            return "using " + value + ";";
        }
    }
}
