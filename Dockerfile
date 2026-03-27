# -------- BUILD STAGE --------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj
COPY MDMPI.App/*.csproj ./MDMPI.App.Api/
WORKDIR /src/MDMPI.App.Api

RUN dotnet restore

# Copy full solution
COPY . .
WORKDIR /src/MDMPI.App.Api

# 🔥 FIXED LINE
RUN dotnet publish MDMPI.App.Api.csproj -c Release -o /app/publish

# -------- RUNTIME STAGE --------
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:10000

ENTRYPOINT ["dotnet", "MDMPI.App.Api.dll"]