using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.Sqlite;
using MyBackEndApp.Model;
using MyBackEndApp.RequestModel;
using MyBackEndApp.Utils;
using Newtonsoft.Json;
using System.Globalization;

namespace MyBackEndApp.Hubs
{
    public class ConnectionHub : Hub
    {
        private const string registerDeviceQuery = "INSERT INTO device (device_id) VALUES ('{0}');";
        private const string registerDeviceOk = "Device was successfully registered!";
        private const string registerDeviceFailed = "Device register faliled (possibly duplicate Device ID)";

        private const string registerGeoEventQuery = "WITH tmp_device AS (SELECT id FROM device WHERE device_id = '{0}')\r\n" +
                                                     "INSERT INTO event (device_id, event_type)\r\n" +
                                                     "SELECT id, 'geo' FROM tmp_device;\r\n" +
                                                     "INSERT INTO geo_event (event_id, x_coord, y_coord)\r\n" +
                                                     "SELECT last_insert_rowid(), {1}, {2};";

        private const string registerTemperatureEventQuery = "WITH tmp_device AS (SELECT id FROM device WHERE device_id = '{0}')\r\n" +
                                                             "INSERT INTO event (device_id, event_type)\r\n" +
                                                             "SELECT id, 'temperature' FROM tmp_device;\r\n" +
                                                             "INSERT INTO temperature_event (event_id, celcium_value)\r\n" +
                                                             "SELECT last_insert_rowid(), {1};";

        private const string eventInfoQuery = "SELECT device.device_id, COUNT(event.event_id), MAX(event.event_date)\r\n" +
                                              "FROM device\r\n" +
                                              "LEFT JOIN event ON event.device_id = device.id\r\n" +
                                              "GROUP BY device.device_id";

        public async Task RegisterDevice(string json)
        {
            var device = JsonConvert.DeserializeObject<Device>(json);
            var result = await ExecuteInsertQuery(string.Format(registerDeviceQuery, device.DeviceId));

            if (result)
                await Clients.All.SendAsync("ResultMessage", registerDeviceOk);
            else
                await Clients.All.SendAsync("ResultMessage", registerDeviceFailed);
        }

        public async Task RegisterGeoEvent(string json)
        {
            var numberFormat = new NumberFormatInfo { NumberDecimalSeparator = "." };

            var geoEvent = JsonConvert.DeserializeObject<GeoEvent>(json);
            _ = await ExecuteInsertQuery(string.Format(
                registerGeoEventQuery,
                new[] { geoEvent.DeviceId, geoEvent.CoordinateX.ToString(numberFormat), geoEvent.CoordinateY.ToString(numberFormat) }));
        }

        public async Task GetDeviceEventInfo()
        {
            var result = await ExecuteSelectQuery(string.Format(eventInfoQuery));
            await Clients.Caller.SendAsync("ReportMessage", JsonConvert.SerializeObject(result));
        }

        public async Task RegisterTemperatureEvent(string json)
        {
            var temperatureEvent = JsonConvert.DeserializeObject<TemperatureEvent>(json);
            _ = await ExecuteInsertQuery(string.Format(registerTemperatureEventQuery, new object[] { temperatureEvent.DeviceId, temperatureEvent.Temperature }));
        }

        private async Task<bool> ExecuteInsertQuery(string commandText)
        {
            var connection = await DbHelper.GetConnection();
            var result = 0;

            var command = new SqliteCommand
            {
                Connection = connection,
                CommandText = commandText
            };

            try
            {
                result = await command.ExecuteNonQueryAsync();
            }
            catch { }

            return result > 0;
        }

        private async Task<List<EventReport>> ExecuteSelectQuery(string commandText)
        {
            var connection = await DbHelper.GetConnection();
            var result = new List<EventReport>();

            var command = new SqliteCommand
            {
                Connection = connection,
                CommandText = commandText
            };
            using SqliteDataReader reader = await command.ExecuteReaderAsync();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    var report = new EventReport();

                    report.DeviceId = reader.GetString(0);
                    report.TotalEventsCount = reader.GetInt32(1);
                    if (reader.GetValue(2) is not DBNull)
                        report.LastEventDate = reader.GetDateTime(2);

                    result.Add(report);
                }
            }
            return result;
        }
    }
}
