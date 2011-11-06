using System;

namespace Samba.Domain.Foundation
{
    public class DocumentDate
    {
        private DateTime _dateTime;

        public DocumentDate()
            : this(DateTime.Now)
        { }

        public DocumentDate(int day, int month, int year)
            : this(new DateTime(year, month, day))
        { }


        public DocumentDate(DateTime dateTime)
        {
            _dateTime = dateTime;
        }

        public static DocumentDate Now { get { return new DocumentDate(); } }

        public override bool Equals(object obj)
        {
            var comparingDate = obj as DocumentDate;
            if (comparingDate != null)
            {
                return comparingDate._dateTime.Day == _dateTime.Day &&
                     comparingDate._dateTime.Month == _dateTime.Month &&
                     comparingDate._dateTime.Year == _dateTime.Year &&
                     comparingDate._dateTime.Hour == _dateTime.Hour &&
                     comparingDate._dateTime.Minute == _dateTime.Minute;
            }
            return base.Equals(obj);
        }

        public static bool operator ==(DocumentDate a, DocumentDate b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            // Return true if the fields match:
            return a._dateTime.Day == b._dateTime.Day &&
                a._dateTime.Month == b._dateTime.Month &&
                a._dateTime.Year == b._dateTime.Year &&
                a._dateTime.Hour == b._dateTime.Hour &&
                a._dateTime.Minute == b._dateTime.Minute;
        }

        public static bool operator !=(DocumentDate a, DocumentDate b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return _dateTime.Day + _dateTime.Month + _dateTime.Year + _dateTime.Hour + _dateTime.Minute;
        }

        public long Ticks { get { return _dateTime.Ticks; } }

        public int Day { get { return _dateTime.Day; } }

        public int Month { get { return _dateTime.Month; } }

        public int Year { get { return _dateTime.Year; } }

        public int Hour { get { return _dateTime.Hour; } }

        public int Minute { get { return _dateTime.Minute; } }
        
        public int Second { get { return _dateTime.Second; } }
        
        public string Date
        {
            get { return _dateTime.ToString(); }
            set
            {
                DateTime.TryParse(value, out _dateTime);
            }
        }

        public string ShortDateDisplay
        {
            get { return _dateTime.ToShortDateString(); }
        }

        public string ShortTimeDisplay
        {
            get { return _dateTime.ToShortTimeString(); }
        }

        public double DifferenceMinutesFromNow()
        {
            return new TimeSpan(DateTime.Now.Ticks - _dateTime.Ticks).TotalMinutes;
        }
    }
}
