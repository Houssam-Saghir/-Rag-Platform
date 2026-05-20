import { Component, DestroyRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { switchMap } from 'rxjs';
import { AdminService } from '../../core/services/admin.service';
import { UserDto } from '../../core/services/auth.service';

@Component({
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
  <h2 class="text-2xl font-bold mb-4">Admin - Users</h2>
  <div class="bg-white dark:bg-gray-800 p-4 rounded mb-4 flex gap-2">
    <input class="rounded" [(ngModel)]="newEmail" placeholder="Email" />
    <input class="rounded" [(ngModel)]="newFirstName" placeholder="First Name" />
    <input class="rounded" [(ngModel)]="newLastName" placeholder="Last Name" />
    <input class="rounded" [(ngModel)]="newPassword" type="password" placeholder="Password" />
    <select class="rounded" [(ngModel)]="newRole"><option>User</option><option>Admin</option></select>
    <button class="bg-primary-600 text-white px-3 rounded" (click)="create()">Create</button>
  </div>
  <table class="w-full bg-white dark:bg-gray-800 rounded"><tr><th class="p-2 text-left">Email</th><th>Role(s)</th><th></th></tr><tr *ngFor="let u of users"><td class="p-2">{{u.email}}</td><td>{{u.roles.join(', ')}}</td><td><button class="text-red-500" (click)="remove(u.id)">Delete</button></td></tr></table>`
})
export class AdminComponent {
  private api = inject(AdminService);
  private destroyRef = inject(DestroyRef);
  users: UserDto[] = [];
  newEmail = ''; newFirstName = ''; newLastName = ''; newPassword = 'Admin@123'; newRole = 'User';

  constructor() { this.load(); }
  load() { this.api.users().pipe(takeUntilDestroyed(this.destroyRef)).subscribe(v => this.users = v); }
  create() {
    this.api.create({ email: this.newEmail, firstName: this.newFirstName, lastName: this.newLastName, password: this.newPassword, role: this.newRole })
      .pipe(switchMap(() => this.api.users()), takeUntilDestroyed(this.destroyRef)).subscribe(v => this.users = v);
  }
  remove(id: string) { this.api.remove(id).pipe(switchMap(() => this.api.users()), takeUntilDestroyed(this.destroyRef)).subscribe(v => this.users = v); }
}
