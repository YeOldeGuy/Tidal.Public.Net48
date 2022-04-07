using MaxMind.Db;
using MaxMind.GeoIP2;
using MaxMind.GeoIP2.Exceptions;
using Prism.Mvvm;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tidal.Core.Collections;
using Tidal.Core.Helpers;
using Tidal.Models;
using Tidal.Models.Messages;
using Tidal.Properties;
using Tidal.Services.Abstract;

namespace Tidal.Services.Actual
{
    internal class GeoService : BindableBase, IGeoService, IDisposable
    {
        private const StorageStrategy storageStrategy = StorageStrategy.Local;

        private readonly SemaphoreSlim downloadSemaphore = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim initSemaphore = new SemaphoreSlim(1, 1);
        private readonly IFileService fileService;
        private readonly ISettingsService settings;
        private readonly IMessenger messenger;
        private DatabaseReader dbreader;
        private bool _IsDownloading;

        // Keep a quick-lookup table of the most recently used GeoLocations.
        // When the peers are displayed, for a period of time, they tend to stay
        // the same for a while until the next announce time. As getting the
        // location data from the mmdb file can be expensive, cache the latest
        // ones.

        private readonly LurchTable<string, GeoLocation> geoCache =
            new LurchTable<string, GeoLocation>(LurchTableOrder.Access, 500);

        public GeoService(IFileService fileService,
                          ISettingsService settings,
                          IMessenger messenger,
                          ITaskService taskService)
        {
            this.fileService = fileService;
            this.settings = settings;
            this.messenger = messenger;

            IsDownloading = false;

            taskService.Add(nameof(CheckMMDB), CheckIfCurrent, TimeSpan.FromDays(1));

            InitializeAsync();
        }

        private async Task CheckIfCurrent()
        {
            if (IsDownloading)
                return;

            messenger.Send(new StatusInfoMessage("Checking MMDB", TimeSpan.FromSeconds(3)));
            try
            {
                await CheckMMDB();
            }
            catch (Exception)
            {
                // ignore weird errors (I think it's a threading error)
            }
        }

        private async Task CheckMMDB()
        {
            if (!fileService.FileExists(settings.GeoDbFileName, storageStrategy) ||
                string.IsNullOrEmpty(settings.MaxMindUserName) ||
                string.IsNullOrEmpty(settings.MaxMindPassword) ||
                string.IsNullOrEmpty(settings.MaxMindLicenseKey))
            {
                return;
            }

            //
            // Yeah, we've got the file, but is it too old? MaxMind
            // updates it once a week on Tuesday, so check if our file
            // is out of date, then fetch it again if necessary.
            //
            long size = fileService.GetFileSize(settings.GeoDbFileName, storageStrategy);
            //
            // if the program crashes while downloading the GeoIp file,
            // then its size will be zero (most the time). The lookup
            // routines will obviously barf at a zero-length file, so
            // download it again.
            //
            if (size == 0 || DBFileAge > settings.GeoIpRetrievalInterval)
            {
                await DownloadMaxMindAsync(settings.MaxMindUserName,
                                           settings.MaxMindPassword,
                                           settings.MaxMindLicenseKey);
            }
        }

        public async Task RefreshDBFile()
        {
            CheckDisposed();

            // Don't even try to refresh the DB file if it's already
            // being downloaded.
            if (downloadSemaphore.CurrentCount == 0 || IsDownloading)
                return;

            // Let's only try to get past the initSemaphore for 10 seconds.
            // After that, assume that the initializing is currently fubar and
            // move on.
            bool gotTheLock = await initSemaphore.WaitAsync(TimeSpan.FromSeconds(10));
            if (!gotTheLock)
                return;

            try
            {
                if (await DownloadMaxMindAsync(settings.MaxMindUserName,
                                               settings.MaxMindPassword,
                                               settings.MaxMindLicenseKey))
                {
                    dbreader = await GetDatabaseReader();
                }
            }
            finally
            {
                initSemaphore.Release();
            }
        }

        public bool IsDownloading
        {
            get => _IsDownloading;
            private set => SetProperty(ref _IsDownloading, value);
        }

