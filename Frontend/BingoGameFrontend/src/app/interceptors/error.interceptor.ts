import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { AuthService } from '../services/auth.service';
import { catchError, throwError } from 'rxjs';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const snackBar = inject(MatSnackBar);
  const authService = inject(AuthService);

  return next(req).pipe(
    catchError((error) => {
      let message = 'An unknown error occurred';

      if (error.status === 401) {
        authService.logout();
        message = $localize `Authentication failed. Please log in again.`;
      } else if (error.status === 400) {
        message = $localize `Validation Error`;
        if (error.error && error.error.errors) {
          message = Object.values(error.error.errors).flat().join(', ');
        }
      } else if (error.status === 404) {
        message = $localize `Not Found`;
      } else if (error.status >= 500) {
        message = $localize `Server Error. Please try again later.`;
      }

      snackBar.open(message, 'Close', { duration: 5000 });

      return throwError(() => error);
    })
  );
};