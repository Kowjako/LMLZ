FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["LMLZ.BootstrapNode/LMLZ.BootstrapNode.csproj", "LMLZ.BootstrapNode/"]
RUN dotnet restore "./LMLZ.BootstrapNode/LMLZ.BootstrapNode.csproj"
COPY . .
WORKDIR "/src/LMLZ.BootstrapNode"
RUN dotnet build "./LMLZ.BootstrapNode.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./LMLZ.BootstrapNode.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LMLZ.BootstrapNode.dll"]