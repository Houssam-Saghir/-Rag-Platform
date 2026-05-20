import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { NgIf } from '@angular/common';

@Component({
  standalone: true,
  selector: 'app-shell',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, NgIf],
  template: `
  <div class="min-h-screen bg-gray-100 dark:bg-gray-900 text-gray-900 dark:text-gray-100 flex">
    <aside class="w-64 bg-white dark:bg-gray-800 p-4 flex flex-col">
      <h1 class="text-xl font-bold mb-6">RAG Platform</h1>
      <a routerLink="/dashboard" routerLinkActive="font-bold" class="py-2">Dashboard</a>
      <a routerLink="/documents" routerLinkActive="font-bold" class="py-2">Documents</a>
      <a routerLink="/chat" routerLinkActive="font-bold" class="py-2">Chat</a>
      <a routerLink="/admin" routerLinkActive="font-bold" class="py-2">Admin</a>
    </aside>
    <main class="flex-1 p-6">
      <router-outlet />
    </main>
  </div>`
})
export class ShellComponent {}
