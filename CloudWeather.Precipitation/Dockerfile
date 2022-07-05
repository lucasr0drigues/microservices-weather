FROM mcr.microsoft.com/dotnet/sdk:6.0-focal AS build
WORKDIR /source
COPY . .
RUN dotnet restore "./CloudWeather.Precipitation.csproj" --disable-parallel
RUN dotnet publish "./CloudWeather.Precipitation.csproj" -c Release -o /app --no-restore

# Server Stage
FROM mcr.microsoft.com/dotnet/aspnet:6.0-focal
WORKDIR /src
COPY --from=build /app ./

EXPOSE 5000

ENTRYPOINT ["dotnet", "CloudWeather.Precipitation.dll"]