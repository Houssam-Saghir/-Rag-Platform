import { Component, DestroyRef, inject } from '@angular/core';
import { AsyncPipe } from '@angular/common';
import { map, startWith } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DocumentService } from '../../core/services/document.service';
import { ChatService } from '../../core/services/chat.service';

@Component({
  standalone: true,
  imports: [AsyncPipe],
  template: `
  <h2 class="text-2xl font-bold mb-4">Dashboard</h2>
  <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
    <div class="bg-white dark:bg-gray-800 p-4 rounded shadow">Documents: {{ docsCount$ | async }}</div>
    <div class="bg-white dark:bg-gray-800 p-4 rounded shadow">Sessions: {{ sessionsCount$ | async }}</div>
  </div>`
})
export class DashboardComponent {
  private docs = inject(DocumentService);
  private chat = inject(ChatService);
  private destroyRef = inject(DestroyRef);

  docsCount$ = this.docs.list().pipe(map(x => x.length), startWith(0), takeUntilDestroyed(this.destroyRef));
  sessionsCount$ = this.chat.getSessions().pipe(map(x => x.length), startWith(0), takeUntilDestroyed(this.destroyRef));
}
