import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private hubUrl = 'http://localhost:5001/gamehub';
  public connection: HubConnection;

  constructor(private auth: AuthService) {
    this.connection = new HubConnectionBuilder()
      .withUrl(this.hubUrl, {
        accessTokenFactory: () => this.auth.getToken() ?? ''
      })
      .withAutomaticReconnect()
      .build();
  }

  public start(): Promise<void> {
    return this.connection.start().catch(err => {
      console.error('SignalR connection error:', err);
      throw err;
    });
  }

  public stop(): Promise<void> {
    return this.connection.stop().catch(err => {
      console.error('SignalR stop error:', err);
      throw err;
    });
  }

  public onBallDrawn(callback: (ball: any) => void): void {
    this.connection.on('BallDrawn', (ball: any) => {
      console.log('Ball drawn:', ball);
      callback(ball);
    });
  }

  public onNewMessage(callback: (message: any) => void): void {
    this.connection.on('NewMessage', (message: any) => {
      console.log('New message:', message);
      callback(message);
    });
  }

  public onWinDetected(callback: (win: any) => void): void {
    this.connection.on('WinDetected', (win: any) => {
      console.log('Win detected:', win);
      callback(win);
    });
  }

  public onPlayerJoined(callback: (player: any) => void): void {
    this.connection.on('PlayerJoined', (player: any) => {
      console.log('Player joined:', player);
      callback(player);
    });
  }

  public onCardMarked(callback: (data: any) => void): void {
    this.connection.on('CardMarked', (data: any) => {
      console.log('Card marked:', data);
      callback(data);
    });
  }

  public onPlayerLeft(callback: (player: any) => void): void {
    this.connection.on('PlayerLeft', (player: any) => {
      console.log('Player left:', player);
      callback(player);
    });
  }

  public getDrawnBalls(roomId: string): Promise<any> {
    return this.connection.invoke('GetDrawnBalls', roomId).catch(err => {
      console.error('Error getting drawn balls:', err);
      throw err;
    });
  }

  public invokeSendChat(roomId: string, message: string): Promise<void> {
    return this.connection.invoke('SendChat', roomId, message).catch(err => {
      console.error('Error sending chat:', err);
      throw err;
    });
  }

  public invokeMarkNumber(roomId: string, cardId: string, position: any, marked: boolean): Promise<void> {
    return this.connection.invoke('MarkNumber', roomId, cardId, position, marked).catch(err => {
      console.error('Error marking number:', err);
      throw err;
    });
  }

  public joinRoom(roomId: string): Promise<void> {
    return this.connection.invoke('JoinRoom', roomId).catch(err => {
      console.error('Error joining room:', err);
      throw err;
    });
  }
}