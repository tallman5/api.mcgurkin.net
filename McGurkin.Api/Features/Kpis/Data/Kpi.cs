using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace McGurkin.Api.Features.Kpis.Data;

public class Kpi
{
    [Key]
    public Guid Id { get; set; }

    public required string KeyName { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public required decimal KeyValue { get; set; }

    public required string DeviceName { get; set; }

    public required DateTimeOffset Timestamp { get; set; }
}
