Each lambda needs to be built and packaged into deployment ZIP before using `serverless deploy`

Build & package
`[ -f /Users/stian/IdeaProjects/KaggleAutomation/KaggleAutomation.sln ] && rm /Users/stian/IdeaProjects/KaggleAutomation/KaggleAutomation.sln; \
dotnet lambda package --configuration Release --output-package bin/Release/net6.0/kaggle.zip && \
dotnet lambda package --configuration Release --output-package bin/Release/net6.0/cleansing.zip && \
dotnet lambda package --configuration Release --output-package bin/Release/net6.0/heroku.zip`

#Serverless commands

npx sls invoke -f herokuDataInsert
npx sls deploy --function herokuDataInsert
