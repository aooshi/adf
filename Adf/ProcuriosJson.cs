using System;
using System.Collections;
using System.Globalization;
using System.Text;
using System.Collections.Generic;

namespace Adf
{
	/// <summary>
	/// This class encodes and decodes JSON strings.
	/// Spec. details, see http://www.json.org/
	///
	/// JSON uses Arrays and Objects. These correspond here to the datatypes ArrayList and Hashtable.
    /// All numbers are parsed to doubles.
    /// 
    /// download http://techblog.procurios.nl/k/618/news/view/14605/14863/How-do-I-write-my-own-parser-for-JSON.html
    /// 
    /// The software is subject to the MIT license: you are free to use it in any way you like, but it must keep its license.
    /// 
	/// </summary>
    internal class ProcuriosJson
	{
        static readonly Type IDictionaryType = typeof(IDictionary);

		public const int TOKEN_NONE = 0;
		public const int TOKEN_CURLY_OPEN = 1;
		public const int TOKEN_CURLY_CLOSE = 2;
		public const int TOKEN_SQUARED_OPEN = 3;
		public const int TOKEN_SQUARED_CLOSE = 4;
		public const int TOKEN_COLON = 5;
		public const int TOKEN_COMMA = 6;
		public const int TOKEN_STRING = 7;
		public const int TOKEN_NUMBER = 8;
		public const int TOKEN_TRUE = 9;
		public const int TOKEN_FALSE = 10;
		public const int TOKEN_NULL = 11;

		private const int BUILDER_CAPACITY = 2000;

		/// <summary>
		/// Parses the string json into a value
		/// </summary>
		/// <param name="json">A JSON string.</param>
        /// <returns>An Array, a Dictionary(string,object), a double, a string, null, true, or false</returns>
		public static object JsonDecode(string json)
		{
			bool success = true;

			return JsonDecode(json, ref success);
		}
              
		/// <summary>
		/// Parses the string json into a value; and fills 'success' with the successfullness of the parse.
		/// </summary>
		/// <param name="json">A JSON string.</param>
		/// <param name="success">Successful parse?</param>
        // <returns>An ArrayList, a Hashtable, a double, a string, null, true, or false</returns>
        public static object JsonDecode(string json, ref bool success)
		{
			success = true;
			if (json != null) {
				char[] charArray = json.ToCharArray();
				int index = 0;
				object value = ParseValue(charArray, ref index, ref success);
				return value;
			} else {
				return null;
			}
		}

		/// <summary>
		/// Converts a Hashtable / ArrayList object into a JSON string
		/// </summary>
		/// <param name="json">A Hashtable / ArrayList / Object</param>
        /// <param name="hex"></param>
		/// <returns>A JSON encoded string, or null if object 'json' is not serializable</returns>
		public static string JsonEncode(object json,bool hex)
		{
			StringBuilder builder = new StringBuilder(BUILDER_CAPACITY);
			bool success = SerializeValue(json, builder,hex);
			return (success ? builder.ToString() : null);
		}

		protected static Dictionary<string,object> ParseObject(char[] json, ref int index, ref bool success)
		{
			var table = new Dictionary<string,object>();
			int token;

			// {
			NextToken(json, ref index);

			bool done = false;
			while (!done) {
				token = LookAhead(json, index);
				if (token == ProcuriosJson.TOKEN_NONE) {
					success = false;
					return null;
				} else if (token == ProcuriosJson.TOKEN_COMMA) {
					NextToken(json, ref index);
				} else if (token == ProcuriosJson.TOKEN_CURLY_CLOSE) {
					NextToken(json, ref index);
					return table;
				} else {

					// name
					string name = ParseString(json, ref index, ref success);
					if (!success) {
						success = false;
						return null;
					}

					// :
					token = NextToken(json, ref index);
					if (token != ProcuriosJson.TOKEN_COLON) {
						success = false;
						return null;
					}

					// value
					object value = ParseValue(json, ref index, ref success);
					if (!success) {
						success = false;
						return null;
					}

					table[name] = value;
				}
			}

			return table;
		}

        protected static ArrayList ParseArray(char[] json, ref int index, ref bool success)
		{
            var array = new ArrayList(5);

			// [
			NextToken(json, ref index);

			bool done = false;
			while (!done) {
				int token = LookAhead(json, index);
				if (token == ProcuriosJson.TOKEN_NONE) {
					success = false;
					return null;
				} else if (token == ProcuriosJson.TOKEN_COMMA) {
					NextToken(json, ref index);
				} else if (token == ProcuriosJson.TOKEN_SQUARED_CLOSE) {
					NextToken(json, ref index);
					break;
				} else {
					object value = ParseValue(json, ref index, ref success);
					if (!success) {
						return null;
					}

                    if (value != null)
                        array.Add(value);
				}
			}

            if (array.Count == 0)
                return null;

			return array;
		}

