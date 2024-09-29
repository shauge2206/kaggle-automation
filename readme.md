
##Star docker daemon
star docker

dotnet tool install -g Amazon.Lambda.Tools

package
dotnet lambda package --configuration Release --output-package bin/Release/net6.0/hello.zip

#Run lambda locally
npx serverless invoke local -f kaggleIntegration


public.ecr.aws/lambda/dotnet:6.2024.09.13.17

serverless invoke local -f kaggleIntegration --docker --docker-arg="-e DOCKER_IMAGE=public.ecr.aws/lambda/dotnet:6" --verbose






#???
Build:
dotnet build --configuration Release