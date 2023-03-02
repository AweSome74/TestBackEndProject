namespace MyBackEndApp.RequestModel
{
    public class Device
    {
        public string DeviceId { get; set; }
    }

    public class GeoEvent
    { 
        public string DeviceId { get; set; }
        public float CoordinateX { get; set; }
        public float CoordinateY { get; set; }
    }

    public class TemperatureEvent
    {
        public string DeviceId { get; set; }
        public int Temperature { get; set; }   
    }
}
