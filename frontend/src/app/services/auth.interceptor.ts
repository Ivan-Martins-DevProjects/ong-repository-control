import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const cloned = req.clone({ withCredentials: true });

  return next(cloned).pipe(
    catchError(err => {
      if (err instanceof HttpErrorResponse && err.status === 401 && !req.url.includes('/api/auth/login')) {
        window.location.href = '/login';
      }
      return throwError(() => err);
    })
  );
};
