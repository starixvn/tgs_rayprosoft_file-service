FROM hub.lumigame.com/base/dotnet/core/sdk:8.0 AS build
ARG BuildConfiguration=Release
#todo RUN --mount=type=secret,id=nuget,dst=/pat cp /pat ~/.nuget/NuGet/NuGet.Config
RUN dotnet nuget remove source 'Microsoft Visual Studio Offline Packages' || true
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["FileService/FileService.csproj", "FileService/"]
COPY ["FileService.Core/FileService.Core.csproj", "FileService.Core/"]
COPY ["MongoFileMetaRepository/MongoFileMetaRepository.csproj", "MongoFileMetaRepository/"]
RUN dotnet restore "FileService/FileService.csproj"
COPY . .
WORKDIR "/src/FileService"
RUN dotnet build "FileService.csproj" -c ${BuildConfiguration} -o /app/build

FROM build AS publish
ARG BuildConfiguration
RUN dotnet publish "FileService.csproj" -c ${BuildConfiguration} -o /app/publish

FROM hub.lumigame.com/base/dotnet/core/aspnet:8.0-alpine-24h1.1 AS base
USER app
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FileService.dll"]