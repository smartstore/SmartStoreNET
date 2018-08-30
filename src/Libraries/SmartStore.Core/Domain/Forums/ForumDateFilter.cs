namespace SmartStore.Core.Domain.Forums
{
    public enum ForumDateFilter
    {
        LastVisit = 0,
        Yesterday = 1,
        LastWeek = 7,
        LastTwoWeeks = 14,
        LastMonth = 30,
        LastThreeMonths = 92,
        LastSixMonths = 183,
        LastYear = 365
    }
}
