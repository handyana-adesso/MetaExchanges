# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution files
COPY MetaExchange.sln ./
COPY src/MetaExchange.Core/ src/MetaExchange.Core/
COPY src/MetaExchange.WebApi/ src/MetaExchange.WebApi/

# Restore dependencies
RUN dotnet restore "src/MetaExchange.WebApi/MetaExchange.WebApi.csproj"

# Now copy the rest of the source
COPY src/ ./src/

# Build the Web API
RUN dotnet publish "src/MetaExchange.WebApi/MetaExchange.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy build artifacts from the previous stage
COPY --from=build /app/publish .

# Expose the default port
EXPOSE 8080
EXPOSE 8081

# Run the API
ENTRYPOINT ["dotnet", "MetaExchange.WebApi.dll"]

## This stage is used when running from VS in fast mode (Default for Debug configuration)
#FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
#USER $APP_UID
#WORKDIR /app
#EXPOSE 8080
#EXPOSE 8081
#
#
## This stage is used to build the service project
#FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
#ARG BUILD_CONFIGURATION=Release
#WORKDIR /src
#COPY ["src/MetaExchange.WebApi/MetaExchange.WebApi.csproj", "src/MetaExchange.WebApi/"]
#RUN dotnet restore "./src/MetaExchange.WebApi/MetaExchange.WebApi.csproj"
#COPY . .
#WORKDIR "/src/src/MetaExchange.WebApi"
#RUN dotnet build "./MetaExchange.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build
#
## This stage is used to publish the service project to be copied to the final stage
#FROM build AS publish
#ARG BUILD_CONFIGURATION=Release
#RUN dotnet publish "./MetaExchange.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false
#
## This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "MetaExchange.WebApi.dll"]