import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  const authService = inject(AuthService);
  
  if (authService.isLoggedIn() && authService.isTokenValid()) {
    return true;
  }
  
  // Si no está autenticado o el token no es válido, hacer logout y redirigir
  authService.logout();
  router.navigate(['/login']);
  return false;
};
