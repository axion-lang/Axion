using System.Globalization;
using System.Text;

namespace Axion
{
	internal static class Utilities
	{
		/// <summary>
		///     JavaScript string encoder (modified). Copied from HttpUtilities class.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string Escape(this string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				return string.Empty;
			}

			StringBuilder b          = null;
			var           startIndex = 0;
			var           count      = 0;
			for (var i = 0; i < value.Length; i++)
			{
				var c = value[i];

				// Append the unhandled characters (that do not require special treament)
				// to the string builder when special characters are detected.
				if (CharRequiresJavaScriptEncoding(c))
				{
					if (b == null)
					{
						b = new StringBuilder(value.Length + 5);
					}

					if (count > 0)
					{
						b.Append(value, startIndex, count);
					}

					startIndex = i + 1;
					count      = 0;
				}

				// ReSharper disable PossibleNullReferenceException
				switch (c)
				{
					case '\r':
						b.Append("\\r");
						break;
					case '\t':
						b.Append("\\t");
						break;
					case '\"':
						b.Append("\\\"");
						break;
					// MOD: ' escaped too
					case '\'':
						b.Append("\\'");
						break;
					case '\\':
						b.Append("\\\\");
						break;
					case '\n':
						b.Append("\\n");
						break;
					case '\b':
						b.Append("\\b");
						break;
					case '\f':
						b.Append("\\f");
						break;
					default:
						if (CharRequiresJavaScriptEncoding(c))
						{
							AppendCharAsUnicodeJavaScript(b, c);
						}
						else
						{
							count++;
						}

						break;
				}
				// ReSharper restore PossibleNullReferenceException
			}

			if (b == null)
			{
				return value;
			}

			if (count > 0)
			{
				b.Append(value, startIndex, count);
			}

			return b.ToString();
		}

		private static bool CharRequiresJavaScriptEncoding(char c)
		{
			return c < 0x20     // control chars always have to be encoded
			       || c == '\"' // chars which must be encoded per JSON spec
			       || c == '\\'
			       || c == '\'' // MOD: http signs escape
			       ||
			       c == '\u0085' // newline chars (see Unicode 6.2, Table 5-1 [http://www.unicode.org/versions/Unicode6.2.0/ch05.pdf]) have to be encoded (DevDiv #663531)
			       || c == '\u2028'
			       || c == '\u2029';
		}

		private static void AppendCharAsUnicodeJavaScript(StringBuilder builder, char c)
		{
			builder.Append("\\u");
			builder.Append(((int) c).ToString("x4", CultureInfo.InvariantCulture));
		}
	}
}