using System;
using System.Text.RegularExpressions;

namespace Buttercup.Control.Model
{
    public class ValueConversionHelper
    {
		#region Fields (4) 

        private static string _fourDigitExtractorPattern = @"(\d{2,4})";
        private static readonly Regex _timeSpanExtractor;
        private static string _timeSpanExtractorPattern = @"(?:{0}:)?{0}:{0}\.{1}";
        private static string _twoDigitExtractorPattern = @"(\d{2})";

		#endregion Fields 

		#region Constructors (1) 

        static ValueConversionHelper()
        {
            _timeSpanExtractor = new Regex(String.Format(_timeSpanExtractorPattern, _twoDigitExtractorPattern, _fourDigitExtractorPattern),
                                           RegexOptions.None);
        }

		#endregion Constructors 

		#region Methods (1) 

		// Public Methods (1) 

        /// <summary>
        /// Gets a new TimeSpan for the value represented by the given input string.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>The new TimeSpan.</returns>
        public static TimeSpan GetConvertedTimeSpan(string input)
        {
            int hours = 0;
            int minutes = 0;
            int seconds = 0;
            int milliseconds = 0;

            Match currentMatch = _timeSpanExtractor.Match(input);

            Int32.TryParse(currentMatch.Groups[1].Value, out hours);
            Int32.TryParse(currentMatch.Groups[2].Value, out minutes);
            Int32.TryParse(currentMatch.Groups[3].Value, out seconds);

            //Convert fraction of a second (variable length) into milliseconds
            string rawTicks = "." + currentMatch.Groups[4].Value;
            double secondsFraction = 0.0;
            Double.TryParse(rawTicks, out secondsFraction);
            milliseconds = (int)(secondsFraction * 1000.0);

            return new TimeSpan(0, hours, minutes, seconds, milliseconds);
        }

		#endregion Methods 
    }
}