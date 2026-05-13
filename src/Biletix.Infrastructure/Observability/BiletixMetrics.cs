using System.Diagnostics.Metrics;

namespace Biletix.Infrastructure.Observability;

/// <summary>
/// Infrastructure katmani icin Application meter'ina erisim saglayan uyumluluk wrapper'i.
/// Handler'lar dependency direction nedeniyle Application tarafindaki meter'i dogrudan kullanir.
/// </summary>
public static class BiletixMetrics
{
    public static Meter Meter => Application.Common.Observability.BiletixMetrics.Meter;
}
