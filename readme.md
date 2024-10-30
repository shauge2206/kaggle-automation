Each lambda needs to be built and packaged into deployment ZIP before using `serverless deploy`

#Serverless commands
npx sls invoke -f herokuDataInsert
npx sls deploy --function herokuDataInsert

npm run package