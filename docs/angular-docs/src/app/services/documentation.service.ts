import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';

export interface DocItem {
  id: string;
  title: string;
  category: string;
  path: string;
}

@Injectable({
  providedIn: 'root'
})
export class DocumentationService {
  private docsBaseUrl = 'assets/docs/';
  
  // Document registry - maps document IDs to paths and metadata
  private docRegistry: DocItem[] = [
    {
      id: 'sql-server',
      title: 'SQL Server Service',
      category: 'Services',
      path: 'sql-server.md'
    },
    {
      id: 'azure-devops',
      title: 'Azure DevOps Service',
      category: 'Services',
      path: 'azure-devops.md'
    },
    {
      id: 'architecture',
      title: 'Architecture Overview',
      category: 'Architecture',
      path: 'architecture.md'
    },
    {
      id: 'security',
      title: 'Security Best Practices',
      category: 'Architecture',
      path: 'security.md'
    },
    {
      id: 'docker-installation',
      title: 'Docker Installation',
      category: 'Deployment',
      path: 'docker-installation.md'
    },
    {
      id: 'docker-compose',
      title: 'Docker Compose Guide',
      category: 'Deployment',
      path: 'docker-compose.md'
    },
    {
      id: 'docker-hub',
      title: 'Docker Hub Information',
      category: 'Deployment',
      path: 'docker-hub.md'
    }
  ];

  constructor(private http: HttpClient) { }

  /**
   * Get all available documentation items
   */
  getAllDocs(): DocItem[] {
    return this.docRegistry;
  }

  /**
   * Get docs by category
   */
  getDocsByCategory(category: string): DocItem[] {
    return this.docRegistry.filter(doc => doc.category === category);
  }

  /**
   * Get unique categories
   */
  getCategories(): string[] {
    return [...new Set(this.docRegistry.map(doc => doc.category))];
  }

  /**
   * Get doc by ID
   */
  getDocById(id: string): DocItem | undefined {
    return this.docRegistry.find(doc => doc.id === id);
  }

  /**
   * Load markdown content for a document
   */
  getDocContent(id: string): Observable<string> {
    const doc = this.getDocById(id);
    
    if (!doc) {
      return of('# Document Not Found\n\nThe requested document could not be found.');
    }
    
    return this.http.get(`${this.docsBaseUrl}${doc.path}`, { responseType: 'text' })
      .pipe(
        catchError(() => {
          return of('# Error Loading Document\n\nThe requested document could not be loaded.');
        })
      );
  }

  /**
   * Extract table of contents from markdown content
   */
  extractTableOfContents(markdown: string): { id: string, level: number, title: string }[] {
    const headingRegex = /^(#{1,6})\s+(.+)$/gm;
    const toc: { id: string, level: number, title: string }[] = [];
    let match;

    while ((match = headingRegex.exec(markdown)) !== null) {
      const level = match[1].length;
      const title = match[2].trim();
      const id = this.slugify(title);
      
      toc.push({ id, level, title });
    }

    return toc;
  }

  /**
   * Create URL-friendly slug from string
   */
  private slugify(text: string): string {
    return text
      .toLowerCase()
      .replace(/[^\w\s-]/g, '')
      .replace(/[\s_-]+/g, '-')
      .replace(/^-+|-+$/g, '');
  }
} 