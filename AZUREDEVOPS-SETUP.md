# Setting Up Azure DevOps Pipeline for NeuroMCP

This document explains how to set up an Azure DevOps pipeline to build and publish NeuroMCP packages.

## Prerequisites

1. An Azure DevOps account
2. A NuGet API key for publishing to NuGet.org
3. A GitHub Personal Access Token (PAT) if you wish to create GitHub releases

## Steps to Set Up the Pipeline

### 1. Create Service Connections

You'll need two service connections:

#### NuGet.org Service Connection

1. In your Azure DevOps project, go to **Project Settings** > **Service Connections** > **New Service Connection**
2. Select **NuGet**
3. Enter the following information:
   - **Connection Name**: `NuGet.org`
   - **Feed URL**: `https://api.nuget.org/v3/index.json`
   - **API Key**: Your NuGet API key
4. Click **Save**

#### GitHub Service Connection (for GitHub releases)

1. In your Azure DevOps project, go to **Project Settings** > **Service Connections** > **New Service Connection**
2. Select **GitHub**
3. Choose **Personal Access Token** authentication
4. Enter the following information:
   - **Connection Name**: `GitHub`
   - **Personal Access Token**: Your GitHub PAT (needs `repo` scope)
5. Click **Save**

### 2. Create the Pipeline

1. In your Azure DevOps project, go to **Pipelines** > **New Pipeline**
2. Select your repository source (GitHub, Azure Repos, etc.)
3. Select your repository
4. Select **Existing Azure Pipelines YAML file**
5. Enter the path: `/azure-pipelines.yml`
6. Click **Continue**
7. Review the pipeline and click **Save and Run**

### 3. Pipeline Features

This pipeline will:

- Build both NeuroMCP.SqlServer and NeuroMCP.AzureDevOps packages
- Set the version based on git tags (for releases) or generate a development version
- Publish packages as build artifacts
- When triggered by a version tag (e.g., `v1.2.3`):
  - Push packages to NuGet.org
  - Create a GitHub release with the packages

### 4. Releasing a New Version

To release a new version:

1. Tag the commit with a version tag, e.g. `v1.2.3`:
   ```bash
   git tag -a v1.2.3 -m "Release v1.2.3"
   git push origin v1.2.3
   ```

2. The pipeline will automatically:
   - Build the packages with the specified version
   - Push the packages to NuGet.org
   - Create a GitHub release

### 5. Common Issues

#### Missing Service Connections

If the pipeline fails with errors about missing service connections, ensure you've created the service connections with the exact names specified:
- `NuGet.org` for NuGet publishing
- `GitHub` for GitHub releases

#### Version Already Exists

If the pipeline fails because the version already exists on NuGet.org, you'll need to:
1. Increment the version tag
2. Push the new tag

#### Insufficient Permissions

Ensure your NuGet API key has push permissions and your GitHub PAT has repository access permissions.

### 6. Advanced Configuration

You can modify `azure-pipelines.yml` to:
- Change the versioning scheme
- Add more packages
- Add tests
- Configure multi-stage deployment (e.g., to a private feed first)
- Add additional validation steps 