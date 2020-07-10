#r "Newtonsoft.Json"

using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System;
using System.Data.SqlClient;

public static string UnixTimeStampToDateTime( double unixTimeStamp )
{
    // Unix timestamp is seconds past epoch
    //System.DateTime dtDateTime = new DateTime(1970,1,1,0,0,0,0,System.DateTimeKind.Utc);
    System.DateTime dtDateTime = new DateTime(1970,1,1,0,0,0,0);
    dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToUniversalTime();
    return dtDateTime.ToString("MM/dd/yyyy HH:mm:ss");
}

//SQL Database Conection
public static void updateDataBase(SqlConnection con, string deviceId, string fixedlat, string fixedlng, string timestamp, ILogger log) { 
    string update = $"INSERT INTO Table_sigfox (deviceID,latitude,longitude,timestamp)VALUES('{deviceId}',{fixedlat},{fixedlng},{timestamp})";

    try {
        con.Open();
        SqlCommand sqlCmd = new SqlCommand(update, con);
        sqlCmd.ExecuteNonQuery();
        con.Close();
        log.LogInformation(update);
    }
    catch (Exception e)
    {
        con.Close();
        log.LogInformation(e.ToString());
    }
}

public static async Task<IActionResult> Run(HttpRequest req, ILogger log)
{
    log.LogInformation("C# HTTP trigger function processed a request.");

    // Variable declaration & HTTP Request if exist
    string deviceId = req.Query["deviceId"];
    string raw_timestamp_str = req.Query["timestamp"];
    double raw_timestamp;
    string timestamp;
    string payload = req.Query["data"];
    string seqNumber_str = req.Query["seqNumber"];
    int seqNumber;
    string messages = req.Query["messages"];
    double lat;
    string fixedlat;
    double lng;
    string fixedlng;
    double radius;

    // JSON Deserialize Object
    string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
    dynamic data = JsonConvert.DeserializeObject(requestBody);

    // SQL Connection
    string connectionString =   "Server=tcp:serversqldatabase.database.windows.net,1433;Initial Catalog=sqldatabase;Persist Security Info=False;User ID=sigfoxadmin;Password=Sigfoxpa$$word;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30";
    SqlConnection con = new SqlConnection(connectionString);

    deviceId = deviceId ?? data?.deviceId;
    raw_timestamp = raw_timestamp_str ?? data?.timestamp;
    payload = payload ?? data?.datos;
    seqNumber = (seqNumber_str ?? data?.seqNumber);

    //message
    lat = messages ?? data?.messages[1].value.lat;
    fixedlat = Convert.ToString(lat);
    lng = messages ?? data?.messages[1].value.lng;
    fixedlng = Convert.ToString(lng);
    radius = messages ?? data?.messages[1].value.radius;
    timestamp = UnixTimeStampToDateTime(raw_timestamp);;

    // Show Logs in console
    log.LogInformation($"deviceId: {deviceId}");
    log.LogInformation($"raw_timestamp: {raw_timestamp}");
    log.LogInformation($"timestamp: {timestamp}");
    log.LogInformation($"payload: {payload}");
    log.LogInformation($"seqNumber: {seqNumber}");
    log.LogInformation($"lat: {fixedlat}");
    log.LogInformation($"lng: {fixedlng}");
    log.LogInformation($"radius: {radius}");
    
    updateDataBase(con, deviceId, fixedlat, fixedlng, timestamp, log);

    return deviceId != null
        ? (ActionResult)new OkObjectResult($"Ok")
        : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
}