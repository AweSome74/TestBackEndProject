namespace MyBackEndApp.Model
{
    public class EventReport
    {
        public string DeviceId { get; set; }
        public int TotalEventsCount { get; set; }
        public DateTime? LastEventDate { get; set; }
    }
}
