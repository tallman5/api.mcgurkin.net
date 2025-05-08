using McGurkin.Api.Features.Kpis.Data;
using Microsoft.EntityFrameworkCore;

namespace McGurkin.Api.Features.Kpis;

public interface IKpiService
{
    Task AddEpochsAsync(EpochsRq epochsRq);
    Task DeleteKpiAsync(Guid kpiId);
    Task<Kpi[]> GetKpisAsync();
    Task<Kpi[]> GetKpisByDeviceAsync(string deviceName);
    Task<Kpi[]> GetKpisByDeviceKeyAsync(string deviceName, string keyName);
    Task<RainSensorHistory> GetRainSensorHistoryAsync();
    Task UpsertBulkDeviceKpisAsync(string deviceName, BulkDeviceRq rq);
    Task UpsertKpiAsync(Kpi kpi);
}

public class KpiService(KpiDbContext kpiDbContext) : IKpiService
{
    private readonly KpiDbContext _kpiDbContext = kpiDbContext;

    public async Task AddEpochsAsync(EpochsRq epochsRq)
    {
        foreach (var epoch in epochsRq.Epochs)
        {
            var kpi = new Kpi
            {
                Id = Guid.NewGuid(),
                DeviceName = epochsRq.DeviceName,
                KeyName = epochsRq.KeyName,
                KeyValue = epochsRq.KeyValue,
                Timestamp = DateTimeOffset.FromUnixTimeSeconds(epoch)
            };
            _kpiDbContext.Kpis.Add(kpi);
        }
        await _kpiDbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task DeleteKpiAsync(Guid kpiId)
    {
        var kpi = _kpiDbContext.Kpis
            .FirstOrDefault(x => x.Id == kpiId);
        if (kpi != null)
        {
            _kpiDbContext.Kpis.Remove(kpi);
            await _kpiDbContext.SaveChangesAsync();
        }
    }

    public async Task<Kpi[]> GetKpisAsync()
    {
        return await _kpiDbContext.Kpis
            .OrderBy(k => k.Timestamp)
            .AsNoTracking()
            .ToArrayAsync();
    }

    public async Task<Kpi[]> GetKpisByDeviceAsync(string deviceName)
    {
        return await _kpiDbContext.Kpis
            .Where(x => x.DeviceName.ToLower() == deviceName.ToLower())
            .OrderBy(x => x.Timestamp)
            .AsNoTracking()
            .ToArrayAsync();
    }

    public async Task<Kpi[]> GetKpisByDeviceKeyAsync(string deviceName, string keyName)
    {
        return await _kpiDbContext.Kpis
            .Where(x => x.DeviceName.ToLower() == deviceName.ToLower()
                && x.KeyName.ToLower() == keyName.ToLower())
            .OrderBy(x => x.Timestamp)
            .AsNoTracking()
            .ToArrayAsync();
    }

    public async Task<RainSensorHistory> GetRainSensorHistoryAsync()
    {
        var kpis = await _kpiDbContext.Kpis
            .Where(x => x.DeviceName.ToLower() == "mcg-rain-01")
            .AsNoTracking()
            .ToArrayAsync();

        var returnValue = new RainSensorHistory
        {
            BattHistory = [.. kpis
                .Where(x => x.KeyName.ToLower() == "batt")
                .OrderBy(x => x.Timestamp)],
            TipHistory = [.. kpis
                .Where(x => x.KeyName.ToLower() == "tip")
                .OrderBy(x => x.Timestamp)],
            VoltHistory = [.. kpis
                .Where(x => x.KeyName.ToLower() == "volt")
                .OrderBy(x => x.Timestamp)],
        };

        return returnValue;
    }

    public async Task UpsertBulkDeviceKpisAsync(string deviceName, BulkDeviceRq rq)
    {
        foreach (var bulkKpi in rq.Kpis)
        {
            var kpi = new Kpi
            {
                Id = Guid.NewGuid(),
                DeviceName = deviceName,
                KeyName = bulkKpi.KeyName,
                KeyValue = bulkKpi.KeyValue,
                Timestamp = DateTimeOffset.FromUnixTimeSeconds(bulkKpi.Epoch)
            };
            _kpiDbContext.Kpis.Add(kpi);
        }
        await _kpiDbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task UpsertKpiAsync(Kpi kpi)
    {
        if (kpi.Id == Guid.Empty) kpi.Id = Guid.NewGuid();
        kpi.Timestamp = DateTimeOffset.Now;

        var existingItem = _kpiDbContext.Kpis
            .AsNoTracking()
            .FirstOrDefault(x => x.Id == kpi.Id);

        if (existingItem == null)
        {
            _kpiDbContext.Kpis.Add(kpi);
        }
        else
        {
            _kpiDbContext.Kpis.Update(kpi);
        }

        await _kpiDbContext.SaveChangesAsync().ConfigureAwait(false);
    }
}