import { Component, DestroyRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { marked } from 'marked';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { switchMap } from 'rxjs';
import { ChatService, ChatSessionDto, ChatMessageDto } from '../../core/services/chat.service';
import { DocumentService, DocumentDto } from '../../core/services/document.service';

@Component({
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
  <div class="grid grid-cols-12 gap-4 h-[80vh]">
    <div class="col-span-3 bg-white dark:bg-gray-800 rounded p-3 overflow-auto">
      <button class="bg-primary-600 text-white px-3 py-2 rounded w-full mb-3" (click)="newSession()">New Chat</button>
      <select class="w-full mb-3 rounded" [(ngModel)]="selectedDocumentId"><option *ngFor="let d of documents" [value]="d.id">{{ d.originalFileName }}</option></select>
      <div *ngFor="let s of sessions" class="p-2 rounded cursor-pointer hover:bg-gray-100 dark:hover:bg-gray-700" (click)="selectSession(s)">{{ s.title }}</div>
    </div>
    <div class="col-span-9 bg-white dark:bg-gray-800 rounded p-4 flex flex-col">
      <div class="flex-1 overflow-auto space-y-3">
        <div *ngFor="let m of messages" [class.text-right]="m.role==='User'">
          <div [class]="m.role==='User' ? 'inline-block bg-primary-600 text-white px-3 py-2 rounded-lg' : 'inline-block bg-gray-200 text-gray-900 px-3 py-2 rounded-lg'">
            <div [innerHTML]="render(m.content)"></div>
            <div *ngIf="m.sourceChunkIds?.length" class="mt-2 text-xs">Sources: {{ m.sourceChunkIds?.join(', ') }}</div>
          </div>
        </div>
        <div *ngIf="loading" class="text-sm text-gray-500">Assistant is typing...</div>
      </div>
      <div class="mt-3 flex gap-2"><input class="flex-1 rounded" [(ngModel)]="question" placeholder="Ask a question" /><button class="bg-primary-600 text-white px-4 rounded" (click)="ask()">Send</button></div>
    </div>
  </div>`
})
export class ChatComponent {
  private chat = inject(ChatService);
  private docsApi = inject(DocumentService);
  private destroyRef = inject(DestroyRef);

  sessions: ChatSessionDto[] = [];
  messages: ChatMessageDto[] = [];
  documents: DocumentDto[] = [];
  selectedSessionId = '';
  selectedDocumentId = '';
  question = '';
  loading = false;

  constructor() {
    this.docsApi.list().pipe(takeUntilDestroyed(this.destroyRef)).subscribe(d => { this.documents = d; this.selectedDocumentId = d[0]?.id ?? ''; });
    this.refreshSessions();
  }

  render(content: string): string { return marked.parse(content) as string; }

  refreshSessions() { this.chat.getSessions().pipe(takeUntilDestroyed(this.destroyRef)).subscribe(v => this.sessions = v); }

  newSession() {
    if (!this.selectedDocumentId) return;
    this.chat.createSession(this.selectedDocumentId, 'New Chat')
      .pipe(switchMap(() => this.chat.getSessions()), takeUntilDestroyed(this.destroyRef))
      .subscribe(v => this.sessions = v);
  }

  selectSession(s: ChatSessionDto) {
    this.selectedSessionId = s.id;
    this.chat.getMessages(s.id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe(v => this.messages = v);
  }

  ask() {
    if (!this.selectedSessionId || !this.question.trim()) return;
    this.loading = true;
    this.chat.ask(this.selectedSessionId, { question: this.question, documentId: this.selectedDocumentId })
      .pipe(switchMap(() => this.chat.getMessages(this.selectedSessionId)), takeUntilDestroyed(this.destroyRef))
      .subscribe(v => { this.messages = v; this.question = ''; this.loading = false; });
  }
}
