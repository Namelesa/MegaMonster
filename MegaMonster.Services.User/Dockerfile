FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["MegaMonster.Services.User/MegaMonster.Services.User.csproj", "MegaMonster.Services.User/"]
COPY ["MegaMonster.MessagingModels/MegaMonster.MessagingModels.csproj", "MegaMonster.MessagingModels/"]
RUN dotnet restore "MegaMonster.Services.User/MegaMonster.Services.User.csproj"
COPY . .
WORKDIR "/src/MegaMonster.Services.User"
RUN dotnet build "MegaMonster.Services.User.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "MegaMonster.Services.User.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MegaMonster.Services.User.dll"]
