import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiService, DrawnBallsDto } from '../../services/api.service';
import { AuthService } from '../../services/auth.service';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatGridListModule } from '@angular/material/grid-list';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { SignalRService } from '../../services/signalr.service';
import { Subscription, interval } from 'rxjs';

interface BingoCardCell {
  number: number;
  marked: boolean;
  column: string; // 'B', 'I', 'N', 'G', 'O' for 75
}

interface Player {
  username: string;
  marks: number;
  totalCells: number;
}

interface Message {
  user: string;
  text: string;
  timestamp: Date;
}

@Component({
  selector: 'app-game-room',
  standalone: true,
  imports: [
    CommonModule,
    MatSidenavModule,
    MatListModule,
    MatChipsModule,
    MatIconModule,
    MatButtonModule,
    MatInputModule,
    MatFormFieldModule,
    MatDialogModule,
    MatSnackBarModule,
    MatProgressBarModule,
    MatGridListModule,
    ReactiveFormsModule
  ],
  templateUrl: './game-room.component.html',
  styleUrls: ['./game-room.component.scss']
})
export class GameRoomComponent implements OnInit, OnDestroy {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private apiService = inject(ApiService);
  private authService = inject(AuthService);
  private signalr = inject(SignalRService);
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);

  roomId: string = '';
  drawnBalls: number[] = [];
  bingoType: 'SeventyFive' | 'Ninety' = 'SeventyFive';
  players: Player[] = [];
  messages: Message[] = [];
  messageControl = new FormControl('');
  isHost = false;
  gameStatus = 'Waiting'; // Waiting, Started, Paused, Ended

  // Stub bingo card
  bingoCard: BingoCardCell[][] = [];
  is75Ball = true;

  ngOnInit(): void {
    this.roomId = this.route.snapshot.paramMap.get('id') || '';
    if (!this.roomId) {
      this.router.navigate(['/dashboard']);
      return;
    }

    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/login']);
      return;
    }

    this.loadRoomData();
    this.generateBingoCard();
    this.initSignalR();
  }

  ngOnDestroy(): void {
    this.signalr.stop();
  }

  private initSignalR(): void {
    this.signalr.start().then(() => {
      this.signalr.joinRoom(this.roomId);

      // Get initial drawn balls
      this.signalr.getDrawnBalls(this.roomId).then((data: { balls?: number[] }) => {
        this.drawnBalls = data.balls || [];
        this.updateMarkedCells();
      });

      // Subscribe to events
      this.signalr.onBallDrawn((ball: number) => {
        this.drawnBalls.push(ball);
        this.updateMarkedCells();
        this.animateBallDraw(ball);
      });

      this.signalr.onNewMessage((msg: { user: string; text: string; timestamp: string }) => {
        this.messages.push({
          user: msg.user,
          text: msg.text,
          timestamp: new Date(msg.timestamp)
        });
      });

      this.signalr.onWinDetected((win: { winner: string }) => {
        this.showWin(win.winner);
      });

      this.signalr.onPlayerJoined((player: { username: string }) => {
        this.players.push({
          username: player.username,
          marks: 0,
          totalCells: 24
        });
      });

      this.signalr.onPlayerLeft((player: { username: string }) => {
        this.players = this.players.filter(p => p.username !== player.username);
      });

      this.signalr.onCardMarked((data: { username: string; marks: number }) => {
        const player = this.players.find(p => p.username === data.username);
        if (player) {
          player.marks = data.marks;
        }
      });
    }).catch(err => {
      console.error('SignalR connection failed', err);
    });
  }

  loadRoomData(): void {
    // Stub: load room info, players, game status
    this.isHost = true; // Assume for now
    this.gameStatus = 'Waiting';
    this.players = [
      { username: 'Player1', marks: 0, totalCells: 24 },
      { username: 'Player2', marks: 5, totalCells: 24 }
    ];
  }


  generateBingoCard(): void {
    this.is75Ball = this.bingoType === 'SeventyFive';
    if (this.is75Ball) {
      // 5x5 card for 75 ball
      this.bingoCard = [];
      const columns = ['B', 'I', 'N', 'G', 'O'];
      for (let col = 0; col < 5; col++) {
        this.bingoCard[col] = [];
        const min = col * 15 + 1;
        const max = min + 14;
        for (let row = 0; row < 5; row++) {
          if (col === 2 && row === 2) {
            // Free space
            this.bingoCard[col][row] = { number: 0, marked: true, column: 'N' };
          } else {
            const number = Math.floor(Math.random() * 15) + min;
            this.bingoCard[col][row] = { number, marked: false, column: columns[col] };
          }
        }
      }
    } else {
      // 9x3 card for 90 ball (stub, simplify)
      this.bingoCard = [];
      for (let col = 0; col < 9; col++) {
        this.bingoCard[col] = [];
        for (let row = 0; row < 3; row++) {
          const number = Math.floor(Math.random() * 90) + 1;
          this.bingoCard[col][row] = { number, marked: false, column: 'C' + col };
        }
      }
    }
  }

  updateMarkedCells(): void {
    this.bingoCard.forEach(col => {
      col.forEach(cell => {
        if (cell.number > 0 && this.drawnBalls.includes(cell.number)) {
          cell.marked = true;
        }
      });
    });
  }

  async toggleCell(colIndex: number, rowIndex: number, cell: BingoCardCell): Promise<void> {
    if (cell.number > 0 && this.drawnBalls.includes(cell.number)) {
      const cardId = this.authService.getUser()?.id ?? 'guest';
      const position = { col: colIndex, row: rowIndex };
      try {
        await this.signalr.invokeMarkNumber(this.roomId, cardId, position, !cell.marked);
        cell.marked = !cell.marked;
      } catch (err: any) {
        console.error('Mark failed', err);
      }
    }
  }

  async sendMessage(): Promise<void> {
    const text = this.messageControl.value?.trim();
    if (text) {
      try {
        await this.signalr.invokeSendChat(this.roomId, text);
        this.messages.push({ user: 'You', text, timestamp: new Date() });
        this.messageControl.setValue('');
      } catch (err: any) {
        console.error('Send message failed', err);
      }
    }
  }

  startGame(): void {
    this.apiService.postStartGame(this.roomId).subscribe({
      next: () => {
        this.gameStatus = 'Started';
      },
      error: () => {}
    });
  }

  drawBall(): void {
    this.apiService.postDrawBall(this.roomId).subscribe({
      next: () => {},
      error: () => {}
    });
  }

  pauseGame(): void {
    this.apiService.postPauseGame(this.roomId).subscribe({
      next: () => {
        this.gameStatus = 'Paused';
      },
      error: () => {}
    });
  }

  endGame(): void {
    this.apiService.postEndGame(this.roomId).subscribe({
      next: () => {
        this.gameStatus = 'Ended';
      },
      error: () => {}
    });
  }

  private showWin(winner: string): void {
    this.snackBar.open(`¡Bingo! ${winner} won!`, 'OK', {
      duration: 5000,
      horizontalPosition: 'center',
      verticalPosition: 'top'
    });
  }

  private animateBallDraw(ball: number): void {
    // Stub for SVG animation
    console.log('Animating ball', ball);
    // To be implemented: create/show SVG ruleta, spin, pop-up
  }

  goToDashboard(): void {
    this.router.navigate(['/dashboard']);
  }
}