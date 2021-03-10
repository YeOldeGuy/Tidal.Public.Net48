using System.Text;
using System.Threading.Tasks;
using Utf8Json;
using Utf8Json.Resolvers;

namespace Tidal.Client.Helpers
{
    public static class Json
    {
        public static async Task<T> ToObjectAsync<T>(string value)
        {
            return await Task.Run(() => ToObject<T>(value));
        }

        public static async Task<string> ToJSONAsync(object value)
        {
            return await Task.Run(() => ToJSON(value));
        }


        /// <summary>
        /// Convert the JSON encoded string into a <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to convert to.</typeparam>
        /// <param name="value">A JSON-encoded string.</param>
        /// <returns>A new instance of <typeparamref name="T"/>.</returns>
        public static T ToObject<T>(string value)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(value))
                    return default;

                var resp = JsonSerializer.Deserialize<T>(value, StandardResolver.AllowPrivate);
                return resp;
            }
            catch (JsonParsingException)
            {
                return default;
            }
        }

        /// <summary>
        /// Convert the JSON encoded byte array into a <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to convert to.</typeparam>
        /// <param name="value">A JSON-encoded string.</param>
        /// <returns>A new instance of <typeparamref name="T"/>.</returns>
        public static T ToObject<T>(byte[] bytes)
        {
            try
            {
                if (bytes == null)
                    return default;

                T temp = JsonSerializer.Deserialize<T>(bytes, StandardResolver.AllowPrivate);
                return temp;
            }
            catch (JsonParsingException)
            {
                return default;
            }
        }

        /// <summary>
        /// Convert the <paramref name="value"/> to a JSON-encoded string.
        /// </summary>
        /// <param name="value">Any object.</param>
        /// <returns>A JSON-encoded representation of the <paramref name="value"/>.</returns>
        public static string ToJSON(object value)
        {
            return Encoding.UTF8.GetString(JsonSerializer.Serialize(value, StandardResolver.ExcludeNull));
        }

        /// <summary>
        /// Convert the <paramref name="value"/> to a JSON-encoded byte array.
        /// </summary>
        /// <param name="value">Any object.</param>
        /// <returns>A JSON-encoded representation of the <paramref name="value"/>.</returns>
        public static byte[] ToJSONBytes(object value)
        {
            return JsonSerializer.Serialize(value, StandardResolver.ExcludeNull);
        }

        /// <summary>
        /// Convert the <paramref name="value"/> to a JSON-encoded string, but
        /// <see langword="null"/> values will be output instead of ignored. 
        /// </summary>
        /// <param name="value">Any object.</param>
        /// <returns>A JSON-encoded representation of the <paramref name="value"/>.</returns>
        public static string ToJsonKeepNulls(object value)
        {
            return Encoding.UTF8.GetString(JsonSerializer.Serialize(value));
        }

        /// <summary>
        /// Convert the <paramref name="value"/> to a JSON-encoded byte array,
        /// but <see langword="null"/> values will be output instead of ignored. 
        /// </summary>
        /// <param name="value">Any object.</param>
        /// <returns>A JSON-encoded representation of the <paramref name="value"/>.</returns>
        public static byte[] ToJSONBytesKeepNulls(object value)
        {
            return JsonSerializer.Serialize(value);
        }
    }
}
