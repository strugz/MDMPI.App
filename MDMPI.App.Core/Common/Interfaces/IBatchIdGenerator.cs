namespace MDMPI.App.Core.Common.Interfaces
{
    public interface IBatchIdGenerator
    {
        Task<long> GenerateAsync();
    }
}