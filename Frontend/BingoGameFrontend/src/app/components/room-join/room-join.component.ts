import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { ApiService, JoinRoomDto } from '../../services/api.service';
import { AuthService } from '../../services/auth.service';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

@Component({
  selector: 'app-room-join',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatFormFieldModule,
    MatCheckboxModule,
    MatSnackBarModule
  ],
  templateUrl: './room-join.component.html',
  styleUrls: ['./room-join.component.scss']
})
export class RoomJoinComponent implements OnInit {
  joinForm: FormGroup;
  isLoading = false;
  guestToken: string | null = null;

  numCardsOptions = [1, 2, 3];

  constructor(
    private fb: FormBuilder,
    private apiService: ApiService,
    private authService: AuthService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {
    this.joinForm = this.fb.group({
      inviteCode: ['', [Validators.required]],
      numCards: [1, [Validators.required, Validators.min(1), Validators.max(3)]],
      isGuest: [false]
    });
  }

  ngOnInit(): void {}

  onGuestToggle(): void {
    const isGuest = this.joinForm.get('isGuest')?.value;
    if (isGuest) {
      this.isLoading = true;
      this.authService.getGuestToken().subscribe({
        next: (response: any) => {
          this.guestToken = response.token;
          this.isLoading = false;
        },
        error: () => {
          this.joinForm.get('isGuest')?.setValue(false);
          this.isLoading = false;
          this.snackBar.open('Failed to create guest session.', 'Close', { duration: 3000 });
        }
      });
    } else {
      this.guestToken = null;
    }
  }

  onSubmit(): void {
    if (this.joinForm.valid) {
      this.isLoading = true;
      const joinData: JoinRoomDto = {
        inviteCode: this.joinForm.value.inviteCode,
        numCards: this.joinForm.value.numCards,
        guestToken: this.guestToken || undefined
      };
      this.apiService.postJoinRoom(joinData).subscribe({
        next: (response: any) => {
          this.isLoading = false;
          const roomId = response.id;
          this.snackBar.open('Joined room successfully!', 'Close', { duration: 3000 });
          this.router.navigate([`/game/${roomId}`]);
        },
        error: (error) => {
          this.isLoading = false;
          this.snackBar.open('Failed to join room. Check the invite code.', 'Close', { duration: 3000 });
        }
      });
    }
  }

  goToDashboard(): void {
    this.router.navigate(['/dashboard']);
  }
}