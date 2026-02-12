# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/JITAccessController.Web.Blazor/JITAccessController.Web.Blazor.csproj", "./"]
RUN dotnet restore --no-cache "JITAccessController.Web.Blazor.csproj"

COPY "src/JITAccessController.Web.Blazor/" "./"
RUN dotnet build "./JITAccessController.Web.Blazor.csproj" -c $BUILD_CONFIGURATION -r linux-x64 -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./JITAccessController.Web.Blazor.csproj" --no-restore -c $BUILD_CONFIGURATION -r linux-x64 -o /app/publish /p:UseAppHost=false

# Stage 2: Serve the application with a lean runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
USER 65532:65532
ENTRYPOINT ["dotnet", "JITAccessController.Web.Blazor.dll"]