# -------- BUILD STAGE --------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj from subfolder
COPY MDMPI.App.Api/*.csproj ./MDMPI.App.Api/
WORKDIR /src/MDMPI.App.Api

# Restore dependencies
RUN dotnet restore

# Copy full project
COPY . .
WORKDIR /src/MDMPI.App.Api

# Publish
RUN dotnet publish -c Release -o /app/publish

# -------- RUNTIME STAGE --------
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/publish .

# Required for Render
ENV ASPNETCORE_URLS=http://+:10000

ENTRYPOINT ["dotnet", "MDMPI.App.Api.dll"]