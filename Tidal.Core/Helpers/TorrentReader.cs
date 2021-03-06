using System;
using System.IO;
using System.Text;
using Tidal.Core.BEncoder;
using Tidal.Core.Models;
using Utf8Json;

namespace Tidal.Core.Helpers
{
    /// <summary>
    /// Translate a <c>.torrent</c> file to a <see cref="TorrentMetadata"/>
    /// instance.
    /// </summary>
    /// <remarks>
    /// Based on the original:
    /// https://gist.github.com/ttrider/bde3ebf5e7af6cd2b5ee, but it has, over
    /// time, drifted a long ways from that code, to the point where it doesn't
    /// even use the Json.NET library anymore. The original recursive-descent
    /// parser is still there, doing its job, though.
    /// <para/>
    /// By rights, this is a general-purpose BEncoding parser and I should
    /// probably rename it to BEncodeParser, but then, who else besides
    /// BitTorrent would be crazy enough to use BEncoding? It remains
    /// TorrentReader.
    /// </remarks>
    public class TorrentReader : IDisposable
    {
        // In the original, this was a TextReader instead of a Stream, which
        // caused some real problems as the TextReader was interpreting the
        // bytes, which we definitely don't want here. Everything would work
        // fine... until it didn't.
        private readonly Stream inputStream;

        /// <summary>
        ///   Parse a .torrent file.
        /// </summary>
        /// <param name="buffer">
        ///   A <see cref="Stream"/> with the torrent file contents.
        /// </param>
        /// <returns>
        ///   A <see cref="TorrentMetadata"/> instance.
        /// </returns>
        /// <exception cref="InvalidDataException">
        ///   Thrown if the data cannot be parsed successfully.
        /// </exception>
        public static TorrentMetadata Parse(Stream buffer)
        {
            using (var reader = new TorrentReader(buffer))
            {
                var builder = new StringBuilder();

                var encoded = reader.Parse();
                encoded.BuildJson(builder);
                var meta = JsonSerializer.Deserialize<TorrentMetadata>(builder.ToString());
                return meta;
            }
        }

        /// <summary>
        /// Parse the specified .torrent file.
        /// </summary>
        /// <param name="path">File path to torrent file.</param>
        /// <returns>A <see cref="TorrentMetadataFile"/> with the parsed contents.</returns>
        /// <exception cref="InvalidDataException">Thrown if file cannot be parsed.</exception>
        public static TorrentMetadata Parse(string path)
        {
            using (FileStream stream = File.OpenRead(path))
                return Parse(stream);
        }

        /// <summary>
        /// Attempt to parse a torrent file, returning <see langword="true"/> if
        /// the parsing is successful. No exceptions are thrown.
        /// </summary>
        /// <param name="path">File path to the .torrent file.</param>
        /// <param name="metafile">The <see cref="TorrentMetadataFile"/> as parsed.
        /// <see langword="null"/> if file cannot be parsed.</param>
        /// <returns><see langword="true"/> if successful, <see langword="false"/> if not.</returns>
        public static bool TryParse(string path, out TorrentMetadata metafile)
        {
            try
            {
                metafile = Parse(path);
                return true;
            }
            catch (UnauthorizedAccessException) { metafile = null; return false; }
            catch (IOException) { metafile = null; return false; }
            catch (InvalidDataException) { metafile = null; return false; }
        }

        private TorrentReader(Stream reader)
        {
            inputStream = reader ?? throw new ArgumentNullException(nameof(reader));
        }

        /// <summary>
        /// Parses the torrent file stream, encoding the result in
        /// a <see cref="IBElement"/>.
        /// </summary>
        /// <returns>A tokenized representation of the torrent file.</returns>
        private IBElement Parse()
        {
            return ReadToken();
        }

        /// <summary>
        ///   Read the next token and return it.
        /// </summary>
        /// <remarks>
        ///   This is where the recursive descent magic happens. The returned
        ///   token can be any of the defined types, from the simple types, <see
        ///   cref="BInteger"/> or <see cref="BString"/> to the complex ones,
        ///   <see cref="BList"/> or <see cref="BDictionary"/>.
        /// </remarks>
        /// <returns>
        ///   An <see cref="IBElement"/>; any of the defined types. Returns <see
        ///   langword="null"/> when EOF is reached or the end of a type is
        ///   scanned.
        /// </returns>
        private IBElement ReadToken()
        {
            var ch = ReadChar();
            switch (ch)
            {
                case char.MinValue:
                case 'e': // found end of object
                    return null;

                case 'i': // read integer.
                    return ReadNumber('e');
                case 'l': // read list
                    return ReadList();
                case 'd': // read dictionary
                    return ReadDictionary();

                case var x when (x >= '0') && (x <= '9'):       // read string value
                    var count = ReadNumber(':', x);
                    return ReadString((count as BInteger).Value);
                default:
                    throw new InvalidDataException("Unrecognized token parsing BEncoded string");
            }
        }

