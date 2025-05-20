# Publishing NeuroMCP.SqlServer to NuGet

This document provides instructions for publishing the NeuroMCP.SqlServer package to NuGet using the GitHub Actions workflow.

## Prerequisites

1. A NuGet.org account
2. GitHub repository with the NeuroMCP.SqlServer code

## Setting Up GitHub Secrets

To publish packages to NuGet, you'll need to set up the following secret in your GitHub repository:

1. Navigate to your GitHub repository
2. Go to "Settings" -> "Secrets and variables" -> "Actions"
3. Click on "New repository secret"
4. Add the following secret:
   - Name: `NUGET_API_KEY`
   - Value: Your NuGet API key

### Obtaining a NuGet API Key

1. Sign in to [NuGet.org](https://www.nuget.org/)
2. Click on your username in the top right corner
3. Select "API Keys"
4. Click "Create" to generate a new API key
5. Provide a name for your key, e.g., "GitHub Actions"
6. Select the packages you want to publish or choose "All packages" (recommended for simplicity)
7. Set an expiration date for the key
8. Click "Create"
9. Copy the generated API key and store it in the GitHub secret

## Publishing Process

The workflow is configured to automatically:

1. Build and test the project
2. Create NuGet packages
3. Publish to NuGet when you create a release tag

### To publish a new version:

1. Update your code and ensure all tests pass
2. Create and push a new tag following semantic versioning:

```bash
git tag v1.2.3
git push origin v1.2.3
```

This will trigger the workflow to:
- Build the project with version 1.2.3
- Package it
- Publish to NuGet.org
- Create a GitHub release

## Manual Publishing

If you need to publish manually:

1. Navigate to the "Actions" tab in your GitHub repository
2. Select the "Build and Publish NuGet Package" workflow
3. Click "Run workflow"
4. Choose the branch to run from
5. Click "Run workflow"

Note: Manual runs will only publish to NuGet if triggered on a tag starting with `v`. 