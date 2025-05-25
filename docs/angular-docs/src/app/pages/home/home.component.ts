import { Component, OnInit } from '@angular/core';
import { DocumentationService, DocItem } from '../../services/documentation.service';

@Component({
  selector: 'app-home',
  template: `
    <!-- Hero Section -->
    <header class="hero text-center">
      <div class="container">
        <h1 class="display-4 fw-bold mb-4">NeuroMCP</h1>
        <p class="lead mb-4">Neural Model Context Protocol - Empowering AI agents with standardized backend integrations</p>
        <div>
          <a href="#getting-started" class="btn btn-light btn-lg me-2">Get Started</a>
          <a href="https://github.com/ahmedkhaled/NeuroMCP" class="btn btn-outline-light btn-lg" target="_blank">View on GitHub</a>
        </div>
      </div>
    </header>

    <!-- About Section -->
    <section class="py-5">
      <div class="container">
        <div class="row">
          <div class="col-lg-8 mx-auto text-center">
            <h2 class="mb-4">What is NeuroMCP?</h2>
            <p class="lead">
              NeuroMCP is a collection of specialized microservices that enable AI agents to interact with
              various backend systems through a standardized API.
            </p>
            <p>
              With NeuroMCP, AI applications can seamlessly access databases, DevOps tools, and other
              enterprise systems through a unified API pattern, greatly reducing integration complexity.
            </p>
          </div>
        </div>
      </div>
    </section>

    <!-- Services Section -->
    <section id="services" class="py-5 bg-light">
      <div class="container">
        <h2 class="text-center mb-5">Available Services</h2>
        <div class="row g-4">
          <div class="col-md-6">
            <div class="card feature-card p-4">
              <img src="assets/images/sqlserver-logo.png" alt="SQL Server" class="service-logo">
              <div class="card-body text-center">
                <h3 class="card-title">NeuroMCP.SqlServer</h3>
                <p class="card-text">
                  Enable AI agents to execute SQL queries, explore database schemas, and perform database
                  operations on Microsoft SQL Server.
                </p>
                <a [routerLink]="['/docs', 'sql-server']" class="btn btn-primary">Learn More</a>
              </div>
            </div>
          </div>
          <div class="col-md-6">
            <div class="card feature-card p-4">
              <img src="assets/images/azuredevops-logo.png" alt="Azure DevOps" class="service-logo">
              <div class="card-body text-center">
                <h3 class="card-title">NeuroMCP.AzureDevOps</h3>
                <p class="card-text">
                  Allow AI agents to interact with Azure DevOps repositories, work items, pipelines, and
                  more through a clean API.
                </p>
                <a [routerLink]="['/docs', 'azure-devops']" class="btn btn-primary">Learn More</a>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- Getting Started Section -->
    <section id="getting-started" class="py-5">
      <div class="container">
        <h2 class="text-center mb-5">Getting Started</h2>
        <div class="row">
          <div class="col-lg-10 mx-auto">
            <div class="card mb-4">
              <div class="card-header bg-dark text-white">
                <h5 class="mb-0">Docker Installation</h5>
              </div>
              <div class="card-body">
                <p>The easiest way to get started is using Docker:</p>
                <div class="bg-light p-3 rounded mb-3">
                  <pre class="mb-0"><code># SQL Server Service
docker pull ahmedkhalil777/neuromcp-sqlserver:latest
docker run -p 5200:5200 ahmedkhalil777/neuromcp-sqlserver

# Azure DevOps Service
docker pull ahmedkhalil777/neuromcp-azuredevops:latest
docker run -p 5300:5300 ahmedkhalil777/neuromcp-azuredevops</code></pre>
                </div>
                <a [routerLink]="['/docs', 'docker-installation']" class="btn btn-primary">Full Installation Guide</a>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- Documentation Section -->
    <section id="documentation" class="py-5 bg-light">
      <div class="container">
        <h2 class="text-center mb-5">Documentation</h2>
        <div class="row g-4">
          <ng-container *ngFor="let category of categories">
            <div class="col-md-4">
              <div class="card h-100">
                <div class="card-body">
                  <h5 class="card-title">{{ category }}</h5>
                  <ul class="list-unstyled">
                    <li *ngFor="let doc of getDocsByCategory(category)">
                      <a [routerLink]="['/docs', doc.id]">{{ doc.title }}</a>
                    </li>
                  </ul>
                </div>
              </div>
            </div>
          </ng-container>
        </div>
      </div>
    </section>
  `
})
export class HomeComponent implements OnInit {
  categories: string[] = [];
  
  constructor(private docService: DocumentationService) {}
  
  ngOnInit(): void {
    this.categories = this.docService.getCategories();
  }
  
  getDocsByCategory(category: string): DocItem[] {
    return this.docService.getDocsByCategory(category);
  }
} 