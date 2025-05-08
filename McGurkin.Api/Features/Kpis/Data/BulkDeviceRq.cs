namespace McGurkin.Api.Features.Kpis.Data;

public class BulkDeviceRq
{
    public required BulkKpi[] Kpis { get; set; } = [];
}

public class BulkKpi
{
    public required string KeyName { get; set; }
    public required decimal KeyValue { get; set; }
    public long Epoch { get; set; }
}
