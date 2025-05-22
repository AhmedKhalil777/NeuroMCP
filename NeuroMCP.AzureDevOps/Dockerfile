FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["NeuroMCP.AzureDevOps.csproj", "./"]
RUN dotnet restore "NeuroMCP.AzureDevOps.csproj"

# Copy all files and build
COPY . .
RUN dotnet publish "NeuroMCP.AzureDevOps.csproj" -c Release -o /app/publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Create configuration for default settings
RUN mkdir -p /app/config

# Expose port
EXPOSE 5300

# Set entry point
ENTRYPOINT ["dotnet", "NeuroMCP.AzureDevOps.dll", "--port", "5300"] 