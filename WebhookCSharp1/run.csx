#r "Newtonsoft.Json"
#r "System.Configuration"
#load "..\shared\processmonth.csx"

using System;
using System.Net;
using Newtonsoft.Json;
using System.Configuration;

public static async Task<object> Run(HttpRequestMessage req, TraceWriter log)
{
    log.Info($"Webhook was triggered!");

    string jsonContent = await req.Content.ReadAsStringAsync();
    dynamic data = JsonConvert.DeserializeObject(jsonContent);

    if (data.month == null) {
        return req.CreateResponse(HttpStatusCode.BadRequest, new {
            error = "Please pass the month to process in YYYY-MM format in the input object"
        });
    }
    string month = data.month.ToString();
    //valid years: 2010-2099
    string regexString = @"^20[1-9][0-9]-(0[1-9]|1[0-2])$";
    RegexStringValidator regex = new RegexStringValidator(regexString);
    try {
        regex.Validate(month);
    }
    catch {
        return req.CreateResponse(HttpStatusCode.BadRequest, new {
            error = "Month must be in YYYY-MM format in the input object"
        });        
    }
    ProcessMonth processMonth = new ProcessMonth();
    string returnMessage = await processMonth.ProcessMonthAsync(month, log);
    return req.CreateResponse(HttpStatusCode.OK, new {
        message = $"Month {month} processed: {returnMessage}"
    });
}
