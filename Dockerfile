FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY nuget.config ./
COPY *.csproj ./
RUN dotnet restore --configfile nuget.config
COPY . .
RUN dotnet publish -c Release -o publish
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "RedditAnalyzer.dll"]