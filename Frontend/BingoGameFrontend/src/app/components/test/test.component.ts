import { Component } from '@angular/core';

@Component({
  selector: 'app-test',
  standalone: true,
  template: `
    <div style="padding: 20px; background: lightblue; margin: 20px;">
      <h1>Test Component Works!</h1>
      <p>This is a test component to verify routing is working.</p>
      <p>Current URL: {{ getCurrentUrl() }}</p>
    </div>
  `
})
export class TestComponent {
  getCurrentUrl() {
    return window.location.href;
  }
}