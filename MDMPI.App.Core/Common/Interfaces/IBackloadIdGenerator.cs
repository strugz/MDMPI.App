namespace MDMPI.App.Core.Common.Interfaces
{
    public interface IBackloadIdGenerator
    {
        Task<long> GenerateAsync();
    }
}
