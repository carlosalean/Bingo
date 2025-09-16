import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  
  // URLs que no requieren autenticación
  const publicUrls = [
    '/auth/login',
    '/auth/register',
    '/auth/guest'
  ];
  
  // Verificar si la URL actual es pública
  const isPublicUrl = publicUrls.some(url => req.url.includes(url));
  
  // Si es una URL pública, continuar sin modificar la petición
  if (isPublicUrl) {
    return next(req);
  }
  
  // Para URLs protegidas, agregar el token de autenticación
  const token = authService.getToken();
  
  if (token) {
    const authReq = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
    return next(authReq);
  }
  
  // Si no hay token, continuar con la petición original
  return next(req);
};