import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { UserDto } from './auth.service';

@Injectable({ providedIn: 'root' })
export class AdminService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/admin`;

  users() { return this.http.get<UserDto[]>(`${this.base}/users`); }
  create(payload: unknown) { return this.http.post<UserDto>(`${this.base}/users`, payload); }
  update(id: string, payload: unknown) { return this.http.put<UserDto>(`${this.base}/users/${id}`, payload); }
  remove(id: string) { return this.http.delete(`${this.base}/users/${id}`); }
  assignRole(id: string, role: string) { return this.http.post(`${this.base}/users/${id}/roles`, { role }); }
}
