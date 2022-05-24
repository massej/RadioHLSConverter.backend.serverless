/*
 * M3U8Stream.cs
 * Allow the application to read a m3u8 that contains a list of streams.
 * Date : 2021-05-24.
 * By : Jonathan Massé
 */


// Includes.
using System.Globalization;
using System.Text.RegularExpressions;
using RadioHLSConverter.backend.serverless.Helpers;


namespace RadioHLSConverter.backend.serverless.Model.M3U8
{
    public class M3U8Stream : ModelBase
    {
        // Regex to decode the stream infos.
        private static Regex _regexBandwidth = new Regex("BANDWIDTH=([0-9]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant); // Ex : BANDWIDTH=96000
        private static Regex _regexCodecs = new Regex("CODECS=\"(.*)\"", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant); // Ex : CODECS="mp4a.40.2"


        // Properties.
        public int Bandwidth { get; private set; } // Stream bandwidth. (Quality)
        public string Codecs { get; private set; } // Stream codec.
        public string StreamFilename { get; private set; } // Stream filename.


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="match"></param>
        public M3U8Stream(Match match)
        {
            // Load match.
            var matchBandwidth = _regexBandwidth.Match(match.Groups[1].Value);
            var matchCodecs = _regexCodecs.Match(match.Groups[1].Value);

            // Load properties.
            Bandwidth = matchBandwidth.Success ? int.Parse(matchBandwidth.Groups[1].Value, NumberStyles.Number, CultureInfo.InvariantCulture) : 0;
            Codecs = matchCodecs.Success ? matchCodecs.Groups[1].Value : "";
            StreamFilename = match.Groups[2].Value;
        }
    }
}