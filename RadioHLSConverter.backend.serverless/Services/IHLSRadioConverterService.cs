/*
 * IHLSRadioConverterService.cs
 * This interface allow HLS Radio convertion to a radio stream to a specific format / codec specified under AppSettings.json
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
    /// IHLSRadioConverterService inteface.
    /// </summary>
    public interface IHLSRadioConverterService : IDisposable
    {
        /// <summary>
        /// ConvertHLSRadio
        /// Convert an HLS radio to HTTP audio stream.
        /// </summary>
        /// <param name="radioId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task ConvertHLSRadio(int radioId, CancellationToken cancellationToken);
    }
}