FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/SupernovaSchool.Host/SupernovaSchool.Host.csproj", "src/SupernovaSchool.Host/"]
COPY ["client/YandexCalendar.Net/YandexCalendar.Net.csproj", "client/YandexCalendar.Net/"]
COPY ["src/SupernovaSchool.Application/SupernovaSchool.Application.csproj", "src/SupernovaSchool.Application/"]
COPY ["src/SupernovaSchool/SupernovaSchool.csproj", "src/SupernovaSchool/"]
COPY ["src/SupernovaSchool.Data/SupernovaSchool.Data.csproj", "src/SupernovaSchool.Data/"]
COPY ["src/SupernovaSchool.Telegram/SupernovaSchool.Telegram.csproj", "src/SupernovaSchool.Telegram/"]
RUN dotnet restore "src/SupernovaSchool.Host/SupernovaSchool.Host.csproj"
COPY . .
WORKDIR "/src/src/SupernovaSchool.Host"
RUN dotnet build "SupernovaSchool.Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "SupernovaSchool.Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SupernovaSchool.Host.dll"]
