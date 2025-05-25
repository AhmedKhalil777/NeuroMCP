import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { DocumentationService } from '../../services/documentation.service';

@Component({
  selector: 'app-markdown-content',
  template: `
    <div class="markdown-content">
      <markdown [data]="content" ngPreserveWhitespaces></markdown>
    </div>
  `,
  styles: []
})
export class MarkdownContentComponent implements OnChanges {
  @Input() content: string = '';
  @Input() generateToc: boolean = false;
  
  tableOfContents: { id: string, level: number, title: string }[] = [];
  
  constructor(private docService: DocumentationService) {}
  
  ngOnChanges(changes: SimpleChanges): void {
    if (changes['content'] && this.generateToc) {
      this.tableOfContents = this.docService.extractTableOfContents(this.content);
    }
  }
  
  get toc(): { id: string, level: number, title: string }[] {
    return this.tableOfContents;
  }
} 