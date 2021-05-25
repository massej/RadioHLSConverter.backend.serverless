/*
 * FFMpegConverterService.cs
 * This class allow conversion by calling FFMpeg software.
 * Date : 2021-05-24.
 * By : Jonathan Massé
 */


// Includes.
using Microsoft.Extensions.Options;
using RadioHLSConverter.backend.serverless.Settings;
using System.IO;
using System.Threading.Tasks;
using FFMpegCore;
using FFMpegCore.Pipes;


namespace RadioHLSConverter.backend.serverless.Services
{
    public class FFMpegConverterService : IFFMpegConverterService
    {
        // Properties.
        private readonly AppSettings _appSettings;


        /// <summary>
        /// Constructor.
        /// </summary>
        public FFMpegConverterService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
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
            // Prepare a memory stream for the output.
            await using (var outputStream = new MemoryStream())
            {
                // Call ffmpeg utility and make the conversion.
                // The output is going to a memory stream.
                await FFMpegArguments
                    // Pipe to memory stream.
                    .FromPipeInput(new StreamPipeSource(new MemoryStream(segmentData)))
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


        /// <summary>
        /// ConvertSegmentStream
        /// Convert a segment from a stream using ffmpeg utility.
        /// </summary>
        /// <param name="radioId"></param>
        /// <param name="segmentStream"></param>
        /// <returns></returns>
        public async Task<byte[]> ConvertSegmentStream(int radioId, Stream segmentStream)
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
    }
}