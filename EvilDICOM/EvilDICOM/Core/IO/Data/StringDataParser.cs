﻿using System;
using System.Globalization;
using EvilDICOM.Core.Element;
using EvilDICOM.Core.Enums;
using EvilDICOM.Core.Logging;
using DateTime = System.DateTime;

namespace EvilDICOM.Core.IO.Data
{
    public class StringDataParser
    {
        public static Age ParseAgeString(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return null;
            }
            var a = new Age();
            a.Number = int.Parse(data.Substring(1, 3));
            switch (data.Substring(4, 1))
            {
                case "D":
                    a.Units = Age.Unit.DAYS;
                    break;
                case "W":
                    a.Units = Age.Unit.WEEKS;
                    break;
                case "M":
                    a.Units = Age.Unit.MONTHS;
                    break;
                case "Y":
                    a.Units = Age.Unit.YEARS;
                    break;
            }
            return a;
        }

        public static DateTime? ParseDate(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return null;
            }
            try
            {
                return DateTime.ParseExact(data, "yyyyMMdd", null);
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static DateTime? ParseDateTime(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return null;
            }
            DateTime dateTime;

            string[] formats =
            {
                "yyyyMMddHHmmss.ffffff",
                "yyyyMMddHHmmss.fffff",
                "yyyyMMddHHmmss.ffff",
                "yyyyMMddHHmmss.fff",
                "yyyyMMddHHmmss.ff",
                "yyyyMMddHHmmss.f",
                "yyyyMMddHHmmss"
            };
            bool success = DateTime.TryParseExact(data, formats, CultureInfo.InvariantCulture, DateTimeStyles.None,
                out dateTime);

            if (!success)
            {
                EvilLogger.Instance.Log("Date {0} does not match any known format", LogPriority.ERROR, data);
                return null;
            }

            return dateTime;
        }


        public static double[] ParseDecimalString(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return new double[0];
            }
            string[] sNumbers = data.Replace(" ", "") //Remove padding
                .Split(new[] {'\\'});
            var numbers = new double[sNumbers.Length];
            for (int i = 0; i < sNumbers.Length; i++)
            {
                double.TryParse(sNumbers[i], NumberStyles.Any, CultureInfo.InvariantCulture, out numbers[i]);
            }
            return numbers;
        }


        public static int[] ParseIntegerString(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return new int[0];
            }
            string[] sNumbers = data.Replace(" ", "").Split(new[] {'\\'});
            var numbers = new int[sNumbers.Length];
            for (int i = 0; i < sNumbers.Length; i++)
            {
                int.TryParse(sNumbers[i], out numbers[i]);
            }
            return numbers;
        }

        public static DateTime? ParseTime(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return null;
            }

            DateTime time;

            string[] formats =
            {
                "HHmmss.ffffff",
                "HHmmss.fffff",
                "HHmmss.ffff",
                "HHmmss.fff",
                "HHmmss.ff",
                "HHmmss.f",
                "HHmmss"
            };

            bool success = DateTime.TryParseExact(data, formats, CultureInfo.InvariantCulture, DateTimeStyles.None,
                out time);

            if (!success)
            {
                EvilLogger.Instance.Log("Time {0} does not match any known format", LogPriority.ERROR, data);
            }
            return time;
        }

        /// <summary>
        /// Used in XML parsing
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static System.DateTime? GetNullableDate(string data)
        {
            System.DateTime? dt = string.IsNullOrEmpty(data) ? null : (System.DateTime?)System.DateTime.Parse(data);
            return dt;
        }
    }
}