        protected static object ParseValue(char[] json, ref int index, ref bool success)
        {
            switch (LookAhead(json, index))
            {
                case ProcuriosJson.TOKEN_STRING:
                    return ParseString(json, ref index, ref success);
                case ProcuriosJson.TOKEN_NUMBER:
                    return ParseNumber(json, ref index, ref success);
                case ProcuriosJson.TOKEN_CURLY_OPEN:
                    return ParseObject(json, ref index, ref success);
                case ProcuriosJson.TOKEN_SQUARED_OPEN:
                    return ParseArray(json, ref index, ref success);
                case ProcuriosJson.TOKEN_TRUE:
                    NextToken(json, ref index);
                    return true;
                case ProcuriosJson.TOKEN_FALSE:
                    NextToken(json, ref index);
                    return false;
                case ProcuriosJson.TOKEN_NULL:
                    NextToken(json, ref index);
                    return null;
                case ProcuriosJson.TOKEN_NONE:
                    break;
            }

            success = false;
            return null;
        }

		protected static string ParseString(char[] json, ref int index, ref bool success)
		{
			StringBuilder s = new StringBuilder(BUILDER_CAPACITY);
			char c;

			EatWhitespace(json, ref index);

			// "
			c = json[index++];

			bool complete = false;
			while (!complete) {

				if (index == json.Length) {
					break;
				}

				c = json[index++];
				if (c == '"') {
					complete = true;
					break;
				} else if (c == '\\') {

					if (index == json.Length) {
						break;
					}
					c = json[index++];
					if (c == '"') {
						s.Append('"');
					} else if (c == '\\') {
						s.Append('\\');
					} else if (c == '/') {
						s.Append('/');
					} else if (c == 'b') {
						s.Append('\b');
					} else if (c == 'f') {
						s.Append('\f');
					} else if (c == 'n') {
						s.Append('\n');
					} else if (c == 'r') {
						s.Append('\r');
					} else if (c == 't') {
						s.Append('\t');
					} else if (c == 'u') {
						int remainingLength = json.Length - index;
						if (remainingLength >= 4) {
							// parse the 32 bit hex into an integer codepoint
							uint codePoint;
							if (!(success = UInt32.TryParse(new string(json, index, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out codePoint))) {
								return "";
							}
							// convert the integer codepoint to a unicode char and add to string
							s.Append(Char.ConvertFromUtf32((int)codePoint));
							// skip 4 chars
							index += 4;
						} else {
							break;
						}
					}

				} else {
					s.Append(c);
				}

			}

			if (!complete) {
				success = false;
				return null;
			}

			return s.ToString();
		}

        //protected static double ParseNumber(char[] json, ref int index, ref bool success)
        //{
        //    EatWhitespace(json, ref index);

        //    int lastIndex = GetLastIndexOfNumber(json, index);
        //    int charLength = (lastIndex - index) + 1;

        //    double number;
        //    success = Double.TryParse(new string(json, index, charLength), NumberStyles.Any, CultureInfo.InvariantCulture, out number);

        //    index = lastIndex + 1;
        //    return number;
        //}

        protected static object ParseNumber(char[] json, ref int index, ref bool success)
        {
            EatWhitespace(json, ref index);

            int lastIndex = GetLastIndexOfNumber(json, index);
            int charLength = (lastIndex - index) + 1;

            var number = new string(json, index, charLength);
            index = lastIndex + 1;

            //
            if (number.IndexOf('.') != -1 ||
     number.IndexOf('e') != -1 ||
     number.IndexOf('E') != -1)
            {
                double n_double;
                if (Double.TryParse(number, out n_double))
                {
                    //token = JsonToken.Double;
                    return n_double;
                }
            }

            int n_int32;
            if (Int32.TryParse(number, out n_int32))
            {
                //token = JsonToken.Int;
                return n_int32;
            }

            long n_int64;
            if (Int64.TryParse(number, out n_int64))
            {
                //token = JsonToken.Long;
                return n_int64;
            }

            // Shouldn't happen, but just in case, return something
            //token = JsonToken.Int;
            success = false;
            return 0;
        }

		protected static int GetLastIndexOfNumber(char[] json, int index)
		{
			int lastIndex;

			for (lastIndex = index; lastIndex < json.Length; lastIndex++) {
				if ("0123456789+-.eE".IndexOf(json[lastIndex]) == -1) {
					break;
				}
			}
			return lastIndex - 1;
		}

		protected static void EatWhitespace(char[] json, ref int index)
		{
			for (; index < json.Length; index++) {
				if (" \t\n\r".IndexOf(json[index]) == -1) {
					break;
				}
			}
		}

		protected static int LookAhead(char[] json, int index)
		{
			int saveIndex = index;
			return NextToken(json, ref saveIndex);
		}

		protected static int NextToken(char[] json, ref int index)
		{
			EatWhitespace(json, ref index);

			if (index == json.Length) {
				return ProcuriosJson.TOKEN_NONE;
			}

			char c = json[index];
			index++;
			switch (c) {
				case '{':
					return ProcuriosJson.TOKEN_CURLY_OPEN;
				case '}':
					return ProcuriosJson.TOKEN_CURLY_CLOSE;
				case '[':
					return ProcuriosJson.TOKEN_SQUARED_OPEN;
				case ']':
					return ProcuriosJson.TOKEN_SQUARED_CLOSE;
				case ',':
					return ProcuriosJson.TOKEN_COMMA;
				case '"':
					return ProcuriosJson.TOKEN_STRING;
				case '0': case '1': case '2': case '3': case '4':
				case '5': case '6': case '7': case '8': case '9':
				case '-':
					return ProcuriosJson.TOKEN_NUMBER;
				case ':':
					return ProcuriosJson.TOKEN_COLON;
			}
			index--;

			int remainingLength = json.Length - index;

			// false
			if (remainingLength >= 5) {
				if (json[index] == 'f' &&
					json[index + 1] == 'a' &&
					json[index + 2] == 'l' &&
					json[index + 3] == 's' &&
					json[index + 4] == 'e') {
					index += 5;
					return ProcuriosJson.TOKEN_FALSE;
				}
			}

			// true
			if (remainingLength >= 4) {
				if (json[index] == 't' &&
					json[index + 1] == 'r' &&
					json[index + 2] == 'u' &&
					json[index + 3] == 'e') {
					index += 4;
					return ProcuriosJson.TOKEN_TRUE;
				}
			}

			// null
			if (remainingLength >= 4) {
				if (json[index] == 'n' &&
					json[index + 1] == 'u' &&
					json[index + 2] == 'l' &&
					json[index + 3] == 'l') {
					index += 4;
					return ProcuriosJson.TOKEN_NULL;
				}
			}

			return ProcuriosJson.TOKEN_NONE;
		}

        protected static bool SerializeValue(object value, StringBuilder builder, bool hex)
        {
            bool success = true;
            if (value == null)
            {
                builder.Append("null");
            }
            else if (value is string)
            {
                success = SerializeString((string)value, builder, hex);
            }
            else if (value is DateTime)
            {
                success = SerializeString(((DateTime)value).ToUniversalTime().ToString("r"), builder, hex);
            }
            else if (value is IDictionary)
            {
                success = SerializeObject((IDictionary)value, builder, hex);
            }
            else if (value is ICollection)
            {
                success = SerializeArray((ICollection)value, builder, hex);
            }
            else if ((value is Boolean) && ((Boolean)value == true))
            {
                builder.Append("true");
            }
            else if ((value is Boolean) && ((Boolean)value == false))
            {
                builder.Append("false");
            }
            else if (TypeHelper.IsNumber(value))
            {
                // thanks to ritchie for pointing out ValueType to me
                success = SerializeNumber(value, builder);
            }
            else if (value is ValueType)
            {
                success = SerializeString(Convert.ToString(value), builder, hex);
            }
            else
            {
                var ht = Adf.ObjectConverter.ToDictionary(value);
                return SerializeValue(ht, builder, hex);
            }
            return success;
        }

        protected static bool SerializeObject(IDictionary anObject, StringBuilder builder,bool hex)
		{
			builder.Append("{");

			IDictionaryEnumerator e = anObject.GetEnumerator();
			bool first = true;
			while (e.MoveNext()) {
				string key = e.Key.ToString();
				object value = e.Value;

				if (!first) {
					builder.Append(",");
				}

				SerializeString(key, builder,hex);
				builder.Append(":");
				if (!SerializeValue(value, builder,hex)) {
					return false;
				}

				first = false;
			}

			builder.Append("}");
			return true;
		}

		protected static bool SerializeArray(ICollection anArray, StringBuilder builder, bool hex)
		{
			builder.Append("[");

			bool first = true;
            var enumerator = anArray.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (!first)
                {
                    builder.Append(",");
                }

                if (!SerializeValue(enumerator.Current, builder, hex))
                {
                    return false;
                }

                first = false;

            }

			builder.Append("]");
			return true;
		}

