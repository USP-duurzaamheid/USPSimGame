
# Build stage with .NET 10 SDK
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["USPSimGame.csproj", "."]
RUN dotnet restore "./USPSimGame.csproj"

COPY . .
RUN dotnet publish "./USPSimGame.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:5261
EXPOSE 5261

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "USPSimGame.dll"]
