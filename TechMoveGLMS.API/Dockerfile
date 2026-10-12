# Dockerfile for TechMoveGLMS.API
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["TechMoveGLMS.API/TechMoveGLMS.API.csproj", "TechMoveGLMS.API/"]
COPY ["TechMoveGLMS-part-1/TechMoveGLMS-part-1.csproj", "TechMoveGLMS-part-1/"]
RUN dotnet restore "TechMoveGLMS.API/TechMoveGLMS.API.csproj"

# Copy all source code
COPY . .

# Remove conflicting appsettings files
RUN rm -f TechMoveGLMS-part-1/appsettings.Development.json
RUN rm -f TechMoveGLMS-part-1/appsettings.json

# Publish the API project
WORKDIR "/src/TechMoveGLMS.API"
RUN dotnet publish "TechMoveGLMS.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Copy published files
COPY --from=build /app/publish .

# Set entry point
ENTRYPOINT ["dotnet", "TechMoveGLMS.API.dll"]