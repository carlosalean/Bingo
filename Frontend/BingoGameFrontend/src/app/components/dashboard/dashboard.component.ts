import { Component, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ApiService } from '../../services/api.service';
import { RoomDto } from '../../services/api.service';
import { Observable } from 'rxjs';
import { AuthService } from '../../services/auth.service';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatButtonModule,
    MatCardModule,
    MatSnackBarModule,
    MatListModule,
    MatIconModule,
    MatProgressSpinnerModule
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent {
  rooms$: Observable<RoomDto[]>;

  constructor(
    private apiService: ApiService,
    private authService: AuthService,
    private snackBar: MatSnackBar,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {
    this.rooms$ = this.apiService.getUserRooms();
  }

  logout() {
    this.authService.logout();
  }

  createRoom() {
    this.router.navigate(['/create-room']);
  }

  joinRoom() {
    this.router.navigate(['/join-room']);
  }

  enterRoom(roomId: string) {
    this.router.navigate(['/game', roomId]);
  }

  deleteRoom(roomId: string) {
    this.apiService.deleteRoom(roomId).subscribe({
      next: () => {
        this.snackBar.open('Sala eliminada correctamente', 'Cerrar', { duration: 3000 });
        this.rooms$ = this.apiService.getUserRooms();
        this.cdr.detectChanges();
      },
      error: (err: any) => {
        this.snackBar.open('Error al eliminar la sala', 'Cerrar', { duration: 3000 });
      }
    });
  }
}