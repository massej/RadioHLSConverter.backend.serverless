/*
 * UnitTestHelper.cs
 * Detect if we are running as a unit test.
 * Date : 2021-05-24.
 * By : Jonathan Massé
 */


// Includes.
using System;
using System.Linq;


namespace RadioHLSConverter.backend.serverless.Helpers
{
    public static class UnitTestHelper
    {
        public static bool IsInUnitTest()
        {
            string assemblyName = "xunit";
            return AppDomain.CurrentDomain.GetAssemblies().Any(a => a.FullName.StartsWith(assemblyName));
        }
    }
}