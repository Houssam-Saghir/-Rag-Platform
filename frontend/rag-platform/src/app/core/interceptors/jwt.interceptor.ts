import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { catchError, switchMap, throwError } from 'rxjs';

export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const token = auth.getAccessToken();

  const cloned = token ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : req;
  return next(cloned).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status !== 401 || req.url.includes('/auth/refresh')) return throwError(() => error);
      return auth.refreshToken().pipe(
        switchMap(res => next(req.clone({ setHeaders: { Authorization: `Bearer ${res.accessToken}` } }))),
        catchError(err => {
          auth.logout();
          router.navigateByUrl('/login');
          return throwError(() => err);
        })
      );
    })
  );
};
