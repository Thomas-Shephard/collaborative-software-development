FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Build.props", "Directory.Build.props"]
COPY ["src/Jahoot.Core/Jahoot.Core.csproj", "src/Jahoot.Core/"]
COPY ["src/Jahoot.WebApi/Jahoot.WebApi.csproj", "src/Jahoot.WebApi/"]
RUN dotnet restore "src/Jahoot.WebApi/Jahoot.WebApi.csproj"
COPY ["src/Jahoot.Core/", "src/Jahoot.Core/"]
COPY ["src/Jahoot.WebApi/", "src/Jahoot.WebApi/"]
WORKDIR "/src/src/Jahoot.WebApi"
RUN dotnet build "./Jahoot.WebApi.csproj" -c "$BUILD_CONFIGURATION" -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Jahoot.WebApi.csproj" -c "$BUILD_CONFIGURATION" -o /app/publish /p:UseAppHost=false

FROM base AS final
USER $APP_UID
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Jahoot.WebApi.dll"]
