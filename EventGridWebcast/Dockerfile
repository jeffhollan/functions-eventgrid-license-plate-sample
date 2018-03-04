FROM microsoft/azure-functions-runtime:v2.0.0-beta1
ENV AzureWebJobsScriptRoot=/home/site/wwwroot
ENV AzureWebJobsStorage=DefaultEndpointsProtocol=https;AccountName=eventgridwebcast;AccountKey=H4qeMVTbYJ4LbMlq0x1ACBhaHd39KU4Q7cjpHH1GMwPGor52Y4AyaRDxQbSw72uLNF8iWXB5k5DRzSH/I0Lujg==;BlobEndpoint=https://eventgridwebcast.blob.core.windows.net/;QueueEndpoint=https://eventgridwebcast.queue.core.windows.net/;TableEndpoint=https://eventgridwebcast.table.core.windows.net/;FileEndpoint=https://eventgridwebcast.file.core.windows.net/;
COPY ./bin/Debug/netstandard2.0 /home/site/wwwroot