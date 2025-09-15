import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface LoginDto {
  email: string;
  password: string;
}

export interface RegisterDto {
  username: string;
  email: string;
  password: string;
}

export interface RoomCreateDto {
  name: string;
  bingoType: 'SeventyFive' | 'Ninety';
  maxPlayers: number;
  isPrivate: boolean;
}



export interface JoinRoomDto {
  inviteCode: string;
  numCards: number;
  guestToken?: string;
}

export interface DrawnBallsDto {
  balls: number[];
}

export interface RoomDto {
  id: string;
  name: string;
  bingoType: 'SeventyFive' | 'Ninety';
  maxPlayers: number;
  isPrivate: boolean;
  inviteCode?: string;
  currentPlayers?: number;
  status?: string;
}

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private baseUrl = 'https://localhost:7083/api';
  private headers = new HttpHeaders({ 'Content-Type': 'application/json' });

  constructor(private http: HttpClient) { }

  private getAuthHeaders(): HttpHeaders {
    const token = localStorage.getItem('token');
    return this.headers.set('Authorization', `Bearer ${token}`);
  }

  postLogin(credentials: LoginDto): Observable<any> {
    return this.http.post(`${this.baseUrl}/auth/login`, credentials, { headers: this.headers });
  }

  postRegister(userData: RegisterDto): Observable<any> {
    return this.http.post(`${this.baseUrl}/auth/register`, userData, { headers: this.headers });
  }

  getRooms(): Observable<any> {
    return this.http.get(`${this.baseUrl}/room`, { headers: this.getAuthHeaders() });
  }

  getUserRooms(): Observable<RoomDto[]> {
    return this.http.get<RoomDto[]>(`${this.baseUrl}/room/user`, { headers: this.getAuthHeaders() });
  }

  deleteRoom(roomId: string): Observable<any> {
    return this.http.delete(`${this.baseUrl}/room/${roomId}`, { headers: this.getAuthHeaders() });
  }

  postCreateRoom(roomData: RoomCreateDto): Observable<any> {
    return this.http.post(`${this.baseUrl}/room`, roomData, { headers: this.getAuthHeaders() });
  }

  postJoinRoom(joinData: JoinRoomDto): Observable<any> {
    return this.http.post(`${this.baseUrl}/room/join`, joinData, { headers: this.getAuthHeaders() });
  }

  getDrawnBalls(roomId: string): Observable<DrawnBallsDto> {
    return this.http.get<DrawnBallsDto>(`${this.baseUrl}/game/drawn/${roomId}`, { headers: this.getAuthHeaders() });
  }

  postStartGame(roomId: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/game/start/${roomId}`, {}, { headers: this.getAuthHeaders() });
  }

  postDrawBall(roomId: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/game/draw/${roomId}`, {}, { headers: this.getAuthHeaders() });
  }

  postPauseGame(roomId: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/game/pause/${roomId}`, {}, { headers: this.getAuthHeaders() });
  }

  postEndGame(roomId: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/game/end/${roomId}`, {}, { headers: this.getAuthHeaders() });
  }

  postGuest(): Observable<any> {
    return this.http.post(`${this.baseUrl}/auth/guest`, {}, { headers: this.headers });
  }



  joinRoom(roomCode: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/room/join-by-code`, { roomCode }, { headers: this.getAuthHeaders() });
  }

  joinRoomById(roomId: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/room/${roomId}/join`, {}, { headers: this.getAuthHeaders() });
  }

  getRoomByCode(roomCode: string): Observable<RoomDto> {
    return this.http.get<RoomDto>(`${this.baseUrl}/room/by-code/${roomCode}`, { headers: this.getAuthHeaders() });
  }

  getPublicRooms(): Observable<RoomDto[]> {
    return this.http.get<RoomDto[]>(`${this.baseUrl}/room/public`, { headers: this.getAuthHeaders() });
  }
}