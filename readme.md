build & package
dotnet lambda package --configuration Release --output-package bin/Release/net6.0/hello.zip

#Run lambda locally
npx serverless invoke local -f kaggleIntegration