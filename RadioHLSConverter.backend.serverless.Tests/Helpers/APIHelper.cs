/*
 * APIHelper.cs
 * Function helper to make the API easier/simpler.
 * Date : 2021-05-24.
 * By : Jonathan Massé
 */


// Includes.
using System.IO;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using Newtonsoft.Json;


namespace RadioHLSConverter.backend.serverless.Tests.Helpers
{
    public static class APIHelper
    {
        public static async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandlerAsync(string file)
        {
            return await FunctionDataHandlerAsync(await File.ReadAllTextAsync(file));
        }

        private static async Task<APIGatewayHttpApiV2ProxyResponse> FunctionDataHandlerAsync(string data)
        {
            var lambdaFunction = new LambdaEntryPoint();
            var request = JsonConvert.DeserializeObject<APIGatewayHttpApiV2ProxyRequest>(data);
            var context = new TestLambdaContext();
            var response = await lambdaFunction.FunctionHandlerAsync(request, context);
            return response;
        }
    }
}