        public TimeSpan DBFileAge =>
            fileService.GetFileAge(settings.GeoDbFileName,
                                   storageStrategy);

        private async void InitializeAsync()
        {
            await initSemaphore.WaitAsync();
            try
            {
                if (!await fileService.FileExistsAsync(settings.GeoDbFileName,
                                                       storageStrategy))
                {
                    if (!await DownloadMaxMindAsync(settings.MaxMindUserName,
                                                    settings.MaxMindPassword,
                                                    settings.MaxMindLicenseKey))
                    {
                        if (!await fileService.ExtractEmbeddedResourceAsync("Tidal.GeoLite2-City.mmdb", settings.GeoDbFileName))
                            return;
                    }    
                }
                else
                {
                    await CheckMMDB();
                }
                dbreader = await GetDatabaseReader();
            }
            finally
            {
                initSemaphore.Release();
            }
        }

        /// <summary>
        /// Return the "City,St", "City, Country" or "Country" of the specified
        /// <see cref="CityResponse"/> value. Only the US and Canada get the
        /// "City,St" treatment.
        /// </summary>
        /// <param name="geo">
        ///   A <see cref="CityResponse"/> from the GeoIP2 methods.
        /// </param>
        /// <returns>A location string value.</returns>
        public string LocationToString(GeoLocation geo)
        {
            CheckDisposed();

            if (geo == null || !geo.IsCountryValid)
                return Resources.GeoService_UnknownCity;

            if (!geo.IsCityValid)
            {
                return geo.Country;
            }

            var iso = geo.CountryCode;

            // For the US and Canada, include the state/province if available.
            // When I try this on other countries, I get weird things like
            // "Paris, 75, France" which seems weird to these not-very-worldly
            // eyes. Using the Subdivision name vs. the iso code makes the
            // display of locations *way* too busy. Anyway, do other countries
            // besides huge ones like the US have issues with multiple cities
            // with the same name? We have 33 different Springfields in 25
            // states, which means that we've got states with MORE THAN ONE
            // SPRINGFIELD. What the hell is wrong with us?

            if (iso == "US" || iso == "CA")
            {
                if (string.IsNullOrEmpty(geo.City))
                    return geo.Country;

                var countryName = iso == "US" ? "USA" : "Canada";
                return $"{geo.City}, {geo.SubDivision}, {countryName}";
            }

            return $"{geo.City}, {geo.Country}";
        }

        private async Task<DatabaseReader> GetDatabaseReader()
        {
            // Don't try to get the DatabaseReader while we're converting
            // the downloaded file

            if (await downloadSemaphore.WaitAsync(TimeSpan.FromSeconds(30)))
            {
                try
                {
                    if (dbreader == null)
                    {
                        if (!fileService.FileExists(settings.GeoDbFileName, storageStrategy))
                        {
                            await DownloadMaxMindAsync(settings.MaxMindUserName,
                                                       settings.MaxMindPassword,
                                                       settings.MaxMindLicenseKey);
                        }

                        if (!fileService.FileExists(settings.GeoDbFileName, storageStrategy))
                            return null;

                        string path = fileService.GetFilePath(settings.GeoDbFileName,
                                                              storageStrategy);
                        dbreader = new DatabaseReader(path);
                    }
                    return dbreader;
                }
                finally
                {
                    downloadSemaphore.Release();
                }
            }
            return null;
        }

        public async Task<GeoLocation> GetRawLocationAsync(string ipAddress)
        {
            CheckDisposed();

            // This semaphore is set up to only allow a single wait. If the
            // CurrentCount is zero, then it's busy because a download is in
            // progress.

            if (downloadSemaphore.CurrentCount == 0 || IsDownloading)
                return null;

            return await Task.Run(async () =>
            {
                if (geoCache.TryGetValue(ipAddress, out var cachedValue))
                {
                    return cachedValue;
                }

                var reader = await GetDatabaseReader();
                if (reader == null)
                    return null;

                try
                {
                    var resp = reader.City(ipAddress);
                    var geoloc = GeoLocation.FromMaxMindCityResponse(resp);
                    geoCache.TryAdd(ipAddress, geoloc);
                    return geoloc;
                }
                catch (InvalidDatabaseException) { return null; }
                catch (AddressNotFoundException) { return GeoLocation.FromNotFoundResponse(ipAddress); }
                catch (DeserializationException) { return null; }
            });
        }

