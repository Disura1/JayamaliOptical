namespace JayamaliOptical.Web.Services
{
    public static class TimeHelper
    {
        private static readonly TimeZoneInfo SriLankaZone =
            TimeZoneInfo.FindSystemTimeZoneById(
                OperatingSystem.IsWindows()
                    ? "Sri Lanka Standard Time"
                    : "Asia/Colombo");

        /// <summary>Returns current date/time in Sri Lanka time (UTC+5:30)</summary>
        public static DateTime Now =>
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, SriLankaZone);

        public static DateTime Today => Now.Date;
    }
}