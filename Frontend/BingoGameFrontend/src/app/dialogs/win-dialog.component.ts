import { Component, Inject } from '@angular/core';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA, MatDialogTitle, MatDialogContent, MatDialogActions, MatDialogClose } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-win-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogTitle, MatDialogContent, MatDialogActions, MatDialogClose, MatButtonModule],
  template: `
    <h2 mat-dialog-title i18n>¡Bingo!</h2>
    <mat-dialog-content>
      <p i18n>{{ data.winner }} won!</p>
      <div class="confetti"></div>
    </mat-dialog-content>
    <mat-dialog-actions>
      <button mat-button mat-dialog-close i18n>OK</button>
    </mat-dialog-actions>
  `,
  styles: [`
    .confetti {
      position: relative;
      width: 100%;
      height: 100px;
      overflow: hidden;
    }
    .confetti::before {
      content: '';
      position: absolute;
      width: 10px;
      height: 10px;
      background: red;
      animation: confetti-fall 3s linear infinite;
    }
    .confetti::after {
      content: '';
      position: absolute;
      width: 10px;
      height: 10px;
      background: blue;
      left: 20px;
      animation: confetti-fall 3s linear infinite 0.5s;
    }
    @keyframes confetti-fall {
      0% { transform: translateY(-100px) rotate(0deg); opacity: 1; }
      100% { transform: translateY(100px) rotate(360deg); opacity: 0; }
    }
    @media (max-width: 768px) {
      .confetti { height: 50px; }
    }
  `]
})
export class WinDialogComponent {
  constructor(@Inject(MAT_DIALOG_DATA) public data: { winner: string }) {}
}