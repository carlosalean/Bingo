import { Component } from '@angular/core';

@Component({
  selector: 'app-simple-test',
  standalone: true,
  template: `
    <div style="background: red; color: white; padding: 50px; margin: 20px; font-size: 24px; font-weight: bold;">
      <h1>🚨 SIMPLE TEST COMPONENT WORKS! 🚨</h1>
      <p>If you can see this, the routing is working!</p>
      <p>Current URL: {{ getCurrentUrl() }}</p>
      <p>Timestamp: {{ getTimestamp() }}</p>
    </div>
  `
})
export class SimpleTestComponent {
  getCurrentUrl() {
    return window.location.href;
  }
  
  getTimestamp() {
    return new Date().toISOString();
  }
}