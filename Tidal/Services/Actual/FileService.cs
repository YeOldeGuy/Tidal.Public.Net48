using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using Tidal.Services.Abstract;
using Env = System.Environment;

namespace Tidal.Services.Actual
{
    internal class FileService : IFileService
    {
        private readonly string appname =
            Path.GetFileNameWithoutExtension(AppDomain.CurrentDomain.FriendlyName) + "-Net48";


        public void CreateDirectory(StorageStrategy strategy)
        {
            Directory.CreateDirectory(GetStorageLocation(strategy));
        }

        public string GetStorageLocation(StorageStrategy strategy = StorageStrategy.Local)
        {
            // This should be something like "C:/Users/bobke/AppData":
            string appdata = Env.GetFolderPath(Env.SpecialFolder.ApplicationData);
            string path = string.Empty;

            switch (strategy)
            {
                // Return something like "C:/Users/bobke/AppData/Local/Tidal"
                case StorageStrategy.Local:
                    path = Path.Combine(Env.GetFolderPath(
                        Env.SpecialFolder.LocalApplicationData), appname);
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);
                    return path;

                // Return something like "C:/Users/bobke/AppData/Roaming/Tidal" 
                case StorageStrategy.Roaming:
                    path = Path.Combine(appdata, "Roaming", appname);
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);
                    return path;

                // Return something like: "C:/Users/bobke/AppData/Local/Temp"
                case StorageStrategy.Temporary:
                    return Path.GetTempPath();

