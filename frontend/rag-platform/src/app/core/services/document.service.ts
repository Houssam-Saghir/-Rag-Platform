import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

export interface DocumentDto { id: string; originalFileName: string; fileType: string; status: string; uploadedAt: string; fileSizeBytes: number; }

@Injectable({ providedIn: 'root' })
export class DocumentService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/documents`;

  upload(file: File) {
    const fd = new FormData();
    fd.append('file', file);
    return this.http.post<{ documentId: string; message: string }>(`${this.base}/upload`, fd);
  }

  list() { return this.http.get<DocumentDto[]>(this.base); }
  delete(id: string) { return this.http.delete(`${this.base}/${id}`); }
}
