import { bootstrapApplication } from '@angular/platform-browser';
import { APP_INITIALIZER } from '@angular/core';
import { provideRouter, withPreloading, PreloadAllModules } from '@angular/router';
import { provideHttpClient, withInterceptorsFromDi, HTTP_INTERCEPTORS } from '@angular/common/http';
import { provideAnimations } from '@angular/platform-browser/animations';
import { routes } from './app/app.routes';
import { AppComponent } from './app/app.component';
import { AuthInterceptor } from './app/core/interceptors/auth.interceptor';
import { MsalAuthService } from './app/core/services/msal-auth.service';

bootstrapApplication(AppComponent, {
  providers: [
    provideRouter(routes, withPreloading(PreloadAllModules)),
    provideHttpClient(withInterceptorsFromDi()),
    provideAnimations(),
    {
      provide: HTTP_INTERCEPTORS,
      useClass: AuthInterceptor,
      multi: true
    },
    {
      // Process the Microsoft redirect response (if any) before the router
      // and guards run, so the token is in place when navigation happens.
      provide: APP_INITIALIZER,
      useFactory: (msal: MsalAuthService) => () => msal.handleRedirectResult(),
      deps: [MsalAuthService],
      multi: true
    }
  ]
}).catch(err => console.error(err));
