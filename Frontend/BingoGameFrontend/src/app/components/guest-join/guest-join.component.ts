import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { InvitationService, AcceptInvitationDto, InvitationDto } from '../../services/invitation.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-guest-join',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatInputModule,
    MatFormFieldModule,
    MatProgressSpinnerModule,
    MatSnackBarModule
  ],
  templateUrl: './guest-join.component.html',
  styleUrls: ['./guest-join.component.scss']
})
export class GuestJoinComponent implements OnInit {
  invitationId: string = '';
  invitation: InvitationDto | null = null;
  guestName: string = '';
  isLoading: boolean = false;
  isValidating: boolean = true;
  isInvitationValid: boolean = false;
  errorMessage: string = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private invitationService: InvitationService,
    private authService: AuthService,
    private snackBar: MatSnackBar
  ) { }

  ngOnInit(): void {
    this.invitationId = this.route.snapshot.paramMap.get('id') || '';
    if (this.invitationId) {
      this.validateInvitation();
    } else {
      this.errorMessage = 'ID de invitación no válido';
      this.isValidating = false;
    }
  }

  private validateInvitation(): void {
    this.isValidating = true;
    
    // Primero verificar si la invitación es válida
    this.invitationService.isInvitationValid(this.invitationId).subscribe({
      next: (isValid) => {
        if (isValid) {
          // Si es válida, obtener los detalles
          this.loadInvitationDetails();
        } else {
          this.errorMessage = 'La invitación ha expirado o ya ha sido utilizada';
          this.isValidating = false;
          this.isInvitationValid = false;
        }
      },
      error: (error) => {
        console.error('Error validating invitation:', error);
        this.errorMessage = 'Error al validar la invitación';
        this.isValidating = false;
        this.isInvitationValid = false;
      }
    });
  }

  private loadInvitationDetails(): void {
    this.invitationService.getInvitationById(this.invitationId).subscribe({
      next: (invitation) => {
        this.invitation = invitation;
        this.isInvitationValid = true;
        this.isValidating = false;
      },
      error: (error) => {
        console.error('Error loading invitation details:', error);
        this.errorMessage = 'Error al cargar los detalles de la invitación';
        this.isValidating = false;
        this.isInvitationValid = false;
      }
    });
  }

  joinRoom(): void {
    if (!this.guestName.trim()) {
      this.showMessage('Por favor ingresa tu nombre', 'error');
      return;
    }

    if (!this.invitationId) {
      this.showMessage('ID de invitación no válido', 'error');
      return;
    }

    this.isLoading = true;

    const acceptData: AcceptInvitationDto = {
      invitationId: this.invitationId,
      guestName: this.guestName.trim()
    };

    this.invitationService.acceptInvitation(acceptData).subscribe({
      next: (tokenResponse) => {
        // Guardar el token y la información del usuario
        localStorage.setItem('token', tokenResponse.token);
        localStorage.setItem('user', JSON.stringify(tokenResponse.user));
        
        this.showMessage('¡Bienvenido! Uniéndote a la sala...', 'success');
        
        // Redirigir a la sala de juego
        setTimeout(() => {
          this.router.navigate(['/game', this.invitation?.roomId]);
        }, 1500);
      },
      error: (error) => {
        console.error('Error accepting invitation:', error);
        let errorMessage = 'Error al unirse a la sala';
        if (error.error?.message) {
          errorMessage = error.error.message;
        }
        this.showMessage(errorMessage, 'error');
        this.isLoading = false;
      }
    });
  }

  private showMessage(message: string, type: 'success' | 'error'): void {
    this.snackBar.open(message, 'Cerrar', {
      duration: 3000,
      panelClass: type === 'success' ? ['success-snackbar'] : ['error-snackbar']
    });
  }

  isValidName(name: string): boolean {
    return name.trim().length >= 2 && name.trim().length <= 50;
  }

  goHome(): void {
    this.router.navigate(['/']);
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleString('es-ES', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }
}