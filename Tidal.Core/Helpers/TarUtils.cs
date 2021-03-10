using System;
using System.IO;
using System.Text;

// Adapted from: https://gist.github.com/ForeverZer0/a2cd292bd2f3b5e114956c00bb6e872b

namespace Tidal.Core.Helpers
{
    /// <summary>
    /// Please, for the love of god, don't use this as a general purpose tar
    /// file reader. It does one thing: extracts a single file from a tarball,
    /// stripping any leading directory information. It does almost no error
    /// checking, assuming that the folks at MaxMind wouldn't supply a faulty
    /// tar file.
    /// </summary>
    public static class TarUtils
    {
        /// <summary>
        /// Extracts a specific file from the tar, then returns.
        /// </summary>
        /// <param name="filepath">The full path of the tar file</param>
        /// <param name="outputDir">The directory to write the extracted file to</param>
        /// <param name="fileToExtract">
        ///   The name of the file within the tar to extract. Uses 
        ///   <see cref="string.EndsWith(string)"/> to find the file.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the file was extracted successfully.
        /// </returns>
        public static bool ExtractSingleFileFromTar(string filepath,
                                                    string outputDir,
                                                    string fileToExtract,
                                                    bool setModDate = false)
        {
            using (var stream = File.OpenRead(filepath))
                return ExtractSingleFileFromTar(stream, outputDir, fileToExtract, setModDate);
        }

        /// <summary>
        ///   Extracts a specific file from the tar. It is contingent upon the
        ///   caller to close/dispose of the source <paramref
        ///   name="tarStream"/>.
        /// </summary>
        /// <param name="tarStream">
        ///   The tar file presented as an open <see cref="Stream"/>. This will
        ///   not be closed or disposed.
        /// </param>
        /// <param name="outputDir">
        ///   The directory to write the extracted file to. The output file
        ///   will have the same name as in the tarball.
        /// </param>
        /// <param name="fileToExtract">
        ///   The name of the file within the tar file to extract. This is the
        ///   name the extracted file will have.
        /// </param>
        /// <param name="setModDate"></param>
        ///   If <see langword="true"/>, sets the file modification
        ///   date to be that supplied by the tar file, otherwise
        ///   it will be set to <see cref="DateTime.Now"/>.
        /// <returns>
        ///   <see langword="true"/> if the file was extracted successfully. If
        ///   an error occurs, a partially written file may exist.
        /// </returns>
        public static bool ExtractSingleFileFromTar(Stream tarStream,
                                                    string outputDir,
                                                    string fileToExtract,
                                                    bool setModDate = false)
        {
            var buffer = new byte[100];

            while (true)
            {
                // The first 100 bytes are the file name, padded with nulls
                if (tarStream.Read(buffer, 0, 100) != 100)
                    return false;
                var name = Encoding.ASCII.GetString(buffer).Trim('\0');

                // An empty file name is considered a failure
                if (string.IsNullOrWhiteSpace(name))
                    return false;

                // At offset 124 (100 for the name, plus 24) is the size,
                // encoded in octal, written in ASCII, and null padded. I *love*
                // the old Unix way of thinking. A tarball of files could be
                // looked at in a binary editor and understood. 
                //
                // Although we don't use them, what's in that extra 24
                // intervening bytes? The file mode (the drwxrwxr-x stuff),
                // encoded in octal (the way God and Kernighan intended), then
                // written in ASCII, like 00000775. That's eight bytes. The next
                // 8 are the user id, then another 8 bytes for the group id,
                // both octal/ASCII encoded.
                // 
                // Pure and simple. You have to admire it. But you also have
                // to skip it, because we just don't care right now 😜

                tarStream.Seek(24, SeekOrigin.Current); // now at offset 124

                // The next 12 bytes (octal/ASCII) has the size of the file in
                // bytes.
                if (tarStream.Read(buffer, 0, 12) != 12 || buffer[0] == 0)
                    return false;
                var size = Convert.ToInt64(Encoding.UTF8.GetString(buffer, 0, 12).Trim('\0').Trim(), fromBase: 8);

                // Get the mtime of the file...
                // ...a time_t value, encoded in octal, stored in ASCII
                if (tarStream.Read(buffer, 0, 12) != 12 || buffer[0] == 0)
                    return false;
                var mtime = Convert.ToInt64(Encoding.UTF8.GetString(buffer, 0, 12).Trim('\0').Trim(), fromBase: 8);
                var modTime = DateTimeOffset.FromUnixTimeSeconds(mtime);

                // Move the position to the next 512-byte interval
                tarStream.Seek(364, SeekOrigin.Current);

                var outputFileName = Path.Combine(outputDir, Path.GetFileName(name));

                // So, now we have all the header information, including the
                // name of the file in the tarball. Don't try anything fancy to
                // see if this is the one we want: Look at the last characters;
                // if they match, then run with it. I do this so I don't have to
                // figure out the directory prefix for the file I want, and this
                // is why you most definitely should NOT use this as a general
                // purpose tar file extractor.

                if (name.EndsWith(fileToExtract))
                {
                    // Read and write the file in chunks of 4K. You could adjust
                    // this to be bigger, but why? Just don't read the whole
                    // file into memory; it's wasteful and not necessary. This
                    // is fast and efficient.

                    var buf = new byte[4096];
                    int bytesLeft = (int)size;
                    using (FileStream outStream = File.Open(outputFileName, FileMode.Create, FileAccess.Write))
                    {
                        while (bytesLeft > 0)
                        {
                            // Try to read a whole buffer's worth, or at least
                            // what's left
                            int bytesRequested = Math.Min(buf.Length, bytesLeft);

                            // Read returns the number of bytes actually read:
                            int amountRead = tarStream.Read(buf, 0, bytesRequested);

                            // If the number of bytes read ended up being less
                            // than what we asked for, then the filesize stored
                            // in the tar header was incorrect or the tarfile
                            // got truncated somehow; we hit the end-of-file
                            // before getting all the bytes.
                            if (amountRead < bytesRequested)
                            {
                                // It's an error to be here, but we might as well
                                // write what we did get to the output file, even
                                // though we know it's not correct. 
                                if (amountRead > 0)
                                    outStream.Write(buf, 0, amountRead);
                                return false;
                            }

                            outStream.Write(buf, 0, amountRead);
                            bytesLeft -= amountRead;
                        }
                    }
                    // Change the file we wrote to match the timestamp of the
                    // tarfile, if requested.
                    if (setModDate)
                    {
                        File.SetLastWriteTimeUtc(outputFileName, modTime.UtcDateTime);
                    }
                    return true;
                }
                else
                {
                    // this wasn't the droid we were looking for, so skip
                    // forward by the size of the file
                    tarStream.Seek(size, SeekOrigin.Current);
                }

                // Current spot in the tar file is directly after an internal
                // file. Headers are written at locations divisible by 512, so
                // skip forward to the next 512 byte multiple:

                var pos = tarStream.Position;

                var offset = 512 - (pos % 512);
                if (offset == 512)
                    offset = 0;

                tarStream.Seek(offset, SeekOrigin.Current);
            }
        }
    }
}
