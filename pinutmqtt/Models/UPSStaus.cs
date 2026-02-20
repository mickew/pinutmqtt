namespace Pinutmqtt.Models;

internal class UPSStaus
{
    public string Status { get; set; } = string.Empty;

    public int LoadPercent { get; set; }

    public int LoadWatts { get; set; }

    public TimeSpan Runtime { get; set; }

    public int BatteryPercent { get; set; }

    public double BatteryVoltage { get; set; }

    public double InputVoltage { get; set; }

    public double OutputVoltage { get; set; }
}
