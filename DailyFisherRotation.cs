using System;

namespace Oberyn.AnglerAssociate.Services
{
    // pulled from wiki, reset at midnight UTC.
    public static class DailyFisherRotation
    {
        private static readonly DateTime AnchorDate = new DateTime(2026, 7, 27); // 27/07/2026 Shiverpeaks

        private static readonly string[] RotationOrder =
        {
            "Daily Shiverpeaks Fisher",
            "Daily Desert Fisher",
            "Daily End of Dragons Fisher",
            "Daily Heart of Maguuma Fisher",
            "Daily Ascalon Fisher",
            "Daily Orr Fisher",
            "Daily Kryta Fisher",
            "Daily Maguuma Jungle Fisher"
        };

        public static string GetToday(DateTime? utcNow = null)
        {
            DateTime today = (utcNow ?? DateTime.UtcNow).Date;
            int daysSinceAnchor = (today - AnchorDate).Days;

            // avoiding a negative index if daysSinceAnchor goes negative (e.g. bad system clock)
            int index = ((daysSinceAnchor % RotationOrder.Length) + RotationOrder.Length)
                         % RotationOrder.Length;

            return RotationOrder[index];
        }
    }
}
