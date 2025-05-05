using System.ComponentModel.DataAnnotations;

namespace McGurkin.Api.Features.Kpis.Data;

public class EpochsRq
{
    public required string DeviceName { get; set; }
    public required string KeyName { get; set; }
    public required decimal KeyValue { get; set; }
    public long[] Epochs { get; set; } = [];
}


public class KpiEpoch
{
    [Key]
    public Guid Id { get; set; }
    public required string KeyName { get; set; }
    public required decimal KeyValue { get; set; }
    public required string DeviceName { get; set; }
    public required long Timestamp { get; set; }

    public Kpi ToKpi()
    {
        return new Kpi
        {
            Id = Id,
            KeyName = KeyName,
            KeyValue = KeyValue,
            DeviceName = DeviceName,
            Timestamp = DateTimeOffset.FromUnixTimeSeconds(Timestamp)
        };
    }
}