namespace Pinutmqtt.Models;

/// <summary>
/// Represents the status flags for a UPS monitored by NUT (Network UPS Tools).
/// Uses bitwise flags to support multiple simultaneous states.
/// </summary>
[Flags]
internal enum NutUpsStatus
{
    /// <summary>
    /// State cannot be determined or initialized.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// (OL) The UPS is running on line power (mains).
    /// </summary>
    Online = 1 << 0, // 1

    /// <summary>
    /// (OB) The UPS is running on battery power.
    /// </summary>
    OnBattery = 1 << 1, // 2

    /// <summary>
    /// (LB) The battery level is critical.
    /// </summary>
    LowBattery = 1 << 2, // 4

    /// <summary>
    /// (HB) High Battery - usually implies fully charged.
    /// </summary>
    HighBattery = 1 << 3, // 8

    /// <summary>
    /// (RB) The battery needs to be replaced.
    /// </summary>
    ReplaceBattery = 1 << 4, // 16

    /// <summary>
    /// (CHRG) The battery is currently charging.
    /// </summary>
    Charging = 1 << 5, // 32

    /// <summary>
    /// (DISCHRG) The battery is discharging (usually accompanies OnBattery).
    /// </summary>
    Discharging = 1 << 6, // 64

    /// <summary>
    /// (BYPASS) The UPS is in bypass mode (mains directly to load).
    /// </summary>
    Bypass = 1 << 7, // 128

    /// <summary>
    /// (CAL) The UPS is performing a runtime calibration/test.
    /// </summary>
    Calibration = 1 << 8, // 256

    /// <summary>
    /// (OFF) The UPS is administratively off or the load is unpowered.
    /// </summary>
    Offline = 1 << 9, // 512

    /// <summary>
    /// (OVER) The UPS is overloaded.
    /// </summary>
    Overload = 1 << 10, // 1024

    /// <summary>
    /// (TRIM) The UPS is trimming incoming voltage (stepping down high voltage).
    /// </summary>
    Trim = 1 << 11, // 2048

    /// <summary>
    /// (BOOST) The UPS is boosting incoming voltage (stepping up low voltage).
    /// </summary>
    Boost = 1 << 12, // 4096

    /// <summary>
    /// Communication with the UPS device has been lost (Software level status).
    /// </summary>
    CommunicationLost = 1 << 13 // 8192
}
