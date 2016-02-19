using System;

namespace com.antlersoft.HostedTools.Utility
{
    public static class UnixTime
    {
        /// <summary>
        /// Start of UnixTime epoch; DateTimeKind is Utc
        /// </summary>
        public static DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static DateTime UtcFromUnixTime(long unixTime)
        {
            return Epoch.AddSeconds(unixTime);
        }

        /// <summary>
        /// Converts a dt to unix time.  IMPORTANT: If dt.DateTimeKind is not Utc, it is considered a local time.
        /// </summary>
        /// <param name="dt">Time to convert</param>
        /// <returns></returns>
        public static long UnixTimeFromDateTime(DateTime dt)
        {
            return (long)(dt.ToUniversalTime() - Epoch).TotalSeconds;
        }
    }
}
