# Dockerfile based off samples in https://github.com/dotnet/dotnet-docker-samples

# "Build Stage" Container: "build-env"
FROM mcr.microsoft.com/dotnet/core/sdk:3.0.100 AS build-env

ENV DOTNET_CLI_TELEMETRY_OPTOUT 1
ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE 1
ENV PATH="$PATH:/root/.dotnet/tools"

WORKDIR /app

# copy csproj and restore as distinct layers
COPY *.sln .
COPY src/FurCoNZ.Web/*.csproj ./src/FurCoNZ.Web/

RUN dotnet restore
RUN dotnet tool install -g Microsoft.Web.LibraryManager.Cli

# copy everything else and publish
COPY src/ ./src/

RUN dotnet publish -c Release -o out
RUN libman restore

# "Front-End Build Stage" Container: "build-frontend"
FROM node AS build-frontend

WORKDIR /app/src/FurCoNZ.Web

COPY src/FurCoNZ.Web/package*.json ./
RUN npm install

COPY src/FurCoNZ.Web/ ./

RUN npm run build

# "Runtime Stage" Container: "runtime"
FROM mcr.microsoft.com/dotnet/core/aspnet:2.2 AS runtime

WORKDIR /app

ENV DOTNET_CLI_TELEMETRY_OPTOUT 1
ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE 1
EXPOSE 80
EXPOSE 443

COPY --from=build-env /app/src/FurCoNZ.Web/out ./
COPY --from=build-env /app/src/FurCoNZ.Web/wwwroot/lib ./
COPY --from=build-frontend /app/src/FurCoNZ.Web/wwwroot/css/bundle* ./wwwroot/css/

ENTRYPOINT ["dotnet", "FurCoNZ.Web.dll"]
