import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatTableModule } from '@angular/material/table';
import { MatChipsModule } from '@angular/material/chips';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { InvitationService, CreateInvitationDto, InvitationDto } from '../../services/invitation.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-invitation-manager',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatInputModule,
    MatFormFieldModule,
    MatTableModule,
    MatChipsModule,
    MatSnackBarModule,
    MatDialogModule
  ],
  templateUrl: './invitation-manager.component.html',
  styleUrls: ['./invitation-manager.component.scss']
})
export class InvitationManagerComponent implements OnInit, OnDestroy {
  @Input() roomId: string = '';
  
  invitations: InvitationDto[] = [];
  newInvitationEmail: string = '';
  isLoading: boolean = false;
  displayedColumns: string[] = ['email', 'status', 'createdAt', 'expiresAt', 'actions'];
  
  private subscription: Subscription = new Subscription();

  constructor(
    private invitationService: InvitationService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog
  ) { }

  ngOnInit(): void {
    if (this.roomId) {
      this.loadInvitations();
    }
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

  loadInvitations(): void {
    if (!this.roomId) return;
    
    this.isLoading = true;
    const sub = this.invitationService.getRoomInvitations(this.roomId).subscribe({
      next: (invitations) => {
        this.invitations = invitations;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading invitations:', error);
        this.showMessage('Error al cargar las invitaciones', 'error');
        this.isLoading = false;
      }
    });
    this.subscription.add(sub);
  }

  sendInvitation(): void {
    if (!this.newInvitationEmail.trim() || !this.roomId) {
      this.showMessage('Por favor ingresa un email válido', 'error');
      return;
    }

    const invitation: CreateInvitationDto = {
      email: this.newInvitationEmail.trim(),
      roomId: this.roomId
    };

    this.isLoading = true;
    const sub = this.invitationService.createInvitation(invitation).subscribe({
      next: (newInvitation) => {
        this.invitations.unshift(newInvitation);
        this.newInvitationEmail = '';
        this.showMessage('Invitación enviada exitosamente', 'success');
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error sending invitation:', error);
        let errorMessage = 'Error al enviar la invitación';
        if (error.error?.message) {
          errorMessage = error.error.message;
        }
        this.showMessage(errorMessage, 'error');
        this.isLoading = false;
      }
    });
    this.subscription.add(sub);
  }

  deleteInvitation(invitationId: string): void {
    const sub = this.invitationService.deleteInvitation(invitationId).subscribe({
      next: () => {
        this.invitations = this.invitations.filter(inv => inv.id !== invitationId);
        this.showMessage('Invitación eliminada', 'success');
      },
      error: (error) => {
        console.error('Error deleting invitation:', error);
        this.showMessage('Error al eliminar la invitación', 'error');
      }
    });
    this.subscription.add(sub);
  }

  copyInvitationLink(invitationId: string): void {
    const invitationUrl = `${window.location.origin}/join/${invitationId}`;
    navigator.clipboard.writeText(invitationUrl).then(() => {
      this.showMessage('Enlace copiado al portapapeles', 'success');
    }).catch(() => {
      this.showMessage('Error al copiar el enlace', 'error');
    });
  }

  getInvitationStatus(invitation: InvitationDto): string {
    if (invitation.isUsed) {
      return 'Usado';
    }
    
    const now = new Date();
    const expiresAt = new Date(invitation.expiresAt);
    
    if (now > expiresAt) {
      return 'Expirado';
    }
    
    return 'Pendiente';
  }

  getStatusClass(invitation: InvitationDto): string {
    const status = this.getInvitationStatus(invitation);
    switch (status) {
      case 'Usado':
        return 'bg-green-100 text-green-800 dark:bg-green-900/20 dark:text-green-400';
      case 'Expirado':
        return 'bg-red-100 text-red-800 dark:bg-red-900/20 dark:text-red-400';
      case 'Pendiente':
        return 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900/20 dark:text-yellow-400';
      default:
        return 'bg-gray-100 text-gray-800 dark:bg-gray-900/20 dark:text-gray-400';
    }
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

  private showMessage(message: string, type: 'success' | 'error'): void {
    this.snackBar.open(message, 'Cerrar', {
      duration: 3000,
      panelClass: type === 'success' ? ['success-snackbar'] : ['error-snackbar']
    });
  }

  isValidEmail(email: string): boolean {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  }

  trackByInvitationId(index: number, invitation: InvitationDto): string {
    return invitation.id;
  }
}