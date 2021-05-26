/*
 * IM3U8FileService.cs
 * IM3U8FileService inteface allow the application to download & read a m3u8 that contains the HLS data.
 * Date : 2021-05-24.
 * By : Jonathan Massé
 */


// Includes.
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using RadioHLSConverter.backend.serverless.Model.M3U8;


namespace RadioHLSConverter.backend.serverless.Services
{
    public interface IM3U8FileService
    {
        // Properties.
        public bool IsM3U8 { get; } // If the last downloaded m3u8 is valid.
        public int Version { get; } // The last m3u8 file version.
        public string Data { get; } // The last m3u8 file data.
        public string URL { get; } // The last m3u8 file URL.
        public string URLPath { get; } // The last m3u8 file URL path (without the filename).


        /// <summary>
        /// M3U8 list of streams.
        /// </summary>
        public IEnumerable<M3U8Stream> Streams { get; }

        /// <summary>
        /// M3U8 list of segments.
        /// </summary>
        public IEnumerable<M3U8Segment> Segments { get; }

        #region Load M3U8 file
        /// <summary>
        /// Load the M3U8 file from an url.
        /// Once the download is completed it will automatically load the M3U8 file.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task LoadM3U8File(string url, CancellationToken cancellationToken);

        /// <summary>
        /// Load the M3U8 stream file from an url.
        /// Once the download is completed it will automatically load the M3U8 file.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task LoadStreamFile(M3U8Stream stream, CancellationToken cancellationToken);
        #endregion

        #region Download segment
        /// <summary>
        /// Download an m3u8 segment and return the segment data.
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<byte[]> DownloadSegment(M3U8Segment segment, CancellationToken cancellationToken);

        /// <summary>
        /// Download an m3u8 segment and return the segment stream.
        /// </summary>
        /// <param name="radioId"></param>
        /// <param name="segment"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<byte[]> DownloadConvertedSegment(int radioId, M3U8Segment segment, CancellationToken cancellationToken);
        #endregion

        #region Segment position
        /// <summary>
        /// Return the first segment to be read based on the required buffer size in the appsettings.json
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public M3U8Segment GetFirstSegment(decimal bufferSize = 30m);

        /// <summary>
        /// GetNextSegmentAndUpdateM3U8
        /// Get the next segment and it will automatically re-download the newer M3U8 if needed to get the new list of segments.
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<M3U8Segment> GetNextSegmentAndUpdateM3U8(M3U8Segment segment, CancellationToken cancellationToken);

        /// <summary>
        /// Return the next segment inside the m3u8
        /// Return null if there is no segment after this one.
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public M3U8Segment GetNextSegment(M3U8Segment segment);
        #endregion

        /// <summary>
        /// Return the highest quality streaming available if any.
        /// </summary>
        /// <returns></returns>
        public M3U8Stream GetHighestQualityStream();
    }
}