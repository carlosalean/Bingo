import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatRadioModule } from '@angular/material/radio';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ApiService, RoomCreateDto } from '../../services/api.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-create-room',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatRadioModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule
  ],
  templateUrl: './create-room.component.html',
  styleUrls: ['./create-room.component.scss']
})
export class CreateRoomComponent implements OnInit {
  createRoomForm!: FormGroup;
  isLoading = false;

  constructor(
    private fb: FormBuilder,
    private apiService: ApiService,
    private authService: AuthService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.initializeForm();
  }

  private initializeForm(): void {
    this.createRoomForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(3)]],
      BingoType: ['SeventyFive', Validators.required],
      maxPlayers: [6, [Validators.required, Validators.min(2), Validators.max(20)]],
      isPrivate: [false, Validators.required]
    });
  }

  onSubmit(): void {
    if (this.createRoomForm.valid && !this.isLoading) {
      // Verificar autenticación antes de proceder
      if (!this.authService.isLoggedIn() || !this.authService.isTokenValid()) {
        this.snackBar.open('Tu sesión ha expirado. Por favor, inicia sesión nuevamente.', 'Cerrar', {
          duration: 5000,
          panelClass: ['error-snackbar']
        });
        this.authService.logout();
        return;
      }

      this.isLoading = true;
      
      const roomData: RoomCreateDto = {
        name: this.createRoomForm.value.name,
        BingoType: this.createRoomForm.value.BingoType,
        maxPlayers: parseInt(this.createRoomForm.value.maxPlayers),
        isPrivate: this.createRoomForm.value.isPrivate
      };

      this.apiService.postCreateRoom(roomData).subscribe({
        next: (response) => {
          this.isLoading = false;
          this.snackBar.open('¡Sala creada exitosamente!', 'Cerrar', {
            duration: 3000,
            panelClass: ['success-snackbar']
          });
          
          // Navigate to the created room or back to dashboard
          if (response.id) {
            this.router.navigate(['/game', response.id]);
          } else {
            this.router.navigate(['/dashboard']);
          }
        },
        error: (error) => {
          this.isLoading = false;
          console.error('Error creating room:', error);
          
          let errorMessage = 'Error al crear la sala';
          
          if (error.status === 401) {
            errorMessage = 'Tu sesión ha expirado. Por favor, inicia sesión nuevamente.';
            // El interceptor ya maneja el logout automáticamente
          } else if (error.status === 400) {
            if (error.error?.errors) {
              const validationErrors = Object.values(error.error.errors).flat();
              errorMessage = validationErrors.join(', ');
            } else if (error.error?.message) {
              errorMessage = error.error.message;
            }
          } else if (error.error?.message) {
            errorMessage = error.error.message;
          } else if (error.message) {
            errorMessage = error.message;
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

  private markFormGroupTouched(): void {
    Object.keys(this.createRoomForm.controls).forEach(key => {
      const control = this.createRoomForm.get(key);
      control?.markAsTouched();
    });
  }

  goBack(): void {
    this.router.navigate(['/dashboard']);
  }

  // Getter methods for easy access in template
  get nameControl() {
    return this.createRoomForm.get('name');
  }

  get bingoTypeControl() {
    return this.createRoomForm.get('BingoType');
  }

  get maxPlayersControl() {
    return this.createRoomForm.get('maxPlayers');
  }

  get isPrivateControl() {
    return this.createRoomForm.get('isPrivate');
  }
}