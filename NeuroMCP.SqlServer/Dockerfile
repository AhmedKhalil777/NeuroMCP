FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["NeuroMCP.SqlServer.csproj", "./"]
RUN dotnet restore "NeuroMCP.SqlServer.csproj"

# Copy all files and build
COPY . .
RUN dotnet publish "NeuroMCP.SqlServer.csproj" -c Release -o /app/publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Create configuration for default settings
RUN mkdir -p /app/config

# Expose port
EXPOSE 5200

# Set entry point
ENTRYPOINT ["dotnet", "NeuroMCP.SqlServer.dll", "--port", "5200"] 