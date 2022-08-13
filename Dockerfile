FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:6.0 AS publish
WORKDIR /src
COPY src/SolarLogExporter/SolarLogExporter.csproj SolarLogExporter/
WORKDIR /src/SolarLogExporter
RUN dotnet restore SolarLogExporter.csproj
COPY src/SolarLogExporter/ .
RUN dotnet publish SolarLogExporter.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SolarLogExporter.dll"]