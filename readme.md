Project needs to be built and packaged into deployment ZIP before using `serverless deploy`

Build & package
`dotnet lambda package --configuration Release --output-package bin/Release/net6.0/kaggle.zip`



#Serverless commands

npx sls invoke -f kaggleDownload
