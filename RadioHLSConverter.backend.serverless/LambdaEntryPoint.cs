/*
 * LambdaEntryPoint.cs
 * Lambda entry point used by Amazon Lambda.
 * Date : 2021-05-24.
 * By : Jonathan Massé
 */


// Includes.
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;


namespace RadioHLSConverter.backend.serverless
{
    /// <summary>
    /// This class extends from APIGatewayProxyFunction which contains the method FunctionHandlerAsync which is the 
    /// actual Lambda function entry point. The Lambda handler field should be set to
    /// 
    /// RadioHLSConverter.backend.serverless::RadioHLSConverter.backend.serverless.LambdaEntryPoint::FunctionHandlerAsync
    /// </summary>
    public class LambdaEntryPoint :
        // When using an ELB's Application Load Balancer as the event source change 
        // the base class to Amazon.Lambda.AspNetCoreServer.ApplicationLoadBalancerFunction
        Amazon.Lambda.AspNetCoreServer.APIGatewayProxyFunction
    {
        /// <summary>
        /// The builder has configuration, logging and Amazon API Gateway already configured. The startup class
        /// needs to be configured in this method using the UseStartup<>() method.
        /// </summary>
        /// <param name="builder"></param>
        protected override void Init(IHostBuilder builder)
        {
            builder.UseServiceProviderFactory(new AutofacServiceProviderFactory());
        }

        protected override void Init(IWebHostBuilder builder)
        {
            builder
                .UseStartup<Startup>()
                .UseKestrel(o => { o.Limits.KeepAliveTimeout = TimeSpan.MaxValue; });
        }
    }
}