		protected static bool SerializeString(string aString, StringBuilder builder,bool hex)
		{
			builder.Append("\"");

			char[] charArray = aString.ToCharArray();
			for (int i = 0; i < charArray.Length; i++) {
				char c = charArray[i];
				if (c == '"') {
					builder.Append("\\\"");
				} else if (c == '\\') {
					builder.Append("\\\\");
				} else if (c == '\b') {
					builder.Append("\\b");
				} else if (c == '\f') {
					builder.Append("\\f");
				} else if (c == '\n') {
					builder.Append("\\n");
				} else if (c == '\r') {
					builder.Append("\\r");
				} else if (c == '\t') {
					builder.Append("\\t");
                } else if (!hex){
                    builder.Append(c);
				} else {
					int codepoint = Convert.ToInt32(c);
					if ((codepoint >= 32) && (codepoint <= 126)) {
						builder.Append(c);
                    }
                    else if (codepoint <= 0xDFFF && codepoint >= 0xD800){
						builder.Append(c);
					} else {
						builder.Append("\\u" + Convert.ToString(codepoint, 16).PadLeft(4, '0'));
					}
				}
			}

			builder.Append("\"");
			return true;
		}

		protected static bool SerializeNumber(object number, StringBuilder builder)
		{
			builder.Append(Convert.ToString(number, CultureInfo.InvariantCulture));
			return true;
		}
	}
}

