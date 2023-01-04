/*
 * M3U8FileService.cs
 * M3U8FileService allow the application to download & read a m3u8 that contains the HLS data.
 * Date : 2021-05-24.
 * By : Jonathan Massé
 */


// Includes.
using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using RadioHLSConverter.backend.serverless.Model.M3U8;
using System.Globalization;
using RadioHLSConverter.backend.serverless.Helpers;

namespace RadioHLSConverter.backend.serverless.Services
{
    public class M3U8FileService : IM3U8FileService
    {
        // Constants.
        private const int MAXIMUM_SEGMENTS_IN_LIST = 30;

        // Regex.
        private static Regex _regexIsM3U8 = new Regex("#EXTM3U", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant); // Ex : #EXTM3U at the beginning of the M3U8 file.
        private static Regex _regexVersion = new Regex("#EXT-X-VERSION:([0-9]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant); // Ex : #EXT-X-VERSION:[WITH A VERSION NUMBER]
        private static Regex _regexListStreams = new Regex("#EXT-X-STREAM-INF:(.*)\n((?!#).*)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant); // Ex : #EXT-X-STREAM-INF:[Duration]
        private static Regex _regexListSegments = new Regex("#EXTINF:(.*),.*\n((?!#).*)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);// Ex : #EXTINF:[INFO],[OTHER INFORMATIONS]\n[SEGMENT FILENAME]
        private static Regex _regexURL = new Regex("(https?:\\/\\/.+\\/)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant); // Ex : http://url.com/path/test.m3u8 will give http://url.com/path/

        // Http client.
        private HttpClient _client = new HttpClient(new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.MaxValue, // Set HTTP connection lifetime to unlimited.
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(5) // Close HTTP connection after 5 minutes of idle.
        });

        // Properties.
        public bool IsM3U8 => _regexIsM3U8.IsMatch(Data); // If the last downloaded m3u8 is valid.
        public int Version => _regexVersion.Match(Data).Success ? int.Parse(_regexVersion.Match(Data).Groups[1].Value, NumberStyles.Number, CultureInfo.InvariantCulture) : 0; // The last m3u8 file version.
        public string Data { get; private set; } // The last m3u8 file data.
        public string URL { get; private set; } // The last m3u8 file full URL.
        public string URLPath { get; private set; } // The last m3u8 file URL path (without the filename).


        /// <summary>
        /// M3U8 list of streams.
        /// </summary>
        public IList<M3U8Stream> Streams { get; private set; } = new List<M3U8Stream>();

        /// <summary>
        /// M3U8 list of segments.
        /// </summary>
        public IList<M3U8Segment> Segments { get; private set; } = new List<M3U8Segment>();


        ///////////////////////////////////////////////////
        // Destructor.
        ///////////////////////////////////////////////////
        public void Dispose()
        {
            _client?.Dispose();
        }
        

        #region Load M3U8 file
        /// <summary>
        /// Load the M3U8 file from an url.
        /// Once the download is completed it will automatically load the M3U8 file.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task LoadM3U8File(string url, CancellationToken cancellationToken)
        {
            URL = url;
            URLPath = _regexURL.Match(url).Groups[1].Value;
            await LoadCurrentM3U8File(cancellationToken);
        }

        /// <summary>
        /// Load the M3U8 file from an url.
        /// Once the download is completed it will automatically load the M3U8 file.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task LoadCurrentM3U8File(CancellationToken cancellationToken)
        {
            HttpResponseMessage response = await _client.GetAsync(URL, cancellationToken);

            // If HTTP status is valid.
            if (response.IsSuccessStatusCode)
            {
                Data = await response.Content.ReadAsStringAsync(cancellationToken);
                await ProcessFile(cancellationToken);
            }
            // Invalid HTTP status url.
            else
            {
                throw new Exception(Resources.Resource.error_invalid_m3u8_file_url + " - Status code : " + response.StatusCode.ToString() + " - " + response?.ReasonPhrase);
            }
        }

        /// <summary>
        /// Load the M3U8 stream file from an url.
        /// Once the download is completed it will automatically load the M3U8 file.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task LoadStreamFile(M3U8Stream stream, CancellationToken cancellationToken)
        {
            await LoadM3U8File(URLPath + stream.StreamFilename, cancellationToken);
        }
        #endregion

        #region Download segment
        /// <summary>
        /// Download an m3u8 segment and return the segment data.
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<byte[]> DownloadSegment(M3U8Segment segment, CancellationToken cancellationToken)
        {
            // Download segment data.
            return await _client.GetByteArrayAsync(URLPath + segment.SegmentFilename, cancellationToken);
        }
        #endregion

        #region Segment position
        /// <summary>
        /// Return the first segment to be read based on the required buffer size in the appsettings.json
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public M3U8Segment GetFirstSegment(decimal bufferSize = 30m)
        {
            decimal bufferSizeInSeconds = 0m;
            var firstSegment = Segments.Reverse().SkipWhile(x => (bufferSizeInSeconds += x.Length) < bufferSize).FirstOrDefault();

            // If buffer size is too big. i.e. (If available radio m3u8 segments is less than the application buffer then use the first segment.)
            if (firstSegment == null)
                firstSegment = Segments.First();

            return firstSegment;
        }

        /// <summary>
        /// GetNextSegmentAndUpdateM3U8
        /// Get the next segment and it will automatically re-download the newer M3U8 if needed to get the new list of segments.
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<M3U8Segment> GetNextSegmentAndUpdateM3U8(M3U8Segment segment, CancellationToken cancellationToken)
        {
            var nextSegment = GetNextSegment(segment);

            // Next segment found.
            if (nextSegment != null)
                return nextSegment;


            // While there is no new segment, we need to download the newer m3u8.
            while (Segments.Any() && nextSegment == null)
            {
                // Wait at least the delay of a segment.
                await Task.Delay(Convert.ToInt32(segment.Length * 500), cancellationToken); // Some radio station has smaller segment. (In this case we need to have to be lower than the current segment length.)

                // Update the m3u8 file to get newer segment.
                await LoadCurrentM3U8File(cancellationToken);

                // Try to get the next segment.
                nextSegment = GetNextSegment(segment);
            }

            // Return the next segment.
            return nextSegment;
        }

        /// <summary>
        /// Return the next segment inside the m3u8
        /// Return null if there is no segment after this one.
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public M3U8Segment GetNextSegment(M3U8Segment segment)
        {
            // If we can use segment number.
            return Segments.SkipWhile(x => x.SegmentNumber <= segment.SegmentNumber).FirstOrDefault();
        }
        #endregion

        /// <summary>
        /// Process the data that has been downloaded into Data property.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task ProcessFile(CancellationToken cancellationToken)
        {
            // If loaded file is valid.
            if(!IsM3U8)
                throw new Exception(Resources.Resource.error_invalid_m3u8_format);

            // Update list of streams. (if needed - if both streams and segments list are empty)
            if(!Streams.Any() && !Segments.Any())
                Streams = _regexListStreams.Matches(Data).Select(x => new M3U8Stream(x)).ToList();

            // Update list of segments. (Take only the last 30 (MAXIMUM_SEGMENTS_IN_LIST) segments to avoid having issues with radio that send too many segments.)
            Segments.Clear();
            Segments.AddRange(_regexListSegments.Matches(Data).TakeLast(MAXIMUM_SEGMENTS_IN_LIST).Select(x => new M3U8Segment(x)));
            
            // Call garbage collector. (Force the garbage collector here, otherwise the process can take too much memory.)
            GC.Collect();

            // If we have a list of streams and no segments, we need to select and download the best streaming available.
            if (Streams.Any() && !Segments.Any())
                await LoadStreamFile(GetHighestQualityStream(), cancellationToken);
            
            // If there is no segment. (Process has failed!)
            if (!Segments.Any())
                throw new Exception(Resources.Resource.error_no_stream_found);
        }

        /// <summary>
        /// Return the highest quality streaming available if any.
        /// </summary>
        /// <returns></returns>
        public M3U8Stream GetHighestQualityStream()
        {
            // If there is no stream available, return null.
            // Maybe the m3u8 is already a stream, so we cannot choose another stream.
            if (!Streams.Any())
                return null;

            // If there is at least one stream available return the best one.
            return Streams.OrderByDescending(x => x.Bandwidth).FirstOrDefault();
        }
    }
}