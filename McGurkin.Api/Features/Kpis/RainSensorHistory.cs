using McGurkin.Api.Features.Kpis.Data;

namespace McGurkin.Api.Features.Kpis;

public class RainSensorHistory
{
    public Kpi[] BattHistory { get; set; } = [];
    public Kpi[] TipHistory { get; set; } = [];
    public Kpi[] VoltHistory { get; set; } = [];
}
