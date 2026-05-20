import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

export interface ChatSessionDto { id: string; title: string; createdAt: string; messageCount: number; documentId?: string; }
export interface ChatMessageDto { id: string; role: string; content: string; createdAt: string; sourceChunkIds?: string[]; }

@Injectable({ providedIn: 'root' })
export class ChatService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/chat`;

  createSession(documentId: string, title?: string) { return this.http.post<ChatSessionDto>(`${this.base}/sessions`, { documentId, title }); }
  getSessions() { return this.http.get<ChatSessionDto[]>(`${this.base}/sessions`); }
  getMessages(sessionId: string) { return this.http.get<ChatMessageDto[]>(`${this.base}/sessions/${sessionId}/messages`); }
  ask(sessionId: string, payload: { question: string; documentId: string }) { return this.http.post<ChatMessageDto>(`${this.base}/sessions/${sessionId}/ask`, payload); }
  deleteSession(sessionId: string) { return this.http.delete(`${this.base}/sessions/${sessionId}`); }
}
