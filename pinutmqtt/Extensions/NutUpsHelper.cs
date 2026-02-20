using Pinutmqtt.Models;

namespace Pinutmqtt.Extensions;

internal static class NutUpsHelper
{
    // specific mapping of NUT string codes to our Enum
    private static readonly Dictionary<string, NutUpsStatus> StatusMap =
        new Dictionary<string, NutUpsStatus>(StringComparer.OrdinalIgnoreCase)
    {
            { "OL", NutUpsStatus.Online },
            { "OB", NutUpsStatus.OnBattery },
            { "LB", NutUpsStatus.LowBattery },
            { "HB", NutUpsStatus.HighBattery },
            { "RB", NutUpsStatus.ReplaceBattery },
            { "CHRG", NutUpsStatus.Charging },
            { "DISCHRG", NutUpsStatus.Discharging },
            { "BYPASS", NutUpsStatus.Bypass },
            { "CAL", NutUpsStatus.Calibration },
            { "OFF", NutUpsStatus.Offline },
            { "OVER", NutUpsStatus.Overload },
            { "TRIM", NutUpsStatus.Trim },
            { "BOOST", NutUpsStatus.Boost },
            { "FSD", NutUpsStatus.Unknown }
    };

    /// <summary>
    /// Parses a raw status string from NUT (e.g., "OL CHRG") into a typed Enum.
    /// </summary>
    /// <param name="rawStatus">The string output from 'upsc ups.status'</param>
    /// <returns>A combined bitmask of current statuses.</returns>
    public static NutUpsStatus ParseStatusString(string rawStatus)
    {
        if (string.IsNullOrWhiteSpace(rawStatus))
            return NutUpsStatus.Unknown;

        var currentStatus = NutUpsStatus.Unknown;

        // Split by space, remove empty entries to handle double spaces safely
        var codes = rawStatus.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var code in codes)
        {
            if (StatusMap.TryGetValue(code.Trim(), out var mappedStatus))
            {
                if (currentStatus == NutUpsStatus.Unknown)
                    currentStatus = mappedStatus;
                else
                    currentStatus |= mappedStatus;
            }
        }

        return currentStatus;
    }
}