        /// <summary>
        ///   Read a string value, encoded as a raw integer defining the length,
        ///   a colon, then the string itself.
        /// </summary>
        /// <remarks>
        ///   An example would be '7:letters'
        /// </remarks>
        private IBElement ReadString(long count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException("Count < 0 in ReadString");

            var buf = new byte[count];
            inputStream.Read(buf, 0, (int)count);
            return new BString(Encoding.UTF8.GetString(buf));
        }

        /// <summary>
        ///   Read a dictionary value. A dictionary is encoded as the letter 'd'
        ///   followed by a series of string/value tuples, then the letter 'e'.
        /// </summary>
        /// <remarks>
        ///   An example is <c>'d4:spam3foo3bari42ee'</c> which is a dictionary
        ///   of <c>'spam:foo' and 'bar:42'</c>.
        /// <para/>
        ///   At invocation, the initial 'd' is already scanned and the parser
        ///   is expecting a string (preceded by an integer, natch). The
        ///   dictionary is stored in a IBElement, which allows the storage of
        ///   anything, just like the dictionary.
        /// </remarks>
        private IBElement ReadDictionary()
        {
            BDictionary ret = new BDictionary();

            while (true)
            {
                // This first token will always be a string, which ReadToken
                // can scoop up with the '0'..'9' cases.
                var key = ReadToken();
                if (key is null)
                {
                    return ret;
                }
                //
                // The second token could be anything: a string,
                // an int, a list, or another dictionary
                var value = ReadToken();
                if (value is null)
                {
                    return ret;
                }
                ret.Add((key as BString).Value, value);
            }
        }

        /// <summary>
        ///   Read a list value. A list value is defined as the letter 'l', a
        ///   series of values, then the letter 'e'.
        /// </summary>
        /// <remarks>
        ///   An example is <c>'l4:spami42ee'</c> which is a list of two
        ///   elements, a string of 4 characters, and the integer 42.
        /// <para/>
        ///   When this method is invoked, the initial 'l' has already been
        ///   scanned. It then recurses on reading tokens and stores the
        ///   returned values in a IBList.
        /// </remarks>
        /// <returns>
        ///   An <see cref="IBElement"/>, in this case, a <see cref="BList"/>.
        /// </returns>
        private IBElement ReadList()
        {
            // read tokens until null
            var ret = new BList();

            var value = ReadToken();
            while (value != null)
            {
                ret.Add(value);
                value = ReadToken();
            }
            return ret;
        }

        /// <summary>
        ///   Reads an integer value, either an integer by itself, as a value:
        ///   <c>'i23e'</c>, or an integer prefix for a string, defining the string
        ///   length: <c>'6:string'</c>
        /// </summary>
        /// <remarks>
        ///   By itself, the parser will have just scanned the letter 'i', and the
        ///   number is terminated with 'e'.
        /// <para/>
        ///   When defining a string length, the parser scanned a digit, which is
        ///   then added to the front of the value, and scanned until the
        ///   terminator is reached, in this case, ':'.
        /// </remarks>
        /// <returns>
        ///   An <see cref="IBElement"/>, in this case, a <see cref="BInteger"/>.
        /// </returns>
        private IBElement ReadNumber(char terminator, char? prefix = null)
        {
            var str = ReadUntil(terminator, prefix);
            return long.TryParse(str, out long value)
                ? new BInteger(value)
                : throw new InvalidDataException($"Could not parse number: {str}");
        }

        /// <summary>
        /// Reads characters until reaching the terminator character. If the
        /// prefix is defined, make it the first character of the returned
        /// string.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> of the chars between the prefix and the
        /// <paramref name="terminator"/>.
        /// </returns>
        private string ReadUntil(char terminator, char? prefix = null)
        {
            var sb = new StringBuilder();
            if (prefix.HasValue)
            {
                sb.Append(prefix.Value);
            }

            char ch = ReadChar();
            while (ch != terminator && ch != char.MinValue)
            {
                sb.Append(ch);
                ch = ReadChar();
            }
            return sb.ToString();
        }

        private readonly byte[] buffer = new byte[1];

        private char ReadChar()
        {
            return inputStream.Read(buffer, 0, 1) == 0 ? char.MinValue : Convert.ToChar(buffer[0]);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    inputStream.Dispose();
                }
                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
