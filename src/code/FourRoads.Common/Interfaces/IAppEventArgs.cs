namespace FourRoads.Common.Interfaces
{
    public interface IAppEventArgs
    {
        string State { get; set; }
        AppEventType Type { get; set; }
    }
}