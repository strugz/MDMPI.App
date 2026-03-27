# -------- BUILD STAGE --------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy ALL project files first (for restore optimization)
COPY MDMPI.App.Api/*.csproj ./MDMPI.App.Api/
COPY MDMPI.App.Common/*.csproj ./MDMPI.App.Common/
COPY MDMPI.App.Core/*.csproj ./MDMPI.App.Core/
COPY MDMPI.App.Data/*.csproj ./MDMPI.App.Data/

# Restore dependencies
WORKDIR /src/MDMPI.App.Api
RUN dotnet restore

# Copy everything else
COPY . .
WORKDIR /src/MDMPI.App.Api

# Publish ONLY API project
RUN dotnet publish MDMPI.App.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# -------- RUNTIME STAGE --------
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:10000

ENTRYPOINT ["dotnet", "MDMPI.App.Api.dll"]