        public GeoLocation GetRawLocation(string ipAddress)
        {
            CheckDisposed();

            if (downloadSemaphore.CurrentCount == 0 || IsDownloading)
                return null;

            try
            {
                if (dbreader != null)
                {
                    if (geoCache.TryGetValue(ipAddress, out var cachedValue))
                    {
                        return cachedValue;
                    }

                    cachedValue = GeoLocation.FromMaxMindCityResponse(dbreader.City(ipAddress));
                    geoCache.TryAdd(ipAddress, cachedValue);
                    return cachedValue;
                }
                return null;
            }
            catch (InvalidDatabaseException) { return null; }
            catch (AddressNotFoundException) { return GeoLocation.FromNotFoundResponse(ipAddress); }
            catch (DeserializationException) { return null; }
        }

        public async Task<string> GetFormattedLocationAsync(string ipAddress)
        {
            CheckDisposed();

            return LocationToString(await GetRawLocationAsync(ipAddress));
        }

        public string GetFormattedLocation(string ipAddress)
        {
            if (dbreader != null)
                return LocationToString(GetRawLocation(ipAddress));
            return "";
        }

        public async Task<bool> LoadDbFileAsync(string compressedDbFile, bool deleteOriginal = false)
        {
            if (IsDownloading)
                return false;

            IsDownloading = true;
            try
            {
                string decompressedTempFile;

                if (compressedDbFile.EndsWith(".mmdb", StringComparison.OrdinalIgnoreCase))
                {
                    decompressedTempFile = compressedDbFile;
                }
                else if (compressedDbFile.EndsWith(".mmdb.gz", StringComparison.OrdinalIgnoreCase))
                {
                    deleteOriginal = true;
                    decompressedTempFile = await fileService.DecompressFileAsync(compressedDbFile, StorageStrategy.None);
                }
                else
                {
                    messenger.Send(new WarningMessage(Resources.GeoService_FilesNeeded, Resources.GeoService_UnknownFormat));
                    return false;
                }

                if (string.IsNullOrEmpty(decompressedTempFile))
                    return false;

                string geoIpFilePath = settings.GeoDbFileName;

                dbreader?.Dispose();
                dbreader = null; // keep from accessing disposed element

                if (deleteOriginal)
                    fileService.Move(sourcefile: decompressedTempFile,
                                     destinationfile: geoIpFilePath,
                                     sourceStrategy: StorageStrategy.None,
                                     destinationStrategy: storageStrategy,
                                     overwrite: true);
                else
                    fileService.Copy(sourcefile: decompressedTempFile,
                                     destinationfile: geoIpFilePath,
                                     sourceStrategy: StorageStrategy.None,
                                     destinationStrategy: storageStrategy,
                                     overwrite: true);

                dbreader = await GetDatabaseReader();
                messenger.Send(new StatusInfoMessage(Resources.GeoService_Successful, TimeSpan.FromSeconds(10)));

                return true;
            }
            finally
            {
                IsDownloading = false;
            }
        }

