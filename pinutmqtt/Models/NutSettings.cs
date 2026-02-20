using System.ComponentModel.DataAnnotations;

namespace Pinutmqtt.Models;

internal class NutSettings
{
    public const string Section = "NUT";

    [Required]
    public string? Host { get; set; }

    [Required]
    public int Port { get; set; } = 3493;

    [Required]
    public string? UPSName { get; set; }

    public double RealPowerNominelWatts { get; set; } = double.NaN;
}
