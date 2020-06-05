using System;
using family_archive_server.Models;

namespace family_archive_server.Utilities
{
    public static class Format
    {
        public static string FindDateFromRange(DateTime startDate, DateTime endDate)
        {
            if (startDate == default)
            {
                return null;
            }

            if (endDate == default)
            {
                return startDate.ToString("d MMM yyyy");
            }

            if (endDate - startDate <= TimeSpan.FromDays(31))
            {
                return startDate.ToString("MMM yyyy");
            }

            return startDate.ToString("yyyy");
        }

        public static UpdateDate FindUpdateDate(DateTime startDate, DateTime endDate)
        {
            var returnDate = new UpdateDate();
            if (startDate == default)
            {
                return new UpdateDate
                {
                    Day = 0,
                    Month = 0,
                    Year = 0
                };
            }

            if (endDate == default)
            {
                returnDate.Day = startDate.Day;
                returnDate.Month = startDate.Month;
            }

            if (endDate - startDate <= TimeSpan.FromDays(31))
            {
                returnDate.Month = startDate.Month;
            }

            returnDate.Year = startDate.Year;
            return returnDate;
        }

        public static DateTime FindStartDateFromUpdateDate(UpdateDate updateDate)
        {
            if (updateDate == null || (updateDate.Year == 0 && updateDate.Month == 0 && updateDate.Day == 0))
            {
                return new DateTime();
            }

            if (ValidDay(updateDate))
            {
                return new DateTime(updateDate.Year, updateDate.Month, updateDate.Day);
            }

            if (updateDate.Year != 0 && ValidMonth(updateDate.Month))
            {

                return new DateTime(updateDate.Year, updateDate.Month, 1);
                
            }

            return new DateTime(updateDate.Year, 1, 1);
        }

        private static bool ValidMonth(int? month)
        {
            if (month >= 1 && month <= 12)
            {
                return true;
            }

            return false;
        }

        private static bool ValidDay(UpdateDate updateDate)
        {
            if (updateDate == null)
            {
                return false;
            }

            try
            {
                var dateTime = new DateTime(updateDate.Year, updateDate.Month, updateDate.Day);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static DateTime FindEndDateFromUpdateDate(UpdateDate updateDate)
        {
            if (updateDate == null || (updateDate.Year == 0 && updateDate.Month == 0 && updateDate.Day == 0))
            {
                return new DateTime();
            }

            if (ValidDay(updateDate))
            {
                return new DateTime();
            }
            
            if (updateDate.Year != 0 && ValidMonth(updateDate.Month))
            {

                var firstDayOfMonth = new DateTime(updateDate.Year, updateDate.Month, 1);
                return firstDayOfMonth.AddMonths(1).AddDays(-1);
            }

            return new DateTime(updateDate.Year, 12, 31);
        }
    }
}
