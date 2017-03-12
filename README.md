# refreshusagedata
This Azure Functions project uses the Azure EA Billing API (https://automaticbillingspec.blob.core.windows.net/spec/MicrosoftAzure%e2%80%93EABilling%26UsageAPI.docx) to download detailed usage information in CSV format to an Azure Storage account such that it can then be used with analysis tools such as Power BI. Though Power BI has a content pack for this very purpose (https://powerbi.microsoft.com/en-us/documentation/powerbi-content-pack-azure-enterprise/), I have found it to not be reliable for very large data sizes (when monthly usage data is greater than ~500MB).

The typical usage scenario is to use the TimerTrigger to fire once a day. It will upload to Azure Blob Storage the current month's partial data and during the first 5 days of the month will also upload the previous month's usage. Uploads only happen if there is new data and a snapshot of the existing blob is taken to preserve history.

A WebHook trigger is also included in case there is a need to process a specific month. The expected payload has the following format:
{
    "month": "YYYY-MM"
}

The following values need to be created and appropriately populated in App Settings: StorageAccountName, StorageAccountKey, ContainerName, EnrollmentNumber, EnrollmentKey.

This project only deals with uploading usage data to Azure Blob Storage. For analysis, I recommend you look at the very good work done by Thomas Vuylsteke at https://github.com/tvuylsteke/azure-ea-powerbi from where I was able to incorporate many ideas into the final solution I have in place. 
