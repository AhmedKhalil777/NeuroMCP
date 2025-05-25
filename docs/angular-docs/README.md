# NeuroMCP Documentation Angular App

This is an Angular-based documentation website for the NeuroMCP project. It dynamically renders Markdown documents from the `docs` directory.

## Setup and Development

1. Install dependencies:
   ```
   npm install
   ```

2. Start the development server:
   ```
   npm start
   ```

3. Build for production:
   ```
   npm run build
   ```

## Documentation Structure

The application loads the Markdown documentation files from `assets/docs/` at runtime. The files are copied to this directory during the build process as configured in `angular.json`.

## Features

- Dynamic rendering of Markdown content
- Automatically generated table of contents from Markdown headings
- Syntax highlighting for code blocks
- Responsive layout with Bootstrap
- Organized documentation by categories

## Adding New Documentation

1. Add your Markdown file to the `docs` directory
2. Update the document registry in `src/app/services/documentation.service.ts` with the new document information

## Deployment

The application is automatically built and deployed to GitHub Pages via the GitHub Actions workflow defined in `.github/workflows/github-pages.yml`. 