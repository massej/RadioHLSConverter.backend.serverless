/*
 * FFMpegConverterService.cs
 * This class allow conversion by calling FFMpeg software.
 * Date : 2021-05-24.
 * By : Jonathan Massé
 */


// Includes.
using System;
using System.ComponentModel;
using Microsoft.Extensions.Options;
using RadioHLSConverter.backend.serverless.Settings;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using FFMpegCore;
using Microsoft.Extensions.Logging;
using RadioHLSConverter.backend.serverless.Helpers;


namespace RadioHLSConverter.backend.serverless.Services
{
    public class FFMpegConverterService : IFFMpegConverterService
    {
        // Application variables.
        private readonly AppSettings _appSettings; // Application settings.
        private readonly ILogger _logger; // Application logs.

        // Pipe stream to FFMpeg.
        private readonly string _toFFMpegStreamName; // to ffmpeg pipe stream name.
        private readonly NamedPipeServerStream _toFFMpegStream; // to ffmpeg pipe stream.
        private readonly string _fromFFMpegStreamName; // from ffmpeg pipe stream name.
        private readonly NamedPipeServerStream _fromFFMpegStream; // from ffmpeg pipe stream.

        // Callback function.
        private Func<byte[], int, int, CancellationToken, Task> _callbackFunction; // Callback fonction when receive data from the ffmpeg pipe stream.
        private CancellationToken _cancellationToken; // Cancellation token.


        /// <summary>
        /// Constructor.
        /// </summary>
        public FFMpegConverterService(IOptions<AppSettings> appSettings, ILoggerFactory loggerFactory)
        {
            _appSettings = appSettings.Value;
            _logger = loggerFactory.CreateLogger(nameof(FFMpegConverterService));

            // Prepare to ffmpeg pipe stream.
            _toFFMpegStreamName = Guid.NewGuid().ToString();
            _toFFMpegStream = new NamedPipeServerStream(_toFFMpegStreamName, PipeDirection.Out, 1, PipeTransmissionMode.Byte, PipeOptions.WriteThrough, 0, _appSettings.FFMpegPipeBufferInBytes);

            // Prepare from ffmpeg pipe stream.
            _fromFFMpegStreamName = Guid.NewGuid().ToString();
            _fromFFMpegStream = new NamedPipeServerStream(_fromFFMpegStreamName, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.WriteThrough, _appSettings.FFMpegPipeBufferInBytes, 0);

            // Open pipe stream for connection.
            _toFFMpegStream.BeginWaitForConnection(null, _toFFMpegStream);
            _fromFFMpegStream.BeginWaitForConnection(PipeStreamConnectCallBack, _fromFFMpegStream);
        }


        ///////////////////////////////////////////////////
        // Destructor.
        ///////////////////////////////////////////////////
        public void Dispose()
        {
            _fromFFMpegStream?.Close();
            _fromFFMpegStream?.Dispose();
            _toFFMpegStream?.Close();
            _toFFMpegStream?.Dispose();
            _callbackFunction = null;
        }


        /// <summary>
        /// Init_FFMpeg
        /// Initialize ffmpeg and initialize connection to pipe stream.
        /// </summary>
        /// <param name="radioId"></param>
        /// <param name="callbackFunction"></param>
        /// <param name="cancellationToken"></param>
        public void Init_FFMpeg(int radioId, Func<byte[], int, int, CancellationToken, Task> callbackFunction, CancellationToken cancellationToken)
        {
            _callbackFunction = callbackFunction;
            _cancellationToken = cancellationToken;

            try
            {
                // Call ffmpeg utility and make the conversion.
                // The output is going to a memory stream.
                // This function must run in background!
                // _ allow to ignore the compiler await warning.
                _ = FFMpegArguments
                    // Pipe to memory stream.
                    .FromFileInput(_toFFMpegStream.GetPipeFullPath(_toFFMpegStreamName), false)
                    // Output set format / audio codec.
                    .OutputToFile(_fromFFMpegStream.GetPipeFullPath(_fromFFMpegStreamName), true, options => options
                        .OverwriteExisting()
                        .ForceFormat(_appSettings.Radios[radioId].FFMPEGForceFormat)
                        .WithAudioCodec(_appSettings.Radios[radioId].FFMPEGConverterAudioCodec)
                        .WithCustomArgument(_appSettings.Radios[radioId].FFMPEGCustomArgument))
                    // Process async.
                    .ProcessAsynchronously();
            }
            catch
            {
                _logger.LogInformation(Resources.Resource.error_throw_ffmpeg);
                throw;
            }
        }


        /// <summary>
        /// UploadSegmentDataToFFMpeg
        /// Upload & convert a segment from a byte[] using ffmpeg utility.
        /// </summary>
        /// <param name="segmentData"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task UploadSegmentDataToFFMpeg(byte[] segmentData, CancellationToken cancellationToken)
        {
            // If pipe is not connected!
            if (!(_toFFMpegStream != null && _toFFMpegStream.IsConnected))
                throw new Exception(Resources.Resource.error_upload_pipe_not_connected);

            await _toFFMpegStream.WriteAsync(segmentData, 0, segmentData.Length, cancellationToken);
        }


        /// <summary>
        /// PipeStreamConnectCallBack
        /// Called when pipe stream receive a connection (i.e. ffmpeg app try to connect)
        /// </summary>
        /// <param name="result"></param>
        private void PipeStreamConnectCallBack(IAsyncResult result)
        {
            try
            {
                _fromFFMpegStream.EndWaitForConnection(result);

                // Start reading buffer.
                byte[] buffer = new byte[_appSettings.FFMpegPipeBufferInBytes];
                _fromFFMpegStream.BeginRead(buffer, 0, buffer.Length, PipeStreamReadCallBack, buffer);
            }
            catch (Exception exception)
            {
                _logger.LogInformation(exception.Message + Environment.NewLine + exception.StackTrace);
            }
        }


        /// <summary>
        /// PipeStreamReadCallBack
        /// Called when ffmpeg send data / when pipe stream receive data back from it.
        /// </summary>
        /// <param name="result"></param>
        private void PipeStreamReadCallBack(IAsyncResult result)
        {
            try
            {
                // Loading data.
                byte[] buffer = result.AsyncState as byte[];

                // Finish asynchronous read into buffer.
                int bufferSize = _fromFFMpegStream.EndRead(result);

                // Call back function with the buffer received from ffmpeg.
                _callbackFunction?.Invoke(buffer, 0, bufferSize, _cancellationToken);

                // If pipe is still open.
                if (_fromFFMpegStream != null && _fromFFMpegStream.IsConnected)
                {
                    buffer = new byte[_appSettings.FFMpegPipeBufferInBytes];
                    _fromFFMpegStream.BeginRead(buffer, 0, buffer.Length, PipeStreamReadCallBack, buffer);
                }
                else
                    Dispose();
            }
            catch (Exception exception)
            {
                _logger.LogInformation(exception.Message + Environment.NewLine + exception.StackTrace);
            }
        }
    }
}