import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatGridListModule } from '@angular/material/grid-list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';

export interface BingoCard {
  id: string;
  playerId: string;
  playerName: string;
  numbers: number[][];
  markedNumbers: boolean[][];
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
    MatChipsModule
  ],
  templateUrl: './admin-tables.component.html',
  styleUrls: ['./admin-tables.component.scss']
})
export class AdminTablesComponent implements OnInit {
  @Input() playerCards: BingoCard[] = [];
  @Input() drawnBalls: number[] = [];
  @Input() bingoType: 'SeventyFive' | 'Ninety' = 'SeventyFive';

  constructor() { }

  ngOnInit(): void {
    // Generar datos de ejemplo si no hay cartas
    if (this.playerCards.length === 0) {
      this.generateSampleCards();
    }
  }

  private generateSampleCards(): void {
    const samplePlayers = [
      { id: '1', name: 'Juan Pérez' },
      { id: '2', name: 'María García' },
      { id: '3', name: 'Carlos López' },
      { id: '4', name: 'Ana Martínez' },
      { id: '5', name: 'Luis Rodríguez' },
      { id: '6', name: 'Carmen Sánchez' }
    ];

    this.playerCards = samplePlayers.map(player => ({
      id: `card-${player.id}`,
      playerId: player.id,
      playerName: player.name,
      numbers: this.generateBingoNumbers(),
      markedNumbers: this.generateMarkedNumbers()
    }));
  }

  private generateBingoNumbers(): number[][] {
    const card: number[][] = [];
    const ranges = this.bingoType === 'SeventyFive' 
      ? [[1, 15], [16, 30], [31, 45], [46, 60], [61, 75]]
      : [[1, 18], [19, 36], [37, 54], [55, 72], [73, 90]];

    for (let col = 0; col < 5; col++) {
      const column: number[] = [];
      const [min, max] = ranges[col];
      const availableNumbers = Array.from({ length: max - min + 1 }, (_, i) => min + i);
      
      for (let row = 0; row < 5; row++) {
        if (row === 2 && col === 2) {
          column.push(0); // Centro libre
        } else {
          const randomIndex = Math.floor(Math.random() * availableNumbers.length);
          column.push(availableNumbers.splice(randomIndex, 1)[0]);
        }
      }
      card.push(column);
    }
    return card;
  }

  private generateMarkedNumbers(): boolean[][] {
    const marked: boolean[][] = [];
    for (let col = 0; col < 5; col++) {
      const column: boolean[] = [];
      for (let row = 0; row < 5; row++) {
        if (row === 2 && col === 2) {
          column.push(true); // Centro siempre marcado
        } else {
          column.push(Math.random() < 0.3); // 30% probabilidad de estar marcado
        }
      }
      marked.push(column);
    }
    return marked;
  }

  isNumberDrawn(number: number): boolean {
    return this.drawnBalls.includes(number);
  }

  getCardProgress(card: BingoCard): number {
    let markedCount = 0;
    let totalCount = 0;
    
    for (let col = 0; col < 5; col++) {
      for (let row = 0; row < 5; row++) {
        totalCount++;
        if (card.markedNumbers[col][row]) {
          markedCount++;
        }
      }
    }
    
    return Math.round((markedCount / totalCount) * 100);
  }

  checkForWinningPatterns(card: BingoCard): string[] {
    const patterns: string[] = [];
    const marked = card.markedNumbers;

    // Verificar líneas horizontales
    for (let row = 0; row < 5; row++) {
      if (marked.every(col => col[row])) {
        patterns.push(`Línea horizontal ${row + 1}`);
      }
    }

    // Verificar líneas verticales
    for (let col = 0; col < 5; col++) {
      if (marked[col].every(cell => cell)) {
        patterns.push(`Línea vertical ${col + 1}`);
      }
    }

    // Verificar diagonales
    if (marked.every((col, index) => col[index])) {
      patterns.push('Diagonal principal');
    }
    if (marked.every((col, index) => col[4 - index])) {
      patterns.push('Diagonal secundaria');
    }

    // Verificar cartón lleno
    if (marked.every(col => col.every(cell => cell))) {
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
}