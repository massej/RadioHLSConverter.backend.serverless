/*
 * M3U8Segment.cs
 * Allow the application to read a m3u8 that contains a list of segments.
 * Date : 2021-05-24.
 * By : Jonathan Massé
 */


// Includes.
using System.Globalization;
using System.Text.RegularExpressions;
using RadioHLSConverter.backend.serverless.Helpers;


namespace RadioHLSConverter.backend.serverless.Model.M3U8
{
    public class M3U8Segment : ModelBase
    {
        // Properties.
        public decimal Length { get; private set; } // Segment length in seconds.
        public string SegmentFilename { get; private set; } // Segment filename.

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="match"></param>
        public M3U8Segment(Match match)
        {
            // Load properties.
            Length = decimal.Parse(match.Groups[1].Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
            SegmentFilename = match.Groups[2].Value;
        }
    }
}