# NeuroMCP Documentation

Welcome to the NeuroMCP documentation. This guide provides comprehensive information about installing, configuring, and using the NeuroMCP services.

## Table of Contents

- [Overview](#overview)
- [Services](#services)
- [Installation](#installation)
- [Configuration](#configuration)
- [Docker](#docker)
- [Development](#development)
- [Examples](#examples)
- [Troubleshooting](#troubleshooting)

## Overview

NeuroMCP is a collection of Model Context Protocol (MCP) services that enable AI agents to interact with various backend systems through a standardized API. Each service acts as a bridge between AI models and specific technologies, allowing for seamless integration.

## Services

NeuroMCP currently includes the following services:

- [NeuroMCP.AzureDevOps](azure-devops.md) - Integrates with Azure DevOps for work items, repositories, and more
- [NeuroMCP.SqlServer](sql-server.md) - Provides SQL Server database access and query execution capabilities

## Installation

There are multiple ways to install and run NeuroMCP services:

- [Docker Installation](docker-installation.md) - Run services using Docker containers
- [Local Installation](local-installation.md) - Install services locally on your machine
- [Server Installation](server-installation.md) - Deploy services on a server

## Configuration

- [AzureDevOps Configuration](azuredevops-configuration.md) - Configure Azure DevOps connection settings
- [SQL Server Configuration](sqlserver-configuration.md) - Configure SQL Server connection settings
- [Security Best Practices](security.md) - Security recommendations for production deployments

## Docker

NeuroMCP services are available as Docker images:

- [Docker Hub](docker-hub.md) - Information about Docker Hub repositories
- [Docker Compose](docker-compose.md) - Run multiple services with Docker Compose
- [Docker Configuration](docker-configuration.md) - Configure Docker containers

## Development

- [Architecture](architecture.md) - System architecture and design
- [Contributing](contributing.md) - Guide for contributing to NeuroMCP
- [Building from Source](building.md) - Build NeuroMCP from source code

## Examples

- [AzureDevOps Examples](azuredevops-examples.md) - Examples of using the Azure DevOps service
- [SQL Server Examples](sqlserver-examples.md) - Examples of using the SQL Server service

## Troubleshooting

- [Common Issues](common-issues.md) - Solutions to common problems
- [Logging](logging.md) - How to use logging for troubleshooting
- [FAQ](faq.md) - Frequently asked questions 