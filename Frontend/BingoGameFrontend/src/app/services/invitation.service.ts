import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface CreateInvitationDto {
  email: string;
  roomId: string;
}

export interface AcceptInvitationDto {
  invitationId: string;
  guestName: string;
}

export interface InvitationDto {
  id: string;
  email: string;
  roomId: string;
  invitedById: string;
  createdAt: string;
  expiresAt: string;
  isUsed: boolean;
  usedAt?: string;
  guestName?: string;
}

export interface TokenDto {
  token: string;
  expiresIn: string;
  user: {
    id: string;
    username: string;
    email: string;
    role: string;
  };
}

@Injectable({
  providedIn: 'root'
})
export class InvitationService {
  private apiUrl = `${environment.apiUrl}/invitation`;

  constructor(private http: HttpClient) { }

  private getAuthHeaders(): HttpHeaders {
    const token = localStorage.getItem('token');
    return new HttpHeaders({
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    });
  }

  createInvitation(invitation: CreateInvitationDto): Observable<InvitationDto> {
    return this.http.post<InvitationDto>(
      `${this.apiUrl}/create`,
      invitation,
      { headers: this.getAuthHeaders() }
    );
  }

  getRoomInvitations(roomId: string): Observable<InvitationDto[]> {
    return this.http.get<InvitationDto[]>(
      `${this.apiUrl}/room/${roomId}`,
      { headers: this.getAuthHeaders() }
    );
  }

  getInvitationById(invitationId: string): Observable<InvitationDto> {
    return this.http.get<InvitationDto>(
      `${this.apiUrl}/${invitationId}`
    );
  }

  acceptInvitation(acceptData: AcceptInvitationDto): Observable<TokenDto> {
    return this.http.post<TokenDto>(
      `${this.apiUrl}/accept`,
      acceptData
    );
  }

  deleteInvitation(invitationId: string): Observable<void> {
    return this.http.delete<void>(
      `${this.apiUrl}/${invitationId}`,
      { headers: this.getAuthHeaders() }
    );
  }

  isInvitationValid(invitationId: string): Observable<boolean> {
    return this.http.get<boolean>(
      `${this.apiUrl}/${invitationId}/valid`
    );
  }
}