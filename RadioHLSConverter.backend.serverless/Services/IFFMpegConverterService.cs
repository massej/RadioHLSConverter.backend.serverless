/*
 * IFFMpegConverterService.cs
 * This inteface allow conversion by calling FFMpeg software.
 * Date : 2021-05-24.
 * By : Jonathan Massé
 */


// Includes.
using System.IO;
using System.Threading.Tasks;


namespace RadioHLSConverter.backend.serverless.Services
{
    /// <summary>
    /// IFFMpegConverterService inteface.
    /// </summary>
    public interface IFFMpegConverterService
    {
        /// <summary>
        /// ConvertSegmentData
        /// Convert a segment from a byte[] using ffmpeg utility.
        /// </summary>
        /// <param name="radioId"></param>
        /// <param name="segmentData"></param>
        /// <returns></returns>
        public Task<byte[]> ConvertSegmentData(int radioId, byte[] segmentData);


        /// <summary>
        /// ConvertSegmentStream
        /// Convert a segment from a stream using ffmpeg utility.
        /// </summary>
        /// <param name="radioId"></param>
        /// <param name="segmentStream"></param>
        /// <returns></returns>
        public Task<byte[]> ConvertSegmentStream(int radioId, Stream segmentStream);
    }
}