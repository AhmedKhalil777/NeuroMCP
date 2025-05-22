# Docker Guide for NeuroMCP.AzureDevOps

This guide explains how to build, run, and deploy the NeuroMCP.AzureDevOps service using Docker.

## Prerequisites

- Docker installed on your machine
- Docker Hub account (for publishing)

## Building the Docker Image

Navigate to the NeuroMCP.AzureDevOps directory and build the Docker image:

```bash
cd NeuroMCP.AzureDevOps
docker build -t neuromcp-azuredevops .
```

## Running the Docker Container

Run the container with default settings:

```bash
docker run -p 5300:5300 neuromcp-azuredevops
```

### Using Environment Variables

You can configure the service using environment variables:

```bash
docker run -p 5300:5300 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  neuromcp-azuredevops
```

### Using Volume Mounts for Configuration

Mount a configuration directory to provide custom settings:

```bash
docker run -p 5300:5300 \
  -v $(pwd)/config:/app/config \
  neuromcp-azuredevops
```

Create an `appsettings.json` file in your local config directory with your Azure DevOps settings:

```json
{
  "azureDevOps": {
    "orgUrl": "https://dev.azure.com/your-organization",
    "defaultProject": "your-project",
    "authentication": {
      "type": "pat",
      "patToken": "your-pat-token"
    }
  }
}
```

## Using Docker Compose

For easier development, you can use Docker Compose:

```bash
cd NeuroMCP.AzureDevOps
docker-compose up
```

To run in detached mode:

```bash
docker-compose up -d
```

## Publishing to Docker Hub

1. Tag your image:

```bash
docker tag neuromcp-azuredevops yourusername/neuromcp-azuredevops:latest
```

2. Log in to Docker Hub:

```bash
docker login
```

3. Push the image:

```bash
docker push yourusername/neuromcp-azuredevops:latest
```

## Using the Docker Hub Image

Once published, you can use the image directly:

```bash
docker pull yourusername/neuromcp-azuredevops:latest
docker run -p 5300:5300 yourusername/neuromcp-azuredevops:latest
```

## Security Considerations

- Never store sensitive PAT tokens in your Docker image
- Use environment variables or volume mounts to provide configuration
- Consider using Docker secrets for production environments 