                // Finally, return the empty string for no strategy
                case StorageStrategy.None:
                default:
                    return path;
            }
        }

        private void CheckFileName(string filename)
        {
            if (string.IsNullOrEmpty(filename))
                throw new ArgumentException(
                    $"'{nameof(filename)}' cannot be null or empty",
                    nameof(filename));
        }


        public string GetFilePath(string filename,
                                  StorageStrategy strategy = StorageStrategy.Local)
        {
            CheckFileName(filename);

            return Path.Combine(GetStorageLocation(strategy), filename);
        }

        public string GetTempFile(string suffix = "",
                                  StorageStrategy strategy = StorageStrategy.Temporary)
        {
            string path = GetStorageLocation(strategy);
            string rand = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
            if (suffix != null && suffix.Length > 0 && !suffix.StartsWith("."))
                suffix = "." + suffix;
            return Path.Combine(path, $"{rand}{suffix}");
        }

        public async Task<bool> FileExistsAsync(string filename,
                                                StorageStrategy strategy = StorageStrategy.Local)
        {
            CheckFileName(filename);

            return await Task.Run(() => FileExists(filename, strategy));
        }

        public bool FileExists(string filename,
                               StorageStrategy strategy = StorageStrategy.Local)
        {
            CheckFileName(filename);

            if (Path.IsPathRooted(filename))
                return File.Exists(filename);

            string path = GetFilePath(filename, strategy);
            return File.Exists(path);
        }

        public string ReadAllText(string filename,
                                  StorageStrategy strategy = StorageStrategy.Local)
        {
            CheckFileName(filename);

            string path = GetFilePath(filename, strategy);
            return File.ReadAllText(path);
        }

        public async Task<string> ReadAllTextAsync(string filename,
                                                   StorageStrategy strategy = StorageStrategy.Local)
        {
            CheckFileName(filename);

            using (var reader = new StreamReader(GetFilePath(filename, strategy)))
            {
                return await reader.ReadToEndAsync();
            }
        }

        public async Task<byte[]> ReadAllBytesAsync(string filename,
                                                    StorageStrategy strategy = StorageStrategy.Local)
        {
            CheckFileName(filename);

            return await Task.Run(() =>
            {
                var buf = File.ReadAllBytes(GetFilePath(filename, strategy));
                return buf;
            });
        }

        public byte[] ReadAllBytes(string filename,
                                   StorageStrategy strategy = StorageStrategy.Local)
        {
            CheckFileName(filename);

            if (!FileExists(filename, strategy))
                return null;
            return File.ReadAllBytes(GetFilePath(filename, strategy));
        }

        public bool WriteAllText(string contents,
                                 string filename,
                                 StorageStrategy strategy = StorageStrategy.Local)
        {
            CheckFileName(filename);

            string path = GetFilePath(filename, strategy);
            File.WriteAllText(path, contents, Encoding.UTF8);
            return FileExists(path, strategy);
        }

        public async Task<bool> WriteAllTextAsync(string contents,
                                                  string filename,
                                                  StorageStrategy strategy = StorageStrategy.Local)
        {
            CheckFileName(filename);

            return await Task.Run(() => WriteAllText(contents, filename, strategy));
        }

        public bool WriteAllBytes(byte[] buffer,
                                  string filename,
                                  StorageStrategy strategy = StorageStrategy.Local)
        {
            CheckFileName(filename);

            string path = GetFilePath(filename, strategy);
            File.WriteAllBytes(path, buffer);
            return FileExists(path);
        }

        public async Task<bool> WriteAllBytesAsync(byte[] buffer,
                                                   string filename,
                                                   StorageStrategy strategy = StorageStrategy.Local)
        {
            CheckFileName(filename);

            string path = GetFilePath(filename, strategy);
            using (var writer = new FileStream(path, FileMode.OpenOrCreate))
            {
                await writer.WriteAsync(buffer, 0, buffer.Length);
                writer.Flush();
            }
            return await FileExistsAsync(filename, strategy);
        }

        public async Task<bool> WriteStreamAsync(Stream stream,
                                                 string filename,
                                                 StorageStrategy strategy = StorageStrategy.Local)
        {
            CheckFileName(filename);

            string path = GetFilePath(filename, strategy);
            using (var outStream = new FileStream(path, FileMode.OpenOrCreate))
            {
                await stream.CopyToAsync(outStream);
            }
            return await FileExistsAsync(filename, strategy);
        }

        public bool Delete(string filename,
                           StorageStrategy strategy = StorageStrategy.Local)
        {
            CheckFileName(filename);
            string path = GetFilePath(filename, strategy);

            if (!File.Exists(path))
                return false;

            File.Delete(path);
            return !File.Exists(path);
        }

        public async Task<bool> DeleteAsync(string filename,
                           StorageStrategy strategy = StorageStrategy.Local)
        {
            CheckFileName(filename);
            string path = GetFilePath(filename, strategy);

            if (!File.Exists(path))
                return false;

            File.Delete(path);
            return !await FileExistsAsync(filename, strategy);
        }

        public void Move(string sourcefile,
                         string destinationfile,
                         StorageStrategy sourceStrategy = StorageStrategy.None,
                         StorageStrategy destinationStrategy = StorageStrategy.None,
                         bool overwrite = false)
        {
            CheckFileName(sourcefile);
            CheckFileName(destinationfile);

#if NETCOREAPP
            File.Move(GetFilePath(sourcefile, sourceStrategy),
                      GetFilePath(destinationfile, destinationStrategy),
                      overwrite);
#else
            if (overwrite && File.Exists(GetFilePath(destinationfile, destinationStrategy)))
                File.Delete(GetFilePath(destinationfile, destinationStrategy));

            File.Move(GetFilePath(sourcefile, sourceStrategy),
                      GetFilePath(destinationfile, destinationStrategy));
#endif
        }

        public void Copy(string sourcefile,
                         string destinationfile,
                         StorageStrategy sourceStrategy = StorageStrategy.None,
                         StorageStrategy destinationStrategy = StorageStrategy.None,
                         bool overwrite = false)
        {
            CheckFileName(sourcefile);
            CheckFileName(destinationfile);

            File.Copy(GetFilePath(sourcefile, sourceStrategy),
                      GetFilePath(destinationfile, destinationStrategy),
                      overwrite);
        }


        public TimeSpan GetFileAge(string filename,
                                   StorageStrategy strategy = StorageStrategy.Local)
        {
            CheckFileName(filename);

            return DateTime.Now - new FileInfo(GetFilePath(filename, strategy)).LastWriteTime;
        }


        public async Task<string> DecompressFileAsync(string filename,
                                                      StorageStrategy strategy = StorageStrategy.None)
        {
            CheckFileName(filename);

            if (strategy != StorageStrategy.None)
                filename = GetFilePath(filename, strategy);

            var fileToDecompress = new FileInfo(filename);

            try
            {
                string curFName = fileToDecompress.FullName;
                var extlen = fileToDecompress.Extension.Length;
                string newFileName = extlen > 0
                    ? curFName.Remove(curFName.Length - fileToDecompress.Extension.Length)
                    : curFName + ".tmp";
                using (FileStream decompressedFileStream = File.Create(newFileName))
                using (FileStream origStrm = fileToDecompress.OpenRead())
                using (var zipStrm = new GZipStream(origStrm, CompressionMode.Decompress))
                {
                    await zipStrm.CopyToAsync(decompressedFileStream);
                }
                return newFileName;
            }
            catch (IOException)
            {
                return null;
            }
        }

        public DateTime GetFileLastWriteTime(
            string filename,
            StorageStrategy strategy = StorageStrategy.Local)
        {
            CheckFileName(filename);

            string path = GetFilePath(filename, strategy);
            var fileinfo = new FileInfo(path);
            if (fileinfo != null)
                return fileinfo.LastWriteTime;
            return DateTime.Now;
        }

        public async Task<long> GetFileSizeAsync(
            string filename,
            StorageStrategy strategy = StorageStrategy.Local)
        {
            CheckFileName(filename);

            long size = await Task.Run(() =>
            {
                return GetFileSize(filename, strategy);
            });
            return size;
        }

        public long GetFileSize(string filename,
                                StorageStrategy strategy = StorageStrategy.Local)
        {
            CheckFileName(filename);

            string path = GetFilePath(filename, strategy);
            var fileinfo = new FileInfo(path);
            if (fileinfo != null)
                return fileinfo.Length;
            return 0;
        }
    }
}
