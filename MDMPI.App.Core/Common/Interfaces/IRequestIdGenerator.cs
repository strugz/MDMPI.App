namespace MDMPI.App.Core.Common.Interfaces
{
    public interface IRequestIdGenerator
    {
        Task<long> GenerateAsync();
    }
}
