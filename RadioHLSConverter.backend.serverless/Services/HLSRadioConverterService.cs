/*
 * HLSRadioConverterService.cs
 * This class allow HLS Radio convertion to a radio stream to a specific format / codec specified under AppSettings.json
 * Date : 2021-05-24.
 * By : Jonathan Massé
 */


// Includes.
using System;
using System.Threading;
using Microsoft.Extensions.Options;
using RadioHLSConverter.backend.serverless.Settings;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http.Features;
using RadioHLSConverter.backend.serverless.Helpers;


namespace RadioHLSConverter.backend.serverless.Services
{
    public class HLSRadioConverterService : IHLSRadioConverterService
    {
        // Application variables.
        private readonly AppSettings _appSettings; // Application configuration.
        private readonly ILogger _logger; // Application logs.
        private readonly IHttpContextAccessor _httpContextAccessor; // HTTP context accessor.

        // m3u8 file service & ffmpeg converter.
        private readonly IM3U8FileService _m3u8FileService; // Read m3u8 file and segment from HTTP.
        private readonly IFFMpegConverterService _ffMpegConverterService; // Allow audio conversion using FFMpeg software.


        /// <summary>
        /// Constructor.
        /// </summary>
        public HLSRadioConverterService(IOptions<AppSettings> appSettings, ILoggerFactory loggerFactory, IHttpContextAccessor httpContextAccessor, IM3U8FileService m3u8FileService, IFFMpegConverterService ffMpegConverterService)
        {
            _appSettings = appSettings.Value;
            _logger = loggerFactory.CreateLogger(nameof(HLSRadioConverterService));
            _httpContextAccessor = httpContextAccessor;
            _m3u8FileService = m3u8FileService;
            _ffMpegConverterService = ffMpegConverterService;
        }


        ///////////////////////////////////////////////////
        // Destructor.
        ///////////////////////////////////////////////////
        public void Dispose()
        {
            _m3u8FileService?.Dispose();
            _ffMpegConverterService?.Dispose();
        }


        /// <summary>
        /// ConvertHLSRadio
        /// Convert an HLS radio to HTTP audio stream.
        /// </summary>
        /// <param name="radioId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task ConvertHLSRadio(int radioId, CancellationToken cancellationToken)
        {
            // Initialize ffmpeg converter pipe stream.
            await _ffMpegConverterService.Init_FFMpeg(radioId, UploadSegmentDataToHTTP, cancellationToken);

            // Download m3u8 file. (It can be a m3u8 with a list of streams of a list of segments.)
            // It will automatically download the best quality segments list if this is a list of streams.
            await _m3u8FileService.LoadM3U8File(_appSettings.Radios[radioId].RadioSourceURL, cancellationToken);

            // Download / Get next segment.
            var segment = _m3u8FileService.GetFirstSegment(_appSettings.BufferSizeInSeconds);

            // If there is no first segment.
            if (segment == null)
                throw new Exception(Resources.Resource.error_no_first_segment_check_buffer);

            ///////////////////////////
            // Uploading new segments.
            ///////////////////////////
            while (true)
            {
                // Download segment.
                var segmentData = await _m3u8FileService.DownloadSegment(segment, cancellationToken);

                // Upload segment to ffmpeg pipe stream.
                // ffmpeg output pipe stream will automatically call back UploadSegmentDataToHTTP with the converted segment.
                await _ffMpegConverterService.UploadSegmentDataToFFMpeg(segmentData, cancellationToken);

                // Get next segment.
                segment = await _m3u8FileService.GetNextSegmentAndUpdateM3U8(segment, cancellationToken);

                // On debug and running as unit test stop there.
#if DEBUG
                if (UnitTestHelper.IsInUnitTest())
                    return;
                #endif
            }
        }


        /// <summary>
        /// UploadSegmentDataToHTTP
        /// Upload a segment data to the HTTP client.
        /// </summary>
        /// <param name="httpResponse"></param>
        /// <param name="segmentData"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task UploadSegmentDataToHTTP(byte[] segmentData, int offset, int count, CancellationToken cancellationToken)
        {
            try
            {
                var responseBufferingFeature = _httpContextAccessor?.HttpContext?.Features.Get<IHttpResponseBodyFeature>();
                await responseBufferingFeature?.Stream?.WriteAsync(segmentData, offset, count, cancellationToken);
                await responseBufferingFeature?.Stream?.FlushAsync(cancellationToken);
            }
            // If the task is cancelled then ignore the error, the cancelled task is already caught into the RadioController.
            catch (OperationCanceledException)
            { }
            // Log the exception.
            catch (Exception exception)
            {
                _logger.LogInformation(exception.Message + Environment.NewLine + exception.StackTrace);
            }
        }
    }
}