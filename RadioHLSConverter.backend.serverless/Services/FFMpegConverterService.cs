/*
 * FFMpegConverterService.cs
 * This class allow conversion by calling FFMpeg software.
 * Date : 2021-05-24.
 * By : Jonathan Massé
 */


// Includes.
using System.ComponentModel;
using Microsoft.Extensions.Options;
using RadioHLSConverter.backend.serverless.Settings;
using System.IO;
using System.Threading.Tasks;
using FFMpegCore;
using FFMpegCore.Pipes;
using Microsoft.Extensions.Logging;


namespace RadioHLSConverter.backend.serverless.Services
{
    public class FFMpegConverterService : IFFMpegConverterService
    {
        // Properties.
        private readonly AppSettings _appSettings;
        private readonly ILogger _logger;


        /// <summary>
        /// Constructor.
        /// </summary>
        public FFMpegConverterService(IOptions<AppSettings> appSettings, ILoggerFactory loggerFactory)
        {
            _appSettings = appSettings.Value;
            _logger = loggerFactory.CreateLogger(nameof(FFMpegConverterService));
        }


        /// <summary>
        /// ConvertSegmentData
        /// Convert a segment from a byte[] using ffmpeg utility.
        /// </summary>
        /// <param name="radioId"></param>
        /// <param name="segmentData"></param>
        /// <returns></returns>
        public async Task<byte[]> ConvertSegmentData(int radioId, byte[] segmentData)
        {
            return await ConvertSegmentStream(radioId, new MemoryStream(segmentData));
        }


        /// <summary>
        /// ConvertSegmentStream
        /// Convert a segment from a stream using ffmpeg utility.
        /// </summary>
        /// <param name="radioId"></param>
        /// <param name="segmentStream"></param>
        /// <returns></returns>
        public async Task<byte[]> ConvertSegmentStream(int radioId, Stream segmentStream)
        {
            try
            {
                // Prepare a memory stream for the output.
                await using (var outputStream = new MemoryStream())
                {
                    // Call ffmpeg utility and make the conversion.
                    // The output is going to a memory stream.
                    await FFMpegArguments
                        // Pipe to memory stream.
                        .FromPipeInput(new StreamPipeSource(segmentStream))
                        // Output set format / audio codec.
                        .OutputToPipe(new StreamPipeSink(outputStream), options => options
                            .ForceFormat(_appSettings.Radios[radioId].FFMPEGForceFormat)
                            .WithAudioCodec(_appSettings.Radios[radioId].FFMPEGConverterAudioCodec)
                            .WithCustomArgument(_appSettings.Radios[radioId].FFMPEGCustomArgument))
                        // Process async.
                        .ProcessAsynchronously();

                    // Return the converted segment byte[] data.
                    return outputStream.ToArray();
                }
            }
            catch (Win32Exception)
            {
                _logger.LogInformation(Resources.Resource.error_missing_ffmpeg);
                throw;
            }
        }
    }
}