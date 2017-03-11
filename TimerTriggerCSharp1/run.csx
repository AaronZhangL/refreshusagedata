#load "..\shared\processmonth.csx"

public async static Task Run(TimerInfo myTimer, TraceWriter log)
{
    log.Info($"C# Timer trigger function executed at: {DateTime.Now}");
    string month = DateTime.Now.ToString("yyyy-MM");  
    await ProcessMonthAsync(month, log);
    if (DateTime.Now.Day <= 5) {
        month = DateTime.Now.AddMonths(-1).ToString("yyyy-MM"); 
        await ProcessMonthAsync(month, log);
    }
}

