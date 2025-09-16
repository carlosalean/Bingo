import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';

export interface BingoCard {
  id: string;
  cardNumber: number;
  numbers: number[][];
  markedNumbers: boolean[][];
  isWinner: boolean;
  winningPattern?: string;
}

export interface CellClickEvent {
  cardId: string;
  row: number;
  col: number;
}

@Component({
  selector: 'app-bingo-card',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatTooltipModule
  ],
  templateUrl: './bingo-card.component.html',
  styleUrls: ['./bingo-card.component.scss']
})
export class BingoCardComponent implements OnInit {
  @Input() card!: BingoCard;
  @Input() drawnBalls: number[] = [];
  @Input() gameStatus: string = 'waiting';
  @Input() showControls: boolean = true;
  @Input() size: 'small' | 'medium' | 'large' = 'medium';
  
  @Output() cellClicked = new EventEmitter<CellClickEvent>();
  @Output() cardWinner = new EventEmitter<string>();

  ngOnInit() {
    // Marcar automáticamente el espacio libre (centro)
    if (this.card && this.card.markedNumbers) {
      this.card.markedNumbers[2][2] = true;
    }
  }

  onCellClick(row: number, col: number) {
    if (this.gameStatus !== 'playing') return;
    
    // No permitir marcar el espacio libre
    if (row === 2 && col === 2) return;
    
    const number = this.card.numbers[col][row];
    
    // Solo permitir marcar números que han sido cantados
    if (!this.isNumberDrawn(number)) return;
    
    // Alternar el estado de marcado
    this.card.markedNumbers[col][row] = !this.card.markedNumbers[col][row];
    
    // Emitir evento de click
    this.cellClicked.emit({
      cardId: this.card.id,
      row: row,
      col: col
    });
    
    // Verificar si la carta es ganadora después del marcado
    this.checkForWin();
  }

  isNumberDrawn(number: number): boolean {
    return this.drawnBalls.includes(number);
  }

  isCellMarked(row: number, col: number): boolean {
    return this.card.markedNumbers[col][row];
  }

  isFreeSpace(row: number, col: number): boolean {
    return row === 2 && col === 2;
  }

  canMarkCell(row: number, col: number): boolean {
    if (this.gameStatus !== 'playing') return false;
    if (this.isFreeSpace(row, col)) return false;
    
    const number = this.card.numbers[col][row];
    return this.isNumberDrawn(number);
  }

  getCardProgress(): number {
    let totalCells = 24; // 25 - 1 (espacio libre)
    let markedCells = 0;
    
    for (let col = 0; col < 5; col++) {
      for (let row = 0; row < 5; row++) {
        if (row === 2 && col === 2) continue; // Saltar espacio libre
        if (this.card.markedNumbers[col][row]) {
          markedCells++;
        }
      }
    }
    
    return Math.round((markedCells / totalCells) * 100);
  }

  checkForWin(): void {
    const patterns = this.getWinningPatterns();
    
    for (const pattern of patterns) {
      if (this.checkPattern(pattern.positions)) {
        this.card.isWinner = true;
        this.card.winningPattern = pattern.name;
        this.cardWinner.emit(this.card.id);
        return;
      }
    }
    
    this.card.isWinner = false;
    this.card.winningPattern = undefined;
  }

  private getWinningPatterns() {
    return [
      // Líneas horizontales
      { name: 'Línea Horizontal 1', positions: [[0,0],[1,0],[2,0],[3,0],[4,0]] },
      { name: 'Línea Horizontal 2', positions: [[0,1],[1,1],[2,1],[3,1],[4,1]] },
      { name: 'Línea Horizontal 3', positions: [[0,2],[1,2],[2,2],[3,2],[4,2]] },
      { name: 'Línea Horizontal 4', positions: [[0,3],[1,3],[2,3],[3,3],[4,3]] },
      { name: 'Línea Horizontal 5', positions: [[0,4],[1,4],[2,4],[3,4],[4,4]] },
      
      // Líneas verticales
      { name: 'Línea Vertical B', positions: [[0,0],[0,1],[0,2],[0,3],[0,4]] },
      { name: 'Línea Vertical I', positions: [[1,0],[1,1],[1,2],[1,3],[1,4]] },
      { name: 'Línea Vertical N', positions: [[2,0],[2,1],[2,2],[2,3],[2,4]] },
      { name: 'Línea Vertical G', positions: [[3,0],[3,1],[3,2],[3,3],[3,4]] },
      { name: 'Línea Vertical O', positions: [[4,0],[4,1],[4,2],[4,3],[4,4]] },
      
      // Diagonales
      { name: 'Diagonal Principal', positions: [[0,0],[1,1],[2,2],[3,3],[4,4]] },
      { name: 'Diagonal Secundaria', positions: [[4,0],[3,1],[2,2],[1,3],[0,4]] },
      
      // Cuatro esquinas
      { name: 'Cuatro Esquinas', positions: [[0,0],[4,0],[0,4],[4,4]] },
      
      // Cartón lleno
      { name: 'Cartón Lleno', positions: this.getAllPositions() }
    ];
  }

  private getAllPositions(): number[][] {
    const positions: number[][] = [];
    for (let col = 0; col < 5; col++) {
      for (let row = 0; row < 5; row++) {
        positions.push([col, row]);
      }
    }
    return positions;
  }

  private checkPattern(positions: number[][]): boolean {
    return positions.every(([col, row]) => {
      // El espacio libre siempre cuenta como marcado
      if (row === 2 && col === 2) return true;
      return this.card.markedNumbers[col][row];
    });
  }

  autoMarkDrawnNumbers(): void {
    if (this.gameStatus !== 'playing') return;
    
    let hasChanges = false;
    
    for (let col = 0; col < 5; col++) {
      for (let row = 0; row < 5; row++) {
        if (row === 2 && col === 2) continue; // Saltar espacio libre
        
        const number = this.card.numbers[col][row];
        if (this.isNumberDrawn(number) && !this.card.markedNumbers[col][row]) {
          this.card.markedNumbers[col][row] = true;
          hasChanges = true;
        }
      }
    }
    
    if (hasChanges) {
      this.checkForWin();
    }
  }

  getCellTooltip(row: number, col: number): string {
    if (this.isFreeSpace(row, col)) {
      return 'Espacio Libre';
    }
    
    const number = this.card.numbers[col][row];
    const isDrawn = this.isNumberDrawn(number);
    const isMarked = this.isCellMarked(row, col);
    
    if (isMarked) {
      return `Número ${number} - Marcado`;
    } else if (isDrawn) {
      return `Número ${number} - Cantado (Click para marcar)`;
    } else {
      return `Número ${number} - No cantado`;
    }
  }

  getCardStatusColor(): string {
    if (this.card.isWinner) return 'accent';
    
    const progress = this.getCardProgress();
    if (progress >= 80) return 'warn';
    if (progress >= 50) return 'primary';
    return 'basic';
  }

  getMarkedCount(): number {
    let count = 0;
    for (let col = 0; col < 5; col++) {
      for (let row = 0; row < 5; row++) {
        if (row === 2 && col === 2) continue; // Saltar espacio libre
        if (this.card.markedNumbers[col][row]) {
          count++;
        }
      }
    }
    return count;
  }

  getDrawnInCardCount(): number {
    let count = 0;
    for (let col = 0; col < 5; col++) {
      for (let row = 0; row < 5; row++) {
        if (row === 2 && col === 2) continue; // Saltar espacio libre
        const number = this.card.numbers[col][row];
        if (this.isNumberDrawn(number)) {
          count++;
        }
      }
    }
    return count;
  }
}