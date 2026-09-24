import { HttpInterceptorFn } from '@angular/common/http';

// The auth token is stored in a HttpOnly cookie (set by the API), so it is never accessible
// to client-side JS. We just need every request to include credentials (cookies) so the
// browser sends it back to the same-origin API.
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const cloned = req.clone({ withCredentials: true });
  return next(cloned);
};
