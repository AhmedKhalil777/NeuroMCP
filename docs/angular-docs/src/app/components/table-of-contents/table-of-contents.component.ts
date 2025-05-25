import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-table-of-contents',
  template: `
    <div class="card sticky-top" style="top: 2rem;">
      <div class="card-header bg-dark text-white">
        <h5 class="mb-0">On This Page</h5>
      </div>
      <div class="card-body toc">
        <ul class="list-unstyled">
          <li *ngFor="let item of items" [style.paddingLeft.px]="(item.level - 1) * 12">
            <a [href]="'#' + item.id">{{ item.title }}</a>
          </li>
        </ul>
      </div>
    </div>
  `,
  styles: []
})
export class TableOfContentsComponent {
  @Input() items: { id: string, level: number, title: string }[] = [];
} 