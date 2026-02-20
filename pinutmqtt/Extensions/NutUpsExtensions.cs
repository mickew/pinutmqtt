using Pinutmqtt.Models;

namespace Pinutmqtt.Extensions;

internal static class NutUpsExtensions
{
    /// <summary>
    /// Returns a human-readable, comma-separated description of the UPS status.
    /// </summary>
    public static string ToFriendlyString(this NutUpsStatus status)
    {
        if (status == NutUpsStatus.Unknown) return "Unknown Status";
        if (status.HasFlag(NutUpsStatus.CommunicationLost)) return "Connection to UPS Lost";

        var messages = new List<string>();

        // Critical alerts first
        if (status.HasFlag(NutUpsStatus.Overload)) messages.Add("System Overloaded");
        if (status.HasFlag(NutUpsStatus.LowBattery)) messages.Add("Critical Battery Level");
        if (status.HasFlag(NutUpsStatus.ReplaceBattery)) messages.Add("Battery Replacement Required");

        // Power Source
        if (status.HasFlag(NutUpsStatus.OnBattery)) messages.Add("Running on Battery");
        else if (status.HasFlag(NutUpsStatus.Online)) messages.Add("Mains Power Online");

        // Operational States
        if (status.HasFlag(NutUpsStatus.Charging)) messages.Add("Charging");
        if (status.HasFlag(NutUpsStatus.Discharging)) messages.Add("Discharging");
        if (status.HasFlag(NutUpsStatus.Bypass)) messages.Add("Bypass Mode Active");
        if (status.HasFlag(NutUpsStatus.Calibration)) messages.Add("Performing Calibration");
        if (status.HasFlag(NutUpsStatus.Trim)) messages.Add("Trimming Voltage");
        if (status.HasFlag(NutUpsStatus.Boost)) messages.Add("Boosting Voltage");

        if (messages.Count == 0) return status.ToString();

        return string.Join(", ", messages);
    }

    /// <summary>
    /// Returns true if the UPS is in any state that indicates power failure.
    /// </summary>
    public static bool IsPowerFailure(this NutUpsStatus status)
    {
        return status.HasFlag(NutUpsStatus.OnBattery) ||
               (!status.HasFlag(NutUpsStatus.Online) && status != NutUpsStatus.Unknown);
    }
}
