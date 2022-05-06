/*
 * M3U8Segment.cs
 * Allow the application to read a m3u8 that contains a list of segments.
 * Date : 2021-05-24.
 * By : Jonathan Massé
 */


// Includes.
using System.Text.RegularExpressions;
using RadioHLSConverter.backend.serverless.Helpers;


namespace RadioHLSConverter.backend.serverless.Model.M3U8
{
    public class M3U8Segment : ModelBase
    {
        // Regex.
        private static Regex _regexSegmentNumber = new Regex("([0-9]+)\\.", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant); // segment_3351399.ts

        // Properties.
        public decimal Length { get; private set; } // Segment length in seconds.
        public string SegmentFilename { get; private set; } // Segment filename.
        public int SegmentNumber  // Segment number.
        {
            get
            {
                // If a segment number is found.
                if (_regexSegmentNumber.IsMatch(SegmentFilename))
                {
                    int segmentNumber;
                    string value = _regexSegmentNumber.Match(SegmentFilename).Groups[1].Value;

                    // If segment number can be converted to int.
                    if (int.TryParse(value, out segmentNumber))
                        return segmentNumber;
                }

                // No segment number found.
                return -1;
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="match"></param>
        public M3U8Segment(Match match)
        {
            // Load properties.
            Length = decimal.Parse(match.Groups[1].Value);
            SegmentFilename = match.Groups[2].Value;
        }
    }
}