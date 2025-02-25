FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["MegaMonster.Services.Payment/MegaMonster.Services.Payment.csproj", "MegaMonster.Services.Payment/"]
COPY ["MegaMonster.MessagingModels/MegaMonster.MessagingModels.csproj", "MegaMonster.MessagingModels/"]
RUN dotnet restore "MegaMonster.Services.Payment/MegaMonster.Services.Payment.csproj"
COPY . .
WORKDIR "/src/MegaMonster.Services.Payment"
RUN dotnet build "MegaMonster.Services.Payment.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "MegaMonster.Services.Payment.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MegaMonster.Services.Payment.dll"]
