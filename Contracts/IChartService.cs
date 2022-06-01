namespace Contracts;

public interface IChartService
{
    public Task<Dictionary<String, int>> GetChartData();

}