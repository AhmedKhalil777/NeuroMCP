import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { DocumentationService, DocItem } from '../../services/documentation.service';
import { switchMap, tap } from 'rxjs/operators';

@Component({
  selector: 'app-documentation',
  template: `
    <!-- Header -->
    <header class="doc-header text-center" *ngIf="document">
      <div class="container">
        <img [src]="getImagePath()" alt="{{ document.title }}" style="width: 120px; margin-bottom: 1rem;">
        <h1 class="display-5 fw-bold">{{ document.title }}</h1>
        <p class="lead">{{ getDescription() }}</p>
      </div>
    </header>

    <!-- Content -->
    <div class="container py-5">
      <div class="row">
        <div class="col-lg-9">
          <app-markdown-content [content]="content" [generateToc]="true" #markdownContent></app-markdown-content>
        </div>
        <div class="col-lg-3">
          <app-table-of-contents [items]="markdownContent.toc"></app-table-of-contents>
        </div>
      </div>
    </div>
  `
})
export class DocumentationComponent implements OnInit {
  document: DocItem | undefined;
  content: string = '';
  
  constructor(
    private route: ActivatedRoute,
    private docService: DocumentationService,
    private titleService: Title
  ) {}
  
  ngOnInit(): void {
    this.route.params.pipe(
      switchMap(params => {
        const docId = params['id'];
        this.document = this.docService.getDocById(docId);
        
        if (this.document) {
          this.titleService.setTitle(`${this.document.title} | NeuroMCP Documentation`);
        }
        
        return this.docService.getDocContent(docId);
      }),
      tap(content => {
        this.content = content;
      })
    ).subscribe();
  }
  
  getImagePath(): string {
    if (!this.document) return '';
    
    if (this.document.id === 'sql-server') {
      return 'assets/images/sqlserver-logo.png';
    } else if (this.document.id === 'azure-devops') {
      return 'assets/images/azuredevops-logo.png';
    }
    
    // Default image based on category
    const category = this.document.category.toLowerCase();
    if (category === 'architecture') {
      return 'assets/images/architecture-icon.png';
    } else if (category === 'deployment') {
      return 'assets/images/docker-icon.png';
    }
    
    return 'assets/images/document-icon.png';
  }
  
  getDescription(): string {
    if (!this.document) return '';
    
    switch (this.document.id) {
      case 'sql-server':
        return 'A Model Context Protocol (MCP) server for interacting with Microsoft SQL Server databases';
      case 'azure-devops':
        return 'Connect your AI agents to Azure DevOps for work item, repository, and pipeline management';
      case 'architecture':
        return 'Understanding the NeuroMCP architecture and design principles';
      case 'security':
        return 'Best practices for securing your NeuroMCP services';
      case 'docker-installation':
        return 'Step-by-step guide to installing NeuroMCP using Docker';
      case 'docker-compose':
        return 'Running multiple NeuroMCP services with Docker Compose';
      default:
        return `Documentation for ${this.document.title}`;
    }
  }
} 