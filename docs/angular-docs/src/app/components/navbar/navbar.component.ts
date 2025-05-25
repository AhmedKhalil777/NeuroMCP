import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { DocumentationService, DocItem } from '../../services/documentation.service';

@Component({
  selector: 'app-navbar',
  template: `
    <nav class="navbar navbar-expand-lg navbar-dark bg-dark">
      <div class="container">
        <a class="navbar-brand" routerLink="/">NeuroMCP</a>
        <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarNav">
          <span class="navbar-toggler-icon"></span>
        </button>
        <div class="collapse navbar-collapse" id="navbarNav">
          <ul class="navbar-nav ms-auto">
            <li class="nav-item">
              <a class="nav-link" routerLink="/" routerLinkActive="active" [routerLinkActiveOptions]="{exact: true}">Home</a>
            </li>
            <li class="nav-item dropdown">
              <a class="nav-link dropdown-toggle" href="#" role="button" data-bs-toggle="dropdown">
                Documentation
              </a>
              <ul class="dropdown-menu">
                <ng-container *ngFor="let category of categories">
                  <li><h6 class="dropdown-header">{{ category }}</h6></li>
                  <ng-container *ngFor="let doc of getDocsByCategory(category)">
                    <li>
                      <a class="dropdown-item" [routerLink]="['/docs', doc.id]">{{ doc.title }}</a>
                    </li>
                  </ng-container>
                  <li><hr class="dropdown-divider" *ngIf="category !== categories[categories.length-1]"></li>
                </ng-container>
              </ul>
            </li>
            <li class="nav-item">
              <a class="nav-link" href="https://github.com/ahmedkhaled/NeuroMCP" target="_blank">
                <i class="fab fa-github"></i> GitHub
              </a>
            </li>
          </ul>
        </div>
      </div>
    </nav>
  `,
  styles: [`
    .dropdown-menu {
      min-width: 240px;
    }
    .dropdown-header {
      font-weight: bold;
      color: var(--primary-color);
    }
  `]
})
export class NavbarComponent {
  categories: string[] = [];

  constructor(
    private docService: DocumentationService, 
    private router: Router
  ) {
    this.categories = this.docService.getCategories();
  }

  getDocsByCategory(category: string): DocItem[] {
    return this.docService.getDocsByCategory(category);
  }
} 