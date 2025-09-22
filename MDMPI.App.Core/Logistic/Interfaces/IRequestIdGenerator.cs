namespace MDMPI.App.Core.Logistic.Interfaces
{
    public interface IRequestIdGenerator
    {
        Task<long> GenerateAsync();
    }
}
