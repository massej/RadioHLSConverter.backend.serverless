/*
 * AppSettings.cs
 * AppSettings JSON configuration object.
 * Date : 2021-05-24.
 * By : Jonathan Massé
 */


namespace RadioHLSConverter.backend.serverless.Settings
{
    //////////////////////////////
    /// AppSettings
    //////////////////////////////
    public class AppSettings
    {
        // Variables.
        public int FFMpegPipeBufferInBytes { get; set; }
        public int BufferSizeInSeconds { get; set; }
        public RadioSettings[] Radios { get; set; }
    }
}