import { Component } from '@angular/core';

@Component({
  selector: 'app-footer',
  template: `
    <footer class="text-center">
      <div class="container">
        <div class="mb-4">
          <a href="https://github.com/ahmedkhaled/NeuroMCP" class="text-white me-3" target="_blank">
            <i class="fab fa-github"></i> GitHub
          </a>
          <a href="https://hub.docker.com/r/ahmedkhalil777/neuromcp-sqlserver" class="text-white me-3" target="_blank">
            <i class="fab fa-docker"></i> Docker Hub
          </a>
        </div>
        <p>NeuroMCP is licensed under the MIT License</p>
      </div>
    </footer>
  `,
  styles: []
})
export class FooterComponent {} 