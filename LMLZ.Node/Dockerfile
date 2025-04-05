FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["LMLZ.Node/LMLZ.Node.csproj", "LMLZ.Node/"]
RUN dotnet restore "./LMLZ.Node/LMLZ.Node.csproj"
COPY . .
WORKDIR "/src/LMLZ.Node"
RUN dotnet build "./LMLZ.Node.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./LMLZ.Node.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LMLZ.Node.dll"]