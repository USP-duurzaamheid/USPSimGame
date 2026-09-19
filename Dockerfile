
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Directory.Build.props", "./"]
COPY ["USPSimGame.sln", "./"]
COPY ["src/USPSimGame.Domain/USPSimGame.Domain.csproj", "src/USPSimGame.Domain/"]
COPY ["src/USPSimGame.Application/USPSimGame.Application.csproj", "src/USPSimGame.Application/"]
COPY ["src/USPSimGame.Infrastructure/USPSimGame.Infrastructure.csproj", "src/USPSimGame.Infrastructure/"]
COPY ["src/USPSimGame.Web/USPSimGame.Web.csproj", "src/USPSimGame.Web/"]
RUN dotnet restore "src/USPSimGame.Web/USPSimGame.Web.csproj"

COPY src/ src/
RUN dotnet publish "src/USPSimGame.Web/USPSimGame.Web.csproj" \
    -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:5261
EXPOSE 5261
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "USPSimGame.Web.dll"]
