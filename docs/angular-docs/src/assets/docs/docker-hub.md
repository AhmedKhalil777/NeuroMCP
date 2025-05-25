# Docker Hub Setup Guide

This guide explains how to set up your Docker Hub repositories and link documentation to them.

## Creating Docker Hub Repositories

1. Log in to your Docker Hub account at [https://hub.docker.com/](https://hub.docker.com/)
2. Click on "Create Repository" button
3. Create two repositories:
   - `neuromcp-azuredevops`
   - `neuromcp-sqlserver`
4. Set visibility to Public or Private based on your needs

## Setting Up Repository Descriptions

For each repository:

1. Go to the repository page on Docker Hub
2. Click on the "Manage Repository" button
3. On the "General" tab, you can:
   - Add a short description
   - Add a full description in Markdown format
   - Set repository tags

## Adding Documentation to Docker Hub

Docker Hub supports Markdown in the "Full Description" field. You can:

1. Go to your repository page
2. Click "Manage Repository"
3. In the "General" tab, under "Full Description":
   - Copy and paste the contents of the DOCKERHUB.md files
   - Add links to GitHub documentation

### For NeuroMCP.AzureDevOps:

Copy the content from `NeuroMCP.AzureDevOps/DOCKERHUB.md` into the full description field.

### For NeuroMCP.SqlServer:

Copy the content from `NeuroMCP.SqlServer/DOCKERHUB.md` into the full description field.

## Linking Documentation

You can link to documentation in several ways:

1. **Direct links in the Full Description field:**
   - Add links to your GitHub repository
   - Link to specific documentation files

2. **Setting Website URL:**
   - In the repository settings, set the Website URL to your GitHub project

3. **Docker Hub Labels:**
   - Add labels like `documentation=https://github.com/AhmedKhalil777/NeuroMCP` when pushing images

## Updating Documentation

To update the documentation:

1. Update the DOCKERHUB.md files in your GitHub repository
2. Copy the new content to the Docker Hub repository descriptions
3. For automatic updates, consider using Docker Hub's GitHub integration:
   - Go to "Manage Repository" > "Builds" tab
   - Link your GitHub repository
   - Configure automated builds

## Promoting Your Docker Images

To make your Docker images more discoverable:

1. Add appropriate tags to your repositories:
   - mcp
   - neuromcp
   - azuredevops
   - sqlserver
   - dotnet

2. Provide usage examples in your description

3. Link to sample applications or use cases

4. Keep images up-to-date with regular releases

## Docker Hub GitHub Integration

You can automate documentation updates by setting up GitHub integration:

1. Go to your Docker Hub repository
2. Click "Builds" tab
3. Link your GitHub repository
4. Configure automated builds
5. Set up README.md syncing

This will automatically keep your Docker Hub description in sync with your GitHub repository's README or specified documentation file. 