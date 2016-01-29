using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Appointments;
using Windows.Storage;

namespace ICSReader.Model
{
    class ICSReader
    {
        public static Appointment Read(IList<string> icsFileLines)
        {
            Appointment appointment = null;
            if (icsFileLines[0].Trim() != "BEGIN:VCALENDAR")
                return null;
            int lineCount = 0;
            foreach (string s in icsFileLines)
            {
                var strim = s.Trim();
                if (strim == "BEGIN:VEVENT")
                    appointment = ReadVEvent(icsFileLines, ref lineCount);
                else if (strim == "END:VCALENDAR")
                    break;
                else if (strim == "BEGIN:VALARM" )
                {
                    if (appointment != null)
                        appointment.Reminder = new TimeSpan(0, 15, 0);
                }
                lineCount++;
            }
            return appointment;
        }
        static Appointment ReadVEvent(IList<string> lines, ref int startIndex)
        {
            if (lines[startIndex].Trim() != "BEGIN:VEVENT")
                return null;
            Appointment appoinment = new Appointment();
            for (; startIndex < lines.Count; startIndex++)
            {
                string s = lines[startIndex].Trim();
                if (s.StartsWith("ORGANIZER"))
                {
                    appoinment.Organizer = GetOrganizer(s);
                }
                else if (s.StartsWith("DTSTART"))
                {
                    appoinment.StartTime = GetDateTimeOffset(s);
                }
                else if (s.StartsWith("DTEND"))
                {
                    var endTime = GetDateTimeOffset(s);
                    var duration = endTime - appoinment.StartTime;
                    appoinment.Duration = duration;
                }
                else if (s.StartsWith("LOCATION"))
                {
                    appoinment.Location = GetLocation(s);
                }
                else if (s.StartsWith("TRANSP"))
                {

                }
                else if (s.StartsWith("SEQUENCE"))
                {

                }
                else if (s.StartsWith("UID"))
                {

                }
                else if (s.StartsWith("DTSTAMP"))
                {

                }
                else if (s.StartsWith("DESCRIPTION"))
                {
                    var details = GetDescription(s);
                    appoinment.Details = ConvertLFToCRLF(details);
                    appoinment.DetailsKind = AppointmentDetailsKind.PlainText;
                }
                else if (s.StartsWith("SUMMARY"))
                {
                    appoinment.Subject = GetSummary(s);
                }
                else if (s.StartsWith("PRIORITY"))
                {

                }
                else if (s.StartsWith("CLASS"))
                {
                    appoinment.Sensitivity = GetClass(s);
                }
                else if (s.StartsWith("END:VEVENT"))
                {
                    break;
                }
                else if ( s.StartsWith("BEGIN") && ! s.StartsWith("BEGIN:VEVENT"))
                {
                    break;
                }
                else
                {
                    /** Unhandled content */
                }
            }
            return appoinment;
        }
       
        private static AppointmentSensitivity GetClass(string s)
        {
            if (!s.StartsWith("CLASS"))
                return AppointmentSensitivity.Private;
            int idx = s.IndexOf(':');
            if (idx > 0)
            {
                var sensitivity = s.Substring(idx + 1);
                if (sensitivity == "PUBLIC")
                    return AppointmentSensitivity.Public;
                else
                    return AppointmentSensitivity.Private;
            }
            else
                return AppointmentSensitivity.Private; ;
        }

        private static string GetSummary(string s)
        {
            if (!s.StartsWith("SUMMARY"))
                return "";
            int idx = s.IndexOf(':');
            if (idx > 0)
                return s.Substring(idx + 1);
            else
                return "";
        }

        private static string GetDescription(string s)
        {
            if (!s.StartsWith("DESCRIPTION"))
                return "";
            int idx = s.IndexOf(':');
            if (idx > 0)
                return s.Substring(idx + 1);
            else
                return "";
        }

        static VTimeZone ReadVTimeZone(IList<string> lines, ref int startIndex)
        {
            return null;
        }

        /// <summary>
        /// ORGANIZER;CN="hundsun 9":MAILTO:webex11@hundsun.com
        /// </summary>
        /// <param name="organizer"></param>
        /// <returns></returns>
        static AppointmentOrganizer GetOrganizer(string organizer)
        {
            AppointmentOrganizer org = new AppointmentOrganizer();
            if (!organizer.StartsWith("ORGANIZER"))
                return null;
            if (organizer.Contains("CN="))
            {
                var idx = organizer.IndexOf("CN=");  //10
                var endIdx = organizer.IndexOf(':', idx); //24
                var cn = organizer.Substring(idx + 4, endIdx - 2 - idx - 3);
                org.DisplayName = cn;
                var addr = organizer.Substring(endIdx + 1);
                if (addr.StartsWith("MAILTO"))
                {
                    addr = addr.Substring(7);
                }
                org.Address = addr;
            }
            else
            {
                org.Address = organizer.Substring("ORGANIZER;".Length);
            }
            return org;
        }

        static DateTimeOffset GetDateTimeOffset(string datetime)
        {
            var Now = DateTime.Now;
            var LocalTimeSpanOffset = TimeZoneInfo.Local.GetUtcOffset(Now);
            var DefaultDateTimeOffset = new DateTimeOffset(Now.Year, Now.Month, Now.Day, Now.Hour, Now.Minute, Now.Second, LocalTimeSpanOffset);
            if (!datetime.StartsWith("DT"))
            {
                return DefaultDateTimeOffset;
            }
            int idx = datetime.IndexOf(':');
            if (idx <= 0)
                return DefaultDateTimeOffset;
            try {
                var date = int.Parse(datetime.Substring(idx + 1, 8));
                var time = datetime.Substring(idx + 1 + 8 + 1, 6);
                int year = date / 10000;
                int mon = (date % 10000) / 100;
                int day = date % 100;
                int hour = int.Parse(time.Substring(0,2));
                int min = int.Parse(time.Substring(2, 2));
                int sec = int.Parse(time.Substring(4, 2));
                var dateTimeOffset = new DateTimeOffset(year, mon, day, hour, min, sec, LocalTimeSpanOffset);
                return dateTimeOffset;
            }
            catch
            {
                return DefaultDateTimeOffset;
            }
        }

        static string GetLocation( string location )
        {
            if (!location.StartsWith("LOCATION"))
                return "";
            int idx = location.IndexOf(':');
            if (idx > 0)
                return location.Substring(idx + 1);
            else
                return "";
        }
        
        static string ConvertLFToCRLF(string s )
        {
            string o = "";
            string[] splitstring = { "\\n", "\\r\\n" };
            var list = s.Split(splitstring, StringSplitOptions.None);
            foreach( var l in list )
            {
                o += (l + "\r\n");
            }
            return o;
        }
    }

    class VTimeZone
    {
        public string TZID { get; set; }
        public string TZNAME { get; set; }
        public string TZOffsetFrom { get; set; }
        public string TZOffsetTO { get; set; }
        public TimeZoneInfo TimeZone { get; set; }
    }
    class VAlarm
    {
        public string Trigger { get; set; }
        public string Action { get; set; }
        public string Description { get; set; }
    }
}
