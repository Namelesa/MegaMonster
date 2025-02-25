FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["MegaMonster.Services.Card/MegaMonster.Services.Card.csproj", "MegaMonster.Services.Card/"]
COPY ["MegaMonster.MessagingModels/MegaMonster.MessagingModels.csproj", "MegaMonster.MessagingModels/"]
RUN dotnet restore "MegaMonster.Services.Card/MegaMonster.Services.Card.csproj"
COPY . .
WORKDIR "/src/MegaMonster.Services.Card"
RUN dotnet build "MegaMonster.Services.Card.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "MegaMonster.Services.Card.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MegaMonster.Services.Card.dll"]
