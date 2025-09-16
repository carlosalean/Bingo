import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatTabsModule } from '@angular/material/tabs';
import { MatBadgeModule } from '@angular/material/badge';
import { BingoCardComponent, BingoCard, CellClickEvent } from '../bingo-card/bingo-card.component';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';

export interface PlayerBingoCard extends BingoCard {
  // Hereda todas las propiedades de BingoCard
}

@Component({
  selector: 'app-player-cards',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatTabsModule,
    MatBadgeModule,
    MatDialogModule,
    MatSnackBarModule,
    BingoCardComponent
  ],
  templateUrl: './player-cards.component.html',
  styleUrls: ['./player-cards.component.scss']
})
export class PlayerCardsComponent implements OnInit {
  @Input() playerCards: PlayerBingoCard[] = [];
  @Input() drawnBalls: number[] = [];
  @Input() bingoType: 'SeventyFive' | 'Ninety' = 'SeventyFive';
  @Input() gameStatus: string = 'waiting';
  @Output() cardMarked = new EventEmitter<{cardId: string, row: number, col: number}>();
  @Output() bingoCall = new EventEmitter<{cardId: string, pattern: string}>();

  selectedCardIndex: number = 0;
  showAllCards: boolean = false;

  constructor(
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    if (this.playerCards.length === 0) {
      this.generatePlayerCards(3);
    }
  }

  generatePlayerCards(count: number): void {
    this.playerCards = [];
    for (let i = 1; i <= count; i++) {
      const card: PlayerBingoCard = {
        id: `player-card-${i}`,
        cardNumber: i,
        numbers: this.generateBingoNumbers(),
        markedNumbers: this.generateEmptyMarkedNumbers(),
        isWinner: false,
        winningPattern: undefined
      };
      this.playerCards.push(card);
    }
  }

  private generateBingoNumbers(): number[][] {
    const numbers: number[][] = [];
    const ranges = [
      { min: 1, max: 15 },   // B
      { min: 16, max: 30 },  // I
      { min: 31, max: 45 },  // N
      { min: 46, max: 60 },  // G
      { min: 61, max: 75 }   // O
    ];

    for (let col = 0; col < 5; col++) {
      numbers[col] = [];
      const usedNumbers = new Set<number>();
      
      for (let row = 0; row < 5; row++) {
        if (row === 2 && col === 2) {
          numbers[col][row] = 0; // Espacio libre
        } else {
          let number: number;
          do {
            number = Math.floor(Math.random() * (ranges[col].max - ranges[col].min + 1)) + ranges[col].min;
          } while (usedNumbers.has(number));
          
          usedNumbers.add(number);
          numbers[col][row] = number;
        }
      }
    }
    
    return numbers;
  }

  private generateEmptyMarkedNumbers(): boolean[][] {
    const marked: boolean[][] = [];
    for (let col = 0; col < 5; col++) {
      marked[col] = [];
      for (let row = 0; row < 5; row++) {
        if (row === 2 && col === 2) {
          marked[col][row] = true; // Espacio libre siempre marcado
        } else {
          marked[col][row] = false;
        }
      }
    }
    return marked;
  }

  onCellClick(event: CellClickEvent): void {
    console.log('Cell clicked:', event);
    this.cardMarked.emit({
      cardId: event.cardId,
      row: event.row,
      col: event.col
    });
  }

  selectCard(cardIndex: number): void {
    this.selectedCardIndex = cardIndex;
  }

  toggleViewMode(): void {
    this.showAllCards = !this.showAllCards;
  }

  getCardProgress(card: PlayerBingoCard): number {
    let totalCells = 24; // 25 - 1 (espacio libre)
    let markedCells = 0;
    
    for (let col = 0; col < 5; col++) {
      for (let row = 0; row < 5; row++) {
        if (row === 2 && col === 2) continue; // Saltar espacio libre
        if (card.markedNumbers[col][row]) {
          markedCells++;
        }
      }
    }
    
    return Math.round((markedCells / totalCells) * 100);
  }

  getCardStatusColor(card: PlayerBingoCard): string {
    if (card.isWinner) return 'accent';
    
    const progress = this.getCardProgress(card);
    if (progress >= 80) return 'warn';
    if (progress >= 50) return 'primary';
    return 'basic';
  }

  isNumberDrawn(number: number): boolean {
    return this.drawnBalls.includes(number);
  }

  getWinnerCardsCount(): number {
    return this.playerCards.filter(card => card.isWinner).length;
  }

  getAverageProgress(): number {
    if (this.playerCards.length === 0) return 0;
    
    const totalProgress = this.playerCards.reduce((sum, card) => {
      return sum + this.getCardProgress(card);
    }, 0);
    
    return Math.round(totalProgress / this.playerCards.length);
  }

  getTotalMarkedNumbers(): number {
    return this.playerCards.reduce((total, card) => {
      let markedCount = 0;
      for (let col = 0; col < 5; col++) {
        for (let row = 0; row < 5; row++) {
          if (row === 2 && col === 2) continue; // Saltar espacio libre
          if (card.markedNumbers[col][row]) {
            markedCount++;
          }
        }
      }
      return total + markedCount;
    }, 0);
  }

  autoMarkDrawnNumbers(): void {
    let hasChanges = false;
    
    this.playerCards.forEach(card => {
      for (let col = 0; col < 5; col++) {
        for (let row = 0; row < 5; row++) {
          if (row === 2 && col === 2) continue; // Saltar espacio libre
          
          const number = card.numbers[col][row];
          if (this.isNumberDrawn(number) && !card.markedNumbers[col][row]) {
            card.markedNumbers[col][row] = true;
            hasChanges = true;
          }
        }
      }
    });
    
    if (hasChanges) {
      this.snackBar.open('Números cantados marcados automáticamente', 'Cerrar', {
        duration: 3000
      });
    } else {
      this.snackBar.open('No hay números nuevos para marcar', 'Cerrar', {
        duration: 2000
      });
    }
  }

  trackByCardId(index: number, card: PlayerBingoCard): string {
    return card.id;
  }
}