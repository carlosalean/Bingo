import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { ApiService, RoomCreateDto } from '../../services/api.service';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

@Component({
  selector: 'app-room-create',
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
  templateUrl: './room-create.component.html',
  styleUrls: ['./room-create.component.scss']
})
export class RoomCreateComponent implements OnInit {
  createForm: FormGroup;
  isLoading = false;

  bingoTypes = ['SeventyFive', 'Ninety'];

  constructor(
    private fb: FormBuilder,
    private apiService: ApiService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {
    this.createForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(3)]],
      bingoType: ['SeventyFive', Validators.required],
      maxPlayers: [50, [Validators.required, Validators.min(1), Validators.max(100)]],
      isPrivate: [false]
    });
  }

  ngOnInit(): void {}

  onSubmit(): void {
    if (this.createForm.valid) {
      this.isLoading = true;
      const roomData: RoomCreateDto = this.createForm.value;
      this.apiService.postCreateRoom(roomData).subscribe({
        next: (response: any) => {
          this.isLoading = false;
          const roomId = response.id;
          this.snackBar.open('Room created successfully!', 'Close', { duration: 3000 });
          this.router.navigate([`/game/${roomId}`]);
        },
        error: (error) => {
          this.isLoading = false;
          this.snackBar.open('Failed to create room. Please try again.', 'Close', { duration: 3000 });
        }
      });
    }
  }

  goToDashboard(): void {
    this.router.navigate(['/dashboard']);
  }
}
