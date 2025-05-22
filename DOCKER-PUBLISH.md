# Docker Image Publishing Guide

This document explains how to set up GitHub Actions to automatically build and publish Docker images for the NeuroMCP projects.

## Required GitHub Secrets

You need to set up the following secrets in your GitHub repository:

1. **DOCKERHUB_USERNAME**: Your Docker Hub username
2. **DOCKERHUB_TOKEN**: Your Docker Hub access token (not your password)

### Getting a Docker Hub Token

1. Log in to your Docker Hub account: https://hub.docker.com/
2. Go to Account Settings > Security > Access Tokens
3. Click "New Access Token" and create a token with appropriate permissions
4. Copy the token immediately (you won't be able to see it again)

### Adding Secrets to Your GitHub Repository

1. Go to your GitHub repository
2. Navigate to Settings > Secrets and variables > Actions
3. Click "New repository secret"
4. Add both secrets with the exact names shown above

## GitHub Actions Workflow

The workflow is configured to:

1. Build automatically on pushes to the main branch
2. Build automatically when you create a tag (v*)
3. Build both NeuroMCP.AzureDevOps and NeuroMCP.SqlServer projects
4. Push the images to Docker Hub with appropriate tags

## Image Tagging

The images will be tagged with:

- `latest` for the main branch
- The git SHA for each build
- Semantic versioning if you use tags (e.g., v1.0.0 becomes tags 1.0.0 and 1.0)

## Images on Docker Hub

After successful builds, your images will be available at:

- `yourusername/neuromcp-azuredevops`
- `yourusername/neuromcp-sqlserver`

## Pulling the Images

Users can pull your images using:

```bash
docker pull yourusername/neuromcp-azuredevops
docker pull yourusername/neuromcp-sqlserver
```

## Troubleshooting

If the publishing fails, check:

1. Are your Docker Hub credentials correct?
2. Does your Docker Hub user have permission to push to these repositories?
3. Are the repository names already taken by someone else?

You may need to create the repositories in Docker Hub first if they don't already exist. 