        public async Task<bool> DownloadMaxMindAsync(string username, string password, string licenseKey)
        {
            if (IsDownloading ||
                string.IsNullOrEmpty(username) ||
                string.IsNullOrEmpty(password) ||
                string.IsNullOrEmpty(licenseKey))
            {
                return false;
            }

            IsDownloading = true;
            try
            {
                using (var client = new HttpClient())
                {
                    if (username.Contains(":"))
                        throw new ArgumentException("Username cannot contain ':' character", nameof(username));

                    var authtoken = Encoding.ASCII.GetBytes($"{username}:{password}");

                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authtoken));

                    // Build the secret decoder ring url:
                    var urlstr = settings.MaxMindPermaLink.Replace("YOUR_LICENSE_KEY", licenseKey);

                    if (await downloadSemaphore.WaitAsync(TimeSpan.FromSeconds(30)))
                    {
                        // Close the dbreader so that we can write to the file. Set the
                        // dbreader to null, making us reopen it the next time there's a
                        // request.
                        dbreader?.Dispose();
                        dbreader = null;

                        try
                        {
                            Uri uri = new Uri(urlstr);
                            string tempfile = fileService.GetTempFile(".tar.gz", StorageStrategy.Temporary);
                            Stream strm = await client.GetStreamAsync(uri);
                            if (!await fileService.WriteStreamAsync(strm, tempfile, StorageStrategy.None))
                            {
                                messenger.Send(new WarningMessage(
                                    string.Format(Resources.GeoService_CannotWriteTempFile_1, tempfile),
                                    Resources.GeoService_WriteError));
                                return false;
                            }

                            // That actually worked? Cool! Now, pull the .tar.gz file apart
                            // and extract the GeoLite2-City.mmdb file from it.

                            // So, at this point, we've downloaded the special file from
                            // MaxMind, using our secret decoder ring. That file is in
                            // 'tempxxxx.tar.gz' over in the /temp directory. Let's
                            // deccompress it and remove the .gz extension
                            string tarfile = await fileService.DecompressFileAsync(tempfile, StorageStrategy.Temporary);

                            // Check that it worked:
                            if (string.IsNullOrEmpty(tarfile) || !fileService.FileExists(tarfile, StorageStrategy.Temporary))
                                return false;

                            // Now, in the temp directory, we have a randomly named
                            // tarball like 'u8y4f8aa.tar', which has the actual file we
                            // want, namely, "GeoLite2-City.mmdb"

                            // Do the actual extraction of the GeoLite file from the tar
                            // file supplied by MaxMind
                            var extractionSuccessful =
                                TarUtils.ExtractSingleFileFromTar(
                                    filepath: tarfile,
                                    outputDir: fileService.GetStorageLocation(storageStrategy),
                                    fileToExtract: settings.GeoDbFileName);

                            // Even if the extraction didn't work, try to delete the two
                            // temp files. The first is the "u8y4f8aa.tar.gz" file that
                            // we downloaded the data from MaxMind to...
                            if (!await fileService.DeleteAsync(tempfile, StorageStrategy.None))
                                messenger.Send(new InfoMessage(string.Format(Resources.GeoService_CannotDelete_1, tempfile),
                                                               Resources.GeoService_FileError,
                                                               TimeSpan.FromSeconds(5)));

                            // ...And the second is the uncompressed version, the
                            // "u8y4f8aa.tar" file
                            if (!await fileService.DeleteAsync(tarfile, StorageStrategy.None))
                                messenger.Send(new InfoMessage(string.Format(Resources.GeoService_CannotDelete_1, tarfile),
                                                               Resources.GeoService_FileError,
                                                               TimeSpan.FromSeconds(5)));

                            if (!extractionSuccessful)
                                return false;

                            messenger.Send(new StatusInfoMessage(Resources.GeoService_DownloadedGeoIP2));
                            messenger.Send(new SaveSettingsMessage(settings));
                            await settings.SaveAsync();
                        }
                        catch (HttpRequestException ex)
                        {
                            // send a message advising of the failure to download
                            messenger.Send(
                                new WarningMessage(
                                    string.Format(Resources.GeoService_DownloadWarning_2, settings.GeoDbFileName, ex.Message),
                                    "Download Error"));
                        }
                        finally
                        {
                            downloadSemaphore.Release();
                        }
                        // If we're at this point, then everything supposedly
                        // worked. One last check to make sure everything did
                        // actually work...
                        string path = fileService.GetFilePath(settings.GeoDbFileName, storageStrategy);
                        if (await fileService.FileExistsAsync(path))
                        {
                            //Messenger.Send(new GeoDbDownloadedMessage());
                            dbreader = new DatabaseReader(path);
                            return true;
                        }
                    }
                    return false;
                }
            }
            finally
            {
                IsDownloading = false;
            }
        }

        private void CheckDisposed()
        {
            if (disposedValue)
                throw new ObjectDisposedException(GetType().FullName);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    dbreader?.Dispose();
                    downloadSemaphore.Dispose();
                    initSemaphore.Dispose();
                    geoCache.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion IDisposable Support
    }
}
