using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using FoxLife.View.UC;

namespace FoxLife.Models
{
    internal static class DateTimeExtenstion
    {
        /// <summary>
        /// modes: <br/>
        /// 1 - today,yesterday, day on week, DMYT(day month year time)<br/>
        /// 2 - DMYT(day month year time)<br/>
        /// 3 - DMY(day month year)
        /// </summary>
        /// <returns>String value of date depending in mode</returns>
        public static string ToString(this DateTime date, int mode)
        {
            var result = new StringBuilder("");
            
            var currentTimeZone = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Hours*60;
            date = date.AddMinutes(currentTimeZone);

            switch (mode)
            {
                case 1:
                    
                    var temp = DateTime.UtcNow.AddMinutes(currentTimeZone);
                    var daysBetween = (int)(temp - date).TotalDays;

                    if (daysBetween >= 7)
                    {
                        result.Append($"{date.Day} {GetMonth(date.Month)} ");

                        if (date.Year != temp.Year)
                            result.Append($"{date.Year} ");

                        result.Append($"{date.ToString("HH:mm")}");
                        break;
                    }

                    switch (daysBetween)
                    {
                        case 0:
                            result.Append($"{Application.Current.Resources["Today"]} {date.ToString("HH:mm")}");
                            break;
                        case 1:
                            result.Append($"{Application.Current.Resources["Yesterday"]} {date.ToString("HH:mm")}");
                            break;
                        default:
                            switch (date.DayOfWeek)
                            {
                                case DayOfWeek.Monday:
                                    result.Append($"{Application.Current.Resources["OnMonday"]} {date.ToString("HH:mm")}");
                                    break;
                                case DayOfWeek.Tuesday:
                                    result.Append($"{Application.Current.Resources["OnTuesday"]} {date.ToString("HH:mm")}");
                                    break;
                                case DayOfWeek.Wednesday:
                                    result.Append($"{Application.Current.Resources["OnWednesday"]} {date.ToString("HH:mm")}");
                                    break;
                                case DayOfWeek.Thursday:
                                    result.Append($"{Application.Current.Resources["OnThursday"]} {date.ToString("HH:mm")}");
                                    break;
                                case DayOfWeek.Friday:
                                    result.Append($"{Application.Current.Resources["OnFriday"]} {date.ToString("HH:mm")}");
                                    break;
                                case DayOfWeek.Saturday:
                                    result.Append($"{Application.Current.Resources["OnSaturday"]} {date.ToString("HH:mm")}");
                                    break;
                                case DayOfWeek.Sunday:
                                    result.Append($"{Application.Current.Resources["OnSunday"]} {date.ToString("HH:mm")}");
                                    break;
                            }
                            break;
                    }
                    break;
                case 2:
                    result.Append($"{date.Day} {GetMonth(date.Month)} {date.Year} {date.ToString("HH:mm")}");
                    break;
                case 3:
                    result.Append($"{date.Day} {GetMonth(date.Month)} {date.Year} ");
                    break;
                default:
                    result.Append($"{date.Day} {GetMonth(date.Month)} {date.Year} ");
                    break;

            }

            return result.ToString();
        }

        private static string GetMonth(int month)
        {
            string result = "";

            switch (month)
            {
                case 1:
                    return Application.Current.Resources["January"].ToString();
                case 2:
                    return Application.Current.Resources["February"].ToString();
                case 3:
                    return Application.Current.Resources["March"].ToString();
                case 4:
                    return Application.Current.Resources["April"].ToString();
                case 5:
                    return Application.Current.Resources["May"].ToString();
                case 6:
                    return Application.Current.Resources["June"].ToString();
                case 7:
                    return Application.Current.Resources["July"].ToString();
                case 8:
                    return Application.Current.Resources["August"].ToString();
                case 9:
                    return Application.Current.Resources["September"].ToString();
                case 10:
                    return Application.Current.Resources["October"].ToString();
                case 11:
                    return Application.Current.Resources["November"].ToString();
                case 12:
                    return Application.Current.Resources["December"].ToString();
                default:
                    return Application.Current.Resources["ErrorTime"].ToString();
            }
        }

        /// <summary>
        /// adding year, month, day, hour, min to date
        /// </summary>
        /// <param name="dateStr">
        /// <br/>dates like: Nvalue <br/>
        /// (N)-number <br/>
        /// (value)-string date <br/>
        /// string dates in descending, available: <br/>
        /// y/year m/month d/day h/hour mm/min <br/>
        /// example: 1year 3m 1h<br/>
        /// </param>
        /// <returns>returns false if string is not match regular expression</returns>
        public static bool TryAddString(ref this DateTime date, string dateStr, bool isOnlyCheck = false)
        {
            var reg = new Regex(
                @"^"+
                @"\s*((?<year>[\d]+)\s*((y)|(year)))?" +
                @"\s*((?<month>[\d]+)\s*((m)|(month)))?" +
                @"\s*((?<day>[\d]+)\s*((d)|(day)))?" +
                @"\s*((?<hour>[\d]+)\s*((h)|(hour)))?" +
                @"\s*((?<min>[\d]+)\s*((mm)|(min)))?" +
                @"\s*$");

            var regNumber = new Regex(@"^[\d]$");
            var matches = reg.Matches(dateStr);

            if (matches.Count == 0)
                return false;

            if (isOnlyCheck)
                return true;

            var m = matches[0];

            int year = m.Groups["year"].Value==""? 0 : int.Parse(m.Groups["year"].Value),
                month = m.Groups["month"].Value == "" ? 0 : int.Parse(m.Groups["month"].Value),
                day = m.Groups["day"].Value == "" ? 0 : int.Parse(m.Groups["day"].Value),
                hour = m.Groups["hour"].Value == "" ? 0 : int.Parse(m.Groups["hour"].Value),
                min = m.Groups["min"].Value == "" ? 0 : int.Parse(m.Groups["min"].Value);

            date = date.AddYears(year);
            date = date.AddMonths(month);
            date = date.AddDays(day);
            date = date.AddHours(hour);
            date = date.AddMinutes(min);

            return true;
        }
    }
}
