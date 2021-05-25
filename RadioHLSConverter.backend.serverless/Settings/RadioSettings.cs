/*
 * RadioSettings.cs
 * RadioSettings JSON configuration object.
 * Date : 2021-05-24.
 * By : Jonathan Massé
 */


namespace RadioHLSConverter.backend.serverless.Settings
{
    //////////////////////////////
    /// RadioSettings
    //////////////////////////////
    public class RadioSettings
    {
        // Variables.
        public string RadioName { get; set; } = "";
        public string RadioDescription { get; set; } = "";
        public string RadioSourceURL { get; set; } = "";
        public string HTTPContentType { get; set; } = "audio/aac";
        public string FFMPEGConverterAudioCodec { get; set; } = "copy";
        public string FFMPEGForceFormat { get; set; } = "adts";
        public string FFMPEGCustomArgument { get; set; } = "";
    }
}