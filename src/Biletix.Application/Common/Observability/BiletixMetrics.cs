using System.Diagnostics.Metrics;

namespace Biletix.Application.Common.Observability;

/// <summary>
/// Biletix'e ozel Prometheus metriklerini ureten ortak meter.
/// </summary>
public static class BiletixMetrics
{
    public static readonly Meter Meter = new("Biletix.API", "1.0.0");

    public static readonly Counter<long> BookingsCreated =
        Meter.CreateCounter<long>(
            "biletix_bookings_created_total",
            description: "Total number of bookings created");

    public static readonly Counter<long> BookingsConfirmed =
        Meter.CreateCounter<long>(
            "biletix_bookings_confirmed_total",
            description: "Total number of bookings confirmed");

    public static readonly Counter<long> BookingsExpired =
        Meter.CreateCounter<long>(
            "biletix_bookings_expired_total",
            description: "Total number of bookings expired");

    public static readonly Counter<long> PaymentsProcessed =
        Meter.CreateCounter<long>(
            "biletix_payments_processed_total",
            unit: "TRY",
            description: "Total payments processed");

    public static readonly Counter<long> TicketsScanned =
        Meter.CreateCounter<long>(
            "biletix_tickets_scanned_total",
            description: "Total ticket scans at door");

    public static readonly Counter<long> SearchRequests =
        Meter.CreateCounter<long>(
            "biletix_search_requests_total",
            description: "Total search requests");

    public static readonly Histogram<double> BookingAmount =
        Meter.CreateHistogram<double>(
            "biletix_booking_amount",
            unit: "TRY",
            description: "Distribution of booking amounts");

    public static readonly Histogram<double> QueueWaitTime =
        Meter.CreateHistogram<double>(
            "biletix_queue_wait_seconds",
            unit: "s",
            description: "Time users wait in queue");

    public static void RegisterObservableGauges(IServiceProvider sp)
    {
        InitializeCounters();

        Meter.CreateObservableGauge(
            "biletix_queue_length",
            () => new[] { new Measurement<long>(0) },
            description: "Current number of users in waiting queues");
    }

    private static void InitializeCounters()
    {
        BookingsCreated.Add(0);
        BookingsConfirmed.Add(0);
        BookingsExpired.Add(0);
        PaymentsProcessed.Add(0);
        TicketsScanned.Add(0);
        SearchRequests.Add(0);
    }
}
