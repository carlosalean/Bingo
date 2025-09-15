import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ApiService, RoomDto } from '../../services/api.service';
import { Observable, of, Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged, switchMap, catchError } from 'rxjs/operators';

interface RoomInfo {
  id: string;
  name: string;
  bingoType: string;
  maxPlayers: number;
  currentPlayers: number;
  isPrivate: boolean;
  status: string;
}

@Component({
  selector: 'app-join-room',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule
  ],
  templateUrl: './join-room.component.html',
  styleUrls: ['./join-room.component.scss']
})
export class JoinRoomComponent implements OnInit {
  joinRoomForm!: FormGroup;
  isLoading = false;
  roomInfo: RoomInfo | null = null;
  errorMessage: string = '';
  publicRooms$: Observable<RoomDto[]>;
  
  private roomCodeSubject = new Subject<string>();

  constructor(
    private fb: FormBuilder,
    private apiService: ApiService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {
    this.publicRooms$ = this.apiService.getPublicRooms();
  }

  ngOnInit(): void {
    this.initializeForm();
    this.setupRoomCodeValidation();
  }

  private initializeForm(): void {
    this.joinRoomForm = this.fb.group({
      roomCode: ['', [
        Validators.required,
        Validators.minLength(4),
        Validators.pattern(/^[A-Za-z0-9]+$/)
      ]]
    });
  }

  private setupRoomCodeValidation(): void {
    this.roomCodeSubject.pipe(
      debounceTime(500),
      distinctUntilChanged(),
      switchMap(code => {
        if (code && code.length >= 4) {
          return this.apiService.getRoomByCode(code).pipe(
            catchError(error => {
              this.errorMessage = 'Sala no encontrada o código inválido';
              this.roomInfo = null;
              return of(null);
            })
          );
        }
        return of(null);
      })
    ).subscribe(room => {
      if (room) {
        this.roomInfo = {
          id: room.id,
          name: room.name,
          bingoType: room.bingoType,
          maxPlayers: room.maxPlayers,
          currentPlayers: room.currentPlayers || 0,
          isPrivate: room.isPrivate,
          status: room.status || 'Esperando jugadores'
        };
        this.errorMessage = '';
      }
    });
  }

  onRoomCodeChange(event: any): void {
    const value = event.target.value.toUpperCase();
    event.target.value = value;
    this.joinRoomForm.patchValue({ roomCode: value });
    
    if (value.length >= 4) {
      this.roomCodeSubject.next(value);
    } else {
      this.roomInfo = null;
      this.errorMessage = '';
    }
  }

  onSubmit(): void {
    if (this.joinRoomForm.valid && !this.isLoading) {
      this.isLoading = true;
      const roomCode = this.joinRoomForm.value.roomCode;

      this.apiService.joinRoom(roomCode).subscribe({
        next: (response) => {
          this.isLoading = false;
          this.snackBar.open('¡Te has unido a la sala exitosamente!', 'Cerrar', {
            duration: 3000,
            panelClass: ['success-snackbar']
          });
          
          // Navigate to the game room
          if (response.roomId) {
            this.router.navigate(['/game', response.roomId]);
          } else {
            this.router.navigate(['/dashboard']);
          }
        },
        error: (error) => {
          this.isLoading = false;
          console.error('Error joining room:', error);
          
          let errorMessage = 'Error al unirse a la sala';
          if (error.error?.message) {
            errorMessage = error.error.message;
          } else if (error.status === 404) {
            errorMessage = 'Sala no encontrada';
          } else if (error.status === 400) {
            errorMessage = 'La sala está llena o no disponible';
          }
          
          this.snackBar.open(errorMessage, 'Cerrar', {
            duration: 5000,
            panelClass: ['error-snackbar']
          });
        }
      });
    } else {
      this.markFormGroupTouched();
    }
  }

  joinPublicRoom(roomId: string): void {
    if (this.isLoading) return;
    
    this.isLoading = true;
    
    this.apiService.joinRoomById(roomId).subscribe({
      next: (response) => {
        this.isLoading = false;
        this.snackBar.open('¡Te has unido a la sala exitosamente!', 'Cerrar', {
          duration: 3000,
          panelClass: ['success-snackbar']
        });
        
        this.router.navigate(['/game', roomId]);
      },
      error: (error) => {
        this.isLoading = false;
        console.error('Error joining public room:', error);
        
        let errorMessage = 'Error al unirse a la sala';
        if (error.error?.message) {
          errorMessage = error.error.message;
        } else if (error.status === 400) {
          errorMessage = 'La sala está llena o no disponible';
        }
        
        this.snackBar.open(errorMessage, 'Cerrar', {
          duration: 5000,
          panelClass: ['error-snackbar']
        });
      }
    });
  }

  private markFormGroupTouched(): void {
    Object.keys(this.joinRoomForm.controls).forEach(key => {
      const control = this.joinRoomForm.get(key);
      control?.markAsTouched();
    });
  }

  goBack(): void {
    this.router.navigate(['/dashboard']);
  }

  // Getter methods for easy access in template
  get roomCodeControl() {
    return this.joinRoomForm.get('roomCode');
  }
}