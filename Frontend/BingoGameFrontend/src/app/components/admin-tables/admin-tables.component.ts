import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatGridListModule } from '@angular/material/grid-list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { ApiService, BingoCardDto } from '../../services/api.service';
import { InvitationManagerComponent } from '../invitation-manager/invitation-manager.component';
import { Subscription } from 'rxjs';

export interface BingoCard {
  id: string;
  playerId: string;
  playerName: string;
  numbers: number[];
  marks: { [key: string]: boolean };
  type: 'SeventyFive' | 'Ninety';
  isWinner?: boolean;
}

@Component({
  selector: 'app-admin-tables',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatGridListModule,
    MatIconModule,
    MatButtonModule,
    MatChipsModule,
    InvitationManagerComponent
  ],
  templateUrl: './admin-tables.component.html',
  styleUrls: ['./admin-tables.component.scss']
})
export class AdminTablesComponent implements OnInit, OnDestroy {
  @Input() playerCards: BingoCard[] = [];
  @Input() drawnBalls: number[] = [];
  @Input() bingoType: 'SeventyFive' | 'Ninety' = 'SeventyFive';
  @Input() roomId: string = '';

  private subscription: Subscription = new Subscription();

  constructor(private apiService: ApiService) { }

  ngOnInit(): void {
    if (this.roomId) {
      this.loadRoomPlayers();
    }
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

  private loadRoomPlayers(): void {
    const sub = this.apiService.getRoomPlayers(this.roomId).subscribe({
      next: (cards: BingoCardDto[]) => {
        this.playerCards = cards.map(card => ({
          id: card.id,
          playerId: card.playerId || 'guest',
          playerName: card.playerName || 'Guest',
          numbers: card.numbers,
          marks: card.marks,
          type: card.type
        }));
      },
      error: (error) => {
        console.error('Error loading room players:', error);
        this.playerCards = [];
      }
    });
    this.subscription.add(sub);
  }



  isNumberDrawn(number: number): boolean {
    return this.drawnBalls.includes(number);
  }

  getCardProgress(card: BingoCard): number {
    let markedCount = 0;
    const totalCount = 25;
    
    for (let i = 0; i < totalCount; i++) {
      if (card.marks[i.toString()]) {
        markedCount++;
      }
    }
    
    return Math.round((markedCount / totalCount) * 100);
  }

  checkForWinningPatterns(card: BingoCard): string[] {
    const patterns: string[] = [];
    const marks = card.marks;

    // Verificar líneas horizontales
    for (let row = 0; row < 5; row++) {
      let isComplete = true;
      for (let col = 0; col < 5; col++) {
        const index = row * 5 + col;
        if (!marks[index.toString()]) {
          isComplete = false;
          break;
        }
      }
      if (isComplete) {
        patterns.push(`Línea horizontal ${row + 1}`);
      }
    }

    // Verificar líneas verticales
    for (let col = 0; col < 5; col++) {
      let isComplete = true;
      for (let row = 0; row < 5; row++) {
        const index = row * 5 + col;
        if (!marks[index.toString()]) {
          isComplete = false;
          break;
        }
      }
      if (isComplete) {
        patterns.push(`Línea vertical ${col + 1}`);
      }
    }

    // Verificar diagonal principal
    let diagonalComplete = true;
    for (let i = 0; i < 5; i++) {
      const index = i * 5 + i;
      if (!marks[index.toString()]) {
        diagonalComplete = false;
        break;
      }
    }
    if (diagonalComplete) {
      patterns.push('Diagonal principal');
    }

    // Verificar diagonal secundaria
    diagonalComplete = true;
    for (let i = 0; i < 5; i++) {
      const index = i * 5 + (4 - i);
      if (!marks[index.toString()]) {
        diagonalComplete = false;
        break;
      }
    }
    if (diagonalComplete) {
      patterns.push('Diagonal secundaria');
    }

    // Verificar cartón lleno
    let isFullCard = true;
    for (let i = 0; i < 25; i++) {
      if (!marks[i.toString()]) {
        isFullCard = false;
        break;
      }
    }
    if (isFullCard) {
      patterns.push('Cartón lleno');
    }

    return patterns;
  }

  getPlayerStatus(card: BingoCard): string {
    const patterns = this.checkForWinningPatterns(card);
    if (patterns.length > 0) {
      return patterns[0]; // Mostrar el primer patrón ganador
    }
    return `${this.getCardProgress(card)}% completado`;
  }

  getStatusColor(card: BingoCard): string {
    const patterns = this.checkForWinningPatterns(card);
    if (patterns.length > 0) {
      return 'accent'; // Color para ganadores
    }
    const progress = this.getCardProgress(card);
    if (progress >= 80) return 'warn';
    if (progress >= 60) return 'primary';
    return '';
  }

  trackByCardId(index: number, card: BingoCard): string {
    return card.id;
  }

  getWinnersCount(): number {
    return this.playerCards.filter(card => 
      this.checkForWinningPatterns(card).length > 0
    ).length;
  }

  getAverageProgress(): number {
    if (this.playerCards.length === 0) return 0;
    
    const totalProgress = this.playerCards.reduce((sum, card) => 
      sum + this.getCardProgress(card), 0
    );
    
    return Math.round(totalProgress / this.playerCards.length);
  }

  // Métodos auxiliares para trabajar con la estructura de datos plana
  getNumberAt(card: BingoCard, row: number, col: number): number {
    const index = row * 5 + col;
    return card.numbers[index] || 0;
  }

  isMarkedAt(card: BingoCard, row: number, col: number): boolean {
    const index = row * 5 + col;
    return card.marks[index.toString()] || false;
  }

  getCardAsMatrix(card: BingoCard): { number: number, marked: boolean }[][] {
    const matrix: { number: number, marked: boolean }[][] = [];
    for (let row = 0; row < 5; row++) {
      const rowData: { number: number, marked: boolean }[] = [];
      for (let col = 0; col < 5; col++) {
        rowData.push({
          number: this.getNumberAt(card, row, col),
          marked: this.isMarkedAt(card, row, col)
        });
      }
      matrix.push(rowData);
    }
    return matrix;
  }
}