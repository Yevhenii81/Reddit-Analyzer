FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY nuget.config ./
COPY *.csproj ./
RUN dotnet restore --configfile nuget.config
COPY . .
RUN dotnet publish -c Release -o publish

RUN dotnet tool install --global Microsoft.Playwright.CLI
ENV PATH="$PATH:/root/.dotnet/tools"
RUN playwright install chromium
RUN playwright install-deps chromium

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

COPY --from=build /root/.cache/ms-playwright /root/.cache/ms-playwright

RUN apt-get update && apt-get install -y \
    libnss3 \
    libatk1.0-0 \
    libatk-bridge2.0-0 \
    libcups2 \
    libdrm2 \
    libxkbcommon0 \
    libxcomposite1 \
    libxdamage1 \
    libxfixes3 \
    libxrandr2 \
    libgbm1 \
    libasound2 \
    libpango-1.0-0 \
    libcairo2 \
    && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "RedditAnalyzer.dll"]