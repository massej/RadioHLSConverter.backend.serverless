/*
 * RadioController.cs
 * The radio controller allow incoming connection to the radio and make the proper codec / format conversion.
 * Date : 2021-05-24.
 * By : Jonathan Massé
 */


// Includes.
using System.Threading.Tasks;
using RadioHLSConverter.backend.serverless.Tests.Helpers;
using Xunit;


namespace RadioHLSConverter.backend.serverless.Tests.Radio
{
    /// <summary>
    /// RadioControllerTest
    /// </summary>
    public class RadioControllerTest
    {
        [Fact]
        public async Task BadGetRadioID()
        {
            // Get response.
            var response = await APIHelper.FunctionHandlerAsync("./Controllers/Radio/SampleRequests/RadioController-Get-Radio-Bad-ID.json");

            // Check response code.
            Assert.Equal(404, response.StatusCode);

            // Check body.
            Assert.False(string.IsNullOrEmpty(response.Body));
        }

        [Fact]
        public async Task BadGetRadioID2()
        {
            // Get response.
            var response = await APIHelper.FunctionHandlerAsync("./Controllers/Radio/SampleRequests/RadioController-Get-Radio-Bad-ID-2.json");

            // Check response code.
            Assert.Equal(404, response.StatusCode);

            // Check body.
            Assert.False(string.IsNullOrEmpty(response.Body));
        }

#if DEBUG
        [Fact] //  - too long to execute can only be run on debug.
#endif
        public async Task ValidGetRadio()
        {
            // Get response.
            var response = await APIHelper.FunctionHandlerAsync("./Controllers/Radio/SampleRequests/RadioController-Get-Radio.json");

            // Check response code.
            Assert.Equal(200, response.StatusCode);

            // Check body.
            Assert.False(string.IsNullOrEmpty(response.Body));
        }

#if DEBUG
        [Fact] //  - too long to execute can only be run on debug.
#endif
        public async Task ValidGetRadioID()
        {
            // Get response.
            var response = await APIHelper.FunctionHandlerAsync("./Controllers/Radio/SampleRequests/RadioController-Get-Radio-ID.json");

            // Check response code.
            Assert.Equal(200, response.StatusCode);

            // Check body.
            Assert.False(string.IsNullOrEmpty(response.Body));
        }
    }
}