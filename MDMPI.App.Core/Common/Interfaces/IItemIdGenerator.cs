namespace MDMPI.App.Core.Common.Interfaces
{
    public interface IItemIdGenerator
    {
        Task<long> GenerateAsync();
    }
}