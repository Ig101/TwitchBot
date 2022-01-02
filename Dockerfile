# Dotnet restore
FROM mcr.microsoft.com/dotnet/core/sdk:6.0 AS dotnetBuild
WORKDIR /src
COPY TwitchBot.Web/*.csproj TwitchBot.Web/
COPY TwitchBot.Application/*.csproj TwitchBot.Application/
COPY TwitchBot.Domain/*.csproj TwitchBot.Domain/
WORKDIR /src/TwitchBot.Web
RUN dotnet restore
WORKDIR /src
COPY . .

# Dotnet publish
FROM dotnetBuild AS publish
WORKDIR /src/TwitchBot.Web
RUN dotnet publish -c Release -o /src/publish

# Run
FROM mcr.microsoft.com/dotnet/core/aspnet:6.0 AS runtime
WORKDIR /app
COPY --from=publish /src/publish .
CMD ASPNETCORE_URLS=http://*:$PORT dotnet TwitchBot.Web.dll