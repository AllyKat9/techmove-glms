# Dockerfile for TechMoveGLMS-part-1 (MVC Web App)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["TechMoveGLMS-Web/TechMoveGLMS-Web.csproj", "TechMoveGLMS-Web/"]
RUN dotnet restore "TechMoveGLMS-Web/TechMoveGLMS-Web.csproj"

# Copy all source code
COPY . .

# Remove conflicting appsettings files
RUN rm -f TechMoveGLMS.API/appsettings.Development.json
RUN rm -f TechMoveGLMS.API/appsettings.json

# Publish the MVC project
WORKDIR "/src/TechMoveGLMS-Web"
RUN dotnet publish "TechMoveGLMS-Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Copy published files
COPY --from=build /app/publish .

# Set entry point
ENTRYPOINT ["dotnet", "TechMoveGLMS-Web.dll"]