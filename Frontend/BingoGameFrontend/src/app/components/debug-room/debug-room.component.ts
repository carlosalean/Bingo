import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { ApiService } from '../../services/api.service';
import { AuthService } from '../../services/auth.service';
import { SignalRService } from '../../services/signalr.service';

@Component({
  selector: 'app-debug-room',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="p-8 bg-white min-h-screen">
      <h1 class="text-3xl font-bold mb-6 text-black">Debug Room Component</h1>
      
      <div class="space-y-4 text-black">
        <div class="bg-gray-100 p-4 rounded">
          <h2 class="text-xl font-semibold mb-2">Route Information</h2>
          <p><strong>Room ID:</strong> {{ roomId }}</p>
          <p><strong>Current URL:</strong> {{ getCurrentUrl() }}</p>
        </div>
        
        <div class="bg-blue-100 p-4 rounded">
          <h2 class="text-xl font-semibold mb-2">Authentication</h2>
          <p><strong>Is Logged In:</strong> {{ isLoggedIn }}</p>
          <p><strong>User ID:</strong> {{ userId }}</p>
          <p><strong>Token Present:</strong> {{ hasToken }}</p>
        </div>
        
        <div class="bg-green-100 p-4 rounded">
          <h2 class="text-xl font-semibold mb-2">API Connection</h2>
          <p><strong>Room Data Status:</strong> {{ roomDataStatus }}</p>
          <p><strong>Room Code:</strong> {{ roomCode }}</p>
          <p><strong>Room Type:</strong> {{ roomType }}</p>
        </div>
        
        <div class="bg-yellow-100 p-4 rounded">
          <h2 class="text-xl font-semibold mb-2">SignalR Connection</h2>
          <p><strong>Connection Status:</strong> {{ signalRStatus }}</p>
          <p><strong>Connection State:</strong> {{ connectionState }}</p>
        </div>
        
        <div class="bg-purple-100 p-4 rounded">
          <h2 class="text-xl font-semibold mb-2">Component State</h2>
          <p><strong>Component Loaded:</strong> {{ componentLoaded }}</p>
          <p><strong>Timestamp:</strong> {{ getCurrentTime() }}</p>
        </div>
        
        <div class="bg-red-100 p-4 rounded" *ngIf="errors.length > 0">
          <h2 class="text-xl font-semibold mb-2">Errors</h2>
          <ul>
            <li *ngFor="let error of errors" class="text-red-700">{{ error }}</li>
          </ul>
        </div>
      </div>
    </div>
  `,
  styles: []
})
export class DebugRoomComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private apiService = inject(ApiService);
  private authService = inject(AuthService);
  private signalRService = inject(SignalRService);

  roomId: string = '';
  isLoggedIn: boolean = false;
  userId: string = '';
  hasToken: boolean = false;
  roomDataStatus: string = 'Not loaded';
  roomCode: string = '';
  roomType: string = '';
  signalRStatus: string = 'Not connected';
  connectionState: string = '';
  componentLoaded: boolean = false;
  errors: string[] = [];

  ngOnInit(): void {
    console.log('DebugRoomComponent ngOnInit started');
    
    try {
      // Get route information
      this.roomId = this.route.snapshot.paramMap.get('roomId') || '';
      console.log('Room ID from route:', this.roomId);
      
      // Check authentication
      this.isLoggedIn = this.authService.isLoggedIn();
      this.userId = this.authService.getCurrentUserId() || '';
      this.hasToken = !!this.authService.getToken();
      
      console.log('Auth status:', { isLoggedIn: this.isLoggedIn, userId: this.userId, hasToken: this.hasToken });
      
      // Test API connection
      if (this.roomId) {
        this.loadRoomData();
      }
      
      // Test SignalR connection
      this.testSignalRConnection();
      
      this.componentLoaded = true;
      console.log('DebugRoomComponent loaded successfully');
      
    } catch (error) {
      console.error('Error in DebugRoomComponent ngOnInit:', error);
      this.errors.push(`Initialization error: ${error}`);
    }
  }
  
  private loadRoomData(): void {
    this.roomDataStatus = 'Loading...';
    
    this.apiService.getRooms().subscribe({
      next: (rooms: any) => {
        console.log('Rooms data loaded:', rooms);
        this.roomDataStatus = 'Loaded successfully (using getRooms)';
        this.roomCode = 'N/A (using getRooms endpoint)';
        this.roomType = 'N/A (using getRooms endpoint)';
      },
      error: (error: any) => {
        console.error('Error loading rooms data:', error);
        this.roomDataStatus = `Error: ${error.message || 'Unknown error'}`;
        this.errors.push(`API Error: ${error.message || 'Unknown error'}`);
      }
    });
  }
  
  private testSignalRConnection(): void {
    this.signalRStatus = 'Connecting...';
    
    try {
      this.signalRService.start(this.roomId).then(() => {
        console.log('SignalR connected successfully');
        this.signalRStatus = 'Connected successfully';
        this.connectionState = 'Connected';
        
        // Try to join room
        return this.signalRService.joinRoom(this.roomId);
      }).then(() => {
        console.log('Joined room successfully');
        this.signalRStatus = 'Connected and joined room';
      }).catch((error) => {
        console.error('SignalR error:', error);
        this.signalRStatus = `Error: ${error.message || 'Connection failed'}`;
        this.errors.push(`SignalR Error: ${error.message || 'Connection failed'}`);
      });
    } catch (error) {
      console.error('SignalR initialization error:', error);
      this.signalRStatus = `Initialization error: ${error}`;
      this.errors.push(`SignalR Init Error: ${error}`);
    }
  }
  
  getCurrentUrl(): string {
    return window.location.href;
  }
  
  getCurrentTime(): string {
    return new Date().toLocaleString();
  }
}