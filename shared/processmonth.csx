#r "Microsoft.WindowsAzure.Storage"

public class ProcessMonth {
    static HttpClient httpClient;
    static string saKey;
    static string saName;
    static string containerName;
    static string enrollmentKey;
    static string enrollmentNumber;
    
    static ProcessMonth() {
        httpClient = new System.Net.Http.HttpClient();
        saKey = System.Configuration.ConfigurationManager.AppSettings["StorageAccountKey"];
        saName = System.Configuration.ConfigurationManager.AppSettings["StorageAccountName"];
        containerName = System.Configuration.ConfigurationManager.AppSettings["ContainerName"];  
        enrollmentKey = System.Configuration.ConfigurationManager.AppSettings["EnrollmentKey"];
        enrollmentNumber = System.Configuration.ConfigurationManager.AppSettings["EnrollmentNumber"];
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", enrollmentKey);
        httpClient.DefaultRequestHeaders.Add("api-version","1.0");
        httpClient.Timeout = new System.TimeSpan(0,5,0);

    }
    
    public async Task<string> ProcessMonthAsync(string month, TraceWriter log)
    {
        string linkToDownloadDetailReport = $"https://ea.azure.com/rest/{enrollmentNumber}/usage-report?month={month}&type=detail";
        string outFile = $"Details{month}.csv";
        
        log.Info($"url: {linkToDownloadDetailReport}");
        HttpResponseMessage response = await httpClient.GetAsync(linkToDownloadDetailReport);
        string responseMessage = string.Empty;
        if (response.IsSuccessStatusCode) {
            log.Info("HTTP call suceeded");
            Stream outputstream = await response.Content.ReadAsStreamAsync();
            Nullable<long> responsesize = response.Content.Headers.ContentLength;
            string storageAccountEndpoint = $"http://{saName}.blob.core.windows.net/";
            Microsoft.WindowsAzure.Storage.Auth.StorageCredentials storageCredentials = new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(saName, saKey);
            Microsoft.WindowsAzure.Storage.Blob.CloudBlobClient blobClient = new Microsoft.WindowsAzure.Storage.Blob.CloudBlobClient(new Uri(storageAccountEndpoint), storageCredentials);
            Microsoft.WindowsAzure.Storage.Blob.CloudBlobContainer containerReference = blobClient.GetContainerReference(containerName);
            Microsoft.WindowsAzure.Storage.Blob.CloudBlockBlob blockBlobReference = containerReference.GetBlockBlobReference(outFile);
            long size = 0;
            if (blockBlobReference.Exists()) {
                blockBlobReference.FetchAttributes();
                size = blockBlobReference.Properties.Length;
            }
            if (responsesize > size) {
                if (blockBlobReference.Exists()) {
                    blockBlobReference.CreateSnapshot();
                }
                await blockBlobReference.UploadFromStreamAsync(outputstream);
                responseMessage = $"Saved to file: {outFile} Size: {responsesize.ToString()}";
                log.Info(responseMessage);
            }
            else {
                responseMessage = $"Skipped file: {outFile} Size: {responsesize.ToString()} as existing Blob is larger or equal in size: {size.ToString()}";
                log.Info(responseMessage);
            }
        }
        else {
            responseMessage = "HTTP call failed";
            log.Info(responseMessage);
        }
        return responseMessage;
    }
}
