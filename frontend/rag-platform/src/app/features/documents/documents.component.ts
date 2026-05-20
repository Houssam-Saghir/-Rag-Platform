import { Component, DestroyRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { switchMap } from 'rxjs';
import { DocumentService, DocumentDto } from '../../core/services/document.service';

@Component({
  standalone: true,
  imports: [CommonModule],
  template: `
  <h2 class="text-2xl font-bold mb-4">Documents</h2>
  <div class="border-2 border-dashed rounded p-6 bg-white dark:bg-gray-800 mb-4" (drop)="drop($event)" (dragover)="$event.preventDefault()">Drop file here</div>
  <table class="w-full bg-white dark:bg-gray-800 rounded overflow-hidden">
    <tr class="text-left"><th class="p-2">Name</th><th>Status</th><th></th></tr>
    <tr *ngFor="let d of docs"><td class="p-2">{{ d.originalFileName }}</td><td>{{ d.status }}</td><td><button class="text-red-500" (click)="remove(d.id)">Delete</button></td></tr>
  </table>`
})
export class DocumentsComponent {
  private service = inject(DocumentService);
  private destroyRef = inject(DestroyRef);
  docs: DocumentDto[] = [];

  constructor() { this.load(); }

  private load() {
    this.service.list().pipe(takeUntilDestroyed(this.destroyRef)).subscribe(v => this.docs = v);
  }

  drop(event: DragEvent) {
    event.preventDefault();
    const file = event.dataTransfer?.files?.item(0);
    if (!file) return;
    this.service.upload(file).pipe(switchMap(() => this.service.list()), takeUntilDestroyed(this.destroyRef)).subscribe(v => this.docs = v);
  }

  remove(id: string) {
    if (!confirm('Delete this document?')) return;
    this.service.delete(id).pipe(switchMap(() => this.service.list()), takeUntilDestroyed(this.destroyRef)).subscribe(v => this.docs = v);
  }
}
