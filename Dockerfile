FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/CleaningMyName.Api/CleaningMyName.Api.csproj", "src/CleaningMyName.Api/"]
COPY ["src/CleaningMyName.Application/CleaningMyName.Application.csproj", "src/CleaningMyName.Application/"]
COPY ["src/CleaningMyName.Domain/CleaningMyName.Domain.csproj", "src/CleaningMyName.Domain/"]
COPY ["src/CleaningMyName.Infrastructure/CleaningMyName.Infrastructure.csproj", "src/CleaningMyName.Infrastructure/"]
RUN dotnet restore "src/CleaningMyName.Api/CleaningMyName.Api.csproj"
COPY . .
WORKDIR "/src/src/CleaningMyName.Api"
RUN dotnet build "CleaningMyName.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CleaningMyName.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CleaningMyName.Api.dll"]
