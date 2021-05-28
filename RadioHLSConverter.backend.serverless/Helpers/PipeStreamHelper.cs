/*
 * PipeStreamHelper.cs
 * This pipe stream helper will return the pipe stream path name based on the current OS.
 * Date : 2021-05-28.
 */


// Includes.
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;


namespace RadioHLSConverter.backend.serverless.Helpers
{
    public static class PipeStreamHelper
    {
        /// <summary>
        /// GetPipeFullPath
        /// Return the valid pipe full path based on the current OS.
        /// Maybe in the future this function will become public (See https://github.com/dotnet/runtime/issues/28979).
        /// </summary>
        /// <param name="pipeStream"></param>
        /// <param name="pipeName"></param>
        public static string GetPipeFullPath(this PipeStream pipeStream, string pipeName)
        {
            // .NET Core on Windows uses Windows named pipes. Use \\.\pipe\{ name} (e.g. \\.\pipe\my - video - pipe) to identify a pipe to FFmpeg on Windows.
            // .NET Core on Linux does not use Linux named pipes(FIFOs). Unix domain sockets are used instead, so use unix://tmp/CoreFxPipe_{name} (e.g. unix://tmp/CoreFxPipe_my-video-pipe) to identify a pipe to FFmpeg on Linux.

            // The value for both serverPath and clientPath would be:
            // Windows  =   \\.\pipe\MyPipeName
            // Unix     =   unix://tmp/CoreFxPipe_MyPipeName or /tmp/CoreFxPipe_MyPipeName

            // If we currently run on Windows, so we need to use Windows pipe.
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return Path.GetFullPath(@"\\.\pipe\" + pipeName);
            }
            // Else we need to use Unix socket.
            else
            {
                return @"unix:/" + Path.Combine(Path.GetTempPath(), "CoreFxPipe_") + pipeName;
            }
        }
    }
}