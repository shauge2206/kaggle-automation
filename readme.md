Each lambda needs to be built and packaged into deployment ZIP before using `serverless deploy`

Build & package
`dotnet lambda package --configuration Release --output-package bin/Release/net6.0/kaggle.zip && dotnet lambda package --configuration Release --output-package bin/Release/net6.0/cleansing.zip`



#Serverless commands

npx sls invoke -f kaggleDownload
npx sls deploy --function kaggleDownload
