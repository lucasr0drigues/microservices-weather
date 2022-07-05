FROM mcr.microsoft.com/dotnet/sdk:6.0-focal AS build
WORKDIR /source
COPY . .
RUN dotnet restore "./CloudWeather.DataLoader.csproj" --disable-parallel
RUN dotnet publish "./CloudWeather.DataLoader.csproj" -c Release -o /app --no-restore

# Server Stage
FROM mcr.microsoft.com/dotnet/aspnet:6.0-focal
WORKDIR /src
COPY --from=build /app ./

ENTRYPOINT ["dotnet", "CloudWeather.DataLoader.dll"]