
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Azure.EventGrid;
using System.Data.SqlClient;
using System;
using System.Data;

namespace Vehicle.Process
{
    public static class EventToSQL
    {
        private static SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder { UserID = "jeffhollan", Password = "Scottgudemo1!", DataSource = "jeffhollaneventgrid.database.windows.net", InitialCatalog = "vehicles", MultipleActiveResultSets = true};
        private static SqlConnection connection = new SqlConnection(builder.ConnectionString);
        [FunctionName("EventToSQL")]
        public static async Task Run(
            [EventGridTrigger]EventGridEvent vehicleEvent,
            TraceWriter log
            )
        {
            log.Info($"EventGrid trigger fired: {vehicleEvent.EventType}");

            if(connection.State != ConnectionState.Open) 
            {
                
                await connection.OpenAsync();
            }
            string cmdText = 
                "INSERT INTO [dbo].[entry_log](Id, PlateNumber, GarageId, GateId) " +
                $"VALUES ('{vehicleEvent.Id}', '{vehicleEvent.Data["plateNumber"]}', '{vehicleEvent.Data["garageId"]}', '{vehicleEvent.Data["gateNumber"]}')";
            using(SqlCommand command = new SqlCommand(cmdText, connection)) {
                await command.ExecuteNonQueryAsync();
            }
        }
    }
}
