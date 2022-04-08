FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["ElasticSearch/ElasticSearch.csproj", "ElasticSearch/"]
RUN dotnet restore "ElasticSearch/ElasticSearch.csproj"
COPY . .
WORKDIR "/src/ElasticSearch"
RUN dotnet build "ElasticSearch.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ElasticSearch.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ElasticSearch.dll"]
