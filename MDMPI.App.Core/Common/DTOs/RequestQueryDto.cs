namespace MDMPI.App.Core.Common.DTOs
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
    public enum RequestStatusFilter
    {
        NewRequest,
        GettingsSupliesReady,
        ItemPrepared,
        ItemPacked,
        ReadyForShipment,
        EndorsedToGuard,
        ForDelivery,
        InTransit,
        Delivered,
        Received,
        PickedUp,
        Cancelled,
        All
    }

    public class RequestQueryDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public RequestDateFilter DateFilter { get; set; } = RequestDateFilter.All;
        public RequestStatusFilter StatusFilter { get; set; } = RequestStatusFilter.All;
    }
}
