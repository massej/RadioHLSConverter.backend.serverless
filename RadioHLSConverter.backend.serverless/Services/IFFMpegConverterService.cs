/*
 * IFFMpegConverterService.cs
 * This inteface allow conversion by calling FFMpeg software.
 * Date : 2021-05-24.
 * By : Jonathan Massé
 */


// Includes.
using System;
using System.Threading;
using System.Threading.Tasks;


namespace RadioHLSConverter.backend.serverless.Services
{
    /// <summary>
    /// IFFMpegConverterService inteface.
    /// </summary>
    public interface IFFMpegConverterService : IDisposable
    {
        /// <summary>
        /// Init_FFMpeg
        /// Initialize ffmpeg and initialize connection to pipe stream.
        /// </summary>
        /// <param name="radioId"></param>
        /// <param name="callbackFunction"></param>
        /// <param name="cancellationToken"></param>
        public Task Init_FFMpeg(int radioId, Func<byte[], int, int, CancellationToken, Task> callbackFunction,  CancellationToken cancellationToken);


        /// <summary>
        /// UploadSegmentDataToFFMpeg
        /// Upload & convert a segment from a byte[] using ffmpeg utility.
        /// </summary>
        /// <param name="segmentData"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task UploadSegmentDataToFFMpeg(byte[] segmentData, CancellationToken cancellationToken);
    }
}