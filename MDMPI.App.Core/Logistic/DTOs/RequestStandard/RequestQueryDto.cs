namespace MDMPI.App.Core.Logistic.DTOs.RequestStandard
{
    public enum RequestDateFilter
    {
        Today,
        Yesterday,
        Tomorrow,
        FiveDaysAgo,
        ThirtyDaysAgo,
        All
    }

    public class RequestQueryDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; }
        public bool SortDesc { get; set; } = false;
        public RequestDateFilter DateFilter { get; set; } = RequestDateFilter.All;
    }
}
