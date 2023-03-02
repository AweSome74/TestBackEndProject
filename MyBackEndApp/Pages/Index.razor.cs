using Microsoft.AspNetCore.SignalR.Client;
using MyBackEndApp.RequestModel;
using MyBackEndApp.Utils;
using Newtonsoft.Json;

namespace MyBackEndApp.Pages
{
    public partial class Index
    {
        private HubConnection? hubConnection;
        private readonly List<string> messages = new();

        private string deviceIdInput = string.Empty;
        private string xCoordinateInput = string.Empty;
        private string yCoordinateInput = string.Empty;
        private string temperatureInput = string.Empty;
        protected override async Task OnInitializedAsync()
        {
            hubConnection = new HubConnectionBuilder().WithUrl(Navigation.ToAbsoluteUri("/events")).Build();
            hubConnection.On<string>("ResultMessage", (message) =>
            {
                var encodedMsg = message;
                messages.Add(encodedMsg);
                InvokeAsync(StateHasChanged);
            });
            await hubConnection.StartAsync();
        }

        private async Task RegisterDevice()
        {
            if (hubConnection is not null)
            {
                if (!string.IsNullOrEmpty(deviceIdInput))
                {
                    var device = new Device() { DeviceId = deviceIdInput };
                    var jsonData = JsonConvert.SerializeObject(device);
                    await hubConnection.SendAsync("RegisterDevice", jsonData);
                }
                else
                {
                    messages.Add("Device ID is empty");
                }
            }
        }

        private async Task SendGeoEvent()
        {
            if (hubConnection is not null)
            {
                if (!string.IsNullOrEmpty(xCoordinateInput) && !string.IsNullOrEmpty(yCoordinateInput))
                {
                    float? xCoordinate = ValueConverter.TryGetFloatValue(xCoordinateInput);
                    float? yCoordinate = ValueConverter.TryGetFloatValue(yCoordinateInput);

                    if (xCoordinate.HasValue && yCoordinate.HasValue)
                    {
                        var geoEvent = new GeoEvent() { DeviceId = "geo_device", CoordinateX = xCoordinate.Value, CoordinateY = yCoordinate.Value };
                        var jsonData = JsonConvert.SerializeObject(geoEvent);
                        await hubConnection.SendAsync("RegisterGeoEvent", jsonData);
                        messages.Add("Event was sended");
                    }
                    else
                    {
                        messages.Add("Coordinates are not float");
                    }
                }
                else
                {
                    messages.Add("Coordinates are empty");
                }
            }
        }

        private async Task SendTemperatureEvent()
        {
            if (hubConnection is not null)
            {
                if (!string.IsNullOrEmpty(temperatureInput))
                {
                    int? temperatute = ValueConverter.TryGetIntValue(temperatureInput);

                    if (temperatute.HasValue)
                    {
                        var temperatureEvent = new TemperatureEvent() { DeviceId = "temperature_device", Temperature = temperatute.Value };
                        var jsonData = JsonConvert.SerializeObject(temperatureEvent);
                        await hubConnection.SendAsync("RegisterTemperatureEvent", jsonData);
                        messages.Add("Event was sended");
                    }
                    else
                    {
                        messages.Add("Temperature is not int");
                    }
                }
                else
                {
                    messages.Add("Temperature is empty");
                }
            }
        }

        private async Task SendManyEvents()
        {
            int i = 0;
            var startTime = DateTime.Now;
            messages.Add("Sending events...");
            StateHasChanged();

            var tempEvent = new TemperatureEvent() { DeviceId = "temperature_device" };

            if (hubConnection is not null)
            {
                do
                {
                    tempEvent.Temperature = i;
                    await hubConnection.SendAsync("RegisterTemperatureEvent", JsonConvert.SerializeObject(tempEvent));
                    i++;
                }
                while (i < 25000);
            }

            messages.Add($"Sending events over. Events sended: {i}. Time elapsed: {(DateTime.Now - startTime).TotalMilliseconds}ms");
            StateHasChanged();
        }

        public bool IsConnected => hubConnection?.State == HubConnectionState.Connected;
        public async ValueTask DisposeAsync()
        {
            if (hubConnection is not null)
            {
                await hubConnection.DisposeAsync();
            }
        }
    }
}