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
using RadioHLSConverter.backend.serverless.Helpers;


namespace RadioHLSConverter.backend.serverless.Services
{
    public class HLSRadioConverterService : IHLSRadioConverterService
    {
        // Properties.
        private readonly AppSettings _appSettings;
        private readonly IM3U8FileService _m3u8FileService;


        /// <summary>
        /// Constructor.
        /// </summary>
        public HLSRadioConverterService(IOptions<AppSettings> appSettings, IM3U8FileService m3u8FileService)
        {
            _appSettings = appSettings.Value;
            _m3u8FileService = m3u8FileService;
        }


        /// <summary>
        /// ConvertHLSRadio
        /// Convert an HLS radio to HTTP audio stream.
        /// </summary>
        /// <param name="httpResponse"></param>
        /// <param name="radioId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task ConvertHLSRadio(HttpResponse httpResponse, int radioId, CancellationToken cancellationToken)
        {
            // Download m3u8 file. (It can be a m3u8 with a list of streams of a list of segments.)
            // It will automatically download the best quality segments list if this is a list of streams.
            await _m3u8FileService.LoadM3U8File(_appSettings.Radios[radioId].RadioSourceURL, cancellationToken);
            
            // Download / Get next segment.
            var segment = _m3u8FileService.GetFirstSegment(_appSettings.BufferSizeInSeconds);

            // If there is no first segment.
            if(segment == null)
                throw new Exception(Resources.Resource.error_no_first_segment_check_buffer);

            ///////////////////////////
            // Uploading new segments.
            ///////////////////////////
            while (true)
            {
                // Download segment.
                var convertedSegmentData = await _m3u8FileService.DownloadConvertedSegment(radioId, segment, cancellationToken);

                // Upload segment.
                await UploadSegmentData(httpResponse, convertedSegmentData, cancellationToken);

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
        /// UploadSegmentData
        /// Upload a segment data to the HTTP client.
        /// </summary>
        /// <param name="httpResponse"></param>
        /// <param name="segmentData"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task UploadSegmentData(HttpResponse httpResponse, byte[] segmentData, CancellationToken cancellationToken)
        {
            await httpResponse.Body.WriteAsync(segmentData, 0, segmentData.Length, cancellationToken);
            await httpResponse.Body.FlushAsync(cancellationToken);
        }
    }
}