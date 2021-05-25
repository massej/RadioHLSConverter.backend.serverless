/*
 * RadioController.cs
 * The radio controller allow incoming connection to the radio and make the proper codec / format conversion.
 * Date : 2021-05-24.
 * By : Jonathan Massé
 */


// Includes.
using System.Threading.Tasks;
using RadioHLSConverter.backend.serverless.Helpers;
using RadioHLSConverter.backend.serverless.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RadioHLSConverter.backend.serverless.Services;
using System;
using Microsoft.Extensions.Logging;


namespace RadioHLSConverter.backend.serverless.Radio
{
    /// <summary>
    /// RadioController
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class RadioController : ControllersBase
    {
        // Variables.
        private readonly AppSettings _appSettings;
        private readonly ILogger _logger;
        private readonly IHLSRadioConverterService _hlsRadioConverterService;


        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        public RadioController(IOptions<AppSettings> appSettings, ILoggerFactory loggerFactory, IHLSRadioConverterService hlsRadioConverterService)
        {
            _appSettings = appSettings.Value;
            _logger = loggerFactory.CreateLogger(nameof(RadioController));
            _hlsRadioConverterService = hlsRadioConverterService;
        }
        #endregion


        #region Get
        /// <summary>
        // GET api/v1.0/radio
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetRadio()
        {
            return await GetRadio(0);
        }


        /// <summary>
        // GET api/v1.0/radio/1
        /// </summary>
        [HttpGet("{radioId}")]
        public async Task<IActionResult> GetRadio(int radioId)
        {
            // If radio id is invalid.
            if (_appSettings.Radios.Length < (radioId + 1) || radioId < 0)
                return NotFound(new { message = Resources.Resource.error_radio_id_not_found });

            // Logs incoming connection.
            string remoteIpAddress = Request?.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            _logger.LogInformation(String.Format(Resources.Resource.radio_connected_client, remoteIpAddress, radioId, _appSettings.Radios[radioId].RadioName));


            /////////////////////////////////
            // Send the HTTP response header.
            /////////////////////////////////
            Response.Headers.Add("Connection", "keep-alive");
            Response.Headers.Add("Content-Type", _appSettings.Radios[radioId].HTTPContentType);
            Response.Headers.Add("Cache-Control", "no-cache");
            Response.Headers.Add("Pragma", "no-cache");
            Response.Headers.Add("Expires", "Mon, 26 Jul 1997 05:00:00 GMT");
            Response.Headers.Add("icy-name", _appSettings.Radios[radioId].RadioName);
            Response.Headers.Add("icy-description", _appSettings.Radios[radioId].RadioDescription);


            // Execute the HLS radio conversion.
            try
            {
                await _hlsRadioConverterService.ConvertHLSRadio(Response, radioId, Response.HttpContext.RequestAborted);
            }
            catch (TaskCanceledException)
            {
                _logger.LogInformation(String.Format(Resources.Resource.radio_disconnected_client, "TaskCanceledException", remoteIpAddress, radioId, _appSettings.Radios[radioId].RadioName));
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation(String.Format(Resources.Resource.radio_disconnected_client, "OperationCanceledException", remoteIpAddress, radioId, _appSettings.Radios[radioId].RadioName));
            }

            // On debug and running as unit test stop there.
#if DEBUG
            if (UnitTestHelper.IsInUnitTest())
                return Ok();
#endif

            // Return nothing. (Client is disconnected.)
            return new EmptyResult();
        }
        #endregion
    }
}