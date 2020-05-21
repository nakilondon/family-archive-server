using System;

namespace family_archive_server.Utilities
{
    public static class Format
    {
        public static string FindDateFromRange(DateTime startDate, DateTime endDateTime)
        {
            if (startDate == default)
            {
                return null;
            }

            if (endDateTime == default)
            {
                return startDate.ToString("d MMM yyyy");
            }

            if (endDateTime - startDate <= TimeSpan.FromDays(31))
            {
                return startDate.ToString("MMM yyyy");
            }

            return startDate.ToString("yyyy");
        }
    }
}
