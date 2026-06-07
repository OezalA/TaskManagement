import { Injectable } from '@angular/core';
import { PublicClientApplication, AuthenticationResult } from '@azure/msal-browser';
import { environment } from '@env/environment';
import { AuthService } from './auth.service';

@Injectable({ providedIn: 'root' })
export class MsalAuthService {
  private instance: PublicClientApplication;
  private ready: Promise<void>;

  constructor(private authService: AuthService) {
    this.instance = new PublicClientApplication({
      auth: {
        clientId: environment.msal.clientId,
        authority: `https://login.microsoftonline.com/${environment.msal.tenantId}`,
        redirectUri: window.location.origin
      },
      cache: { cacheLocation: 'sessionStorage' }
    });
    this.ready = this.instance.initialize();
  }

  /**
   * Runs once at app startup (APP_INITIALIZER). If the page was just
   * redirected back from Microsoft, this exchanges the auth code for an
   * access token and stores it, so the router can land on the dashboard.
   * On a normal load it resolves immediately with no effect.
   */
  async handleRedirectResult(): Promise<void> {
    await this.ready;
    let result: AuthenticationResult | null = null;
    try {
      result = await this.instance.handleRedirectPromise();
    } catch {
      // Stale/aborted redirect state — ignore; the user can sign in again.
      return;
    }
    if (result?.account) {
      this.instance.setActiveAccount(result.account);
    }
    if (result?.accessToken) {
      this.authService.setToken(result.accessToken);
    }
  }

  /**
   * Starts an interactive sign-in via a full-page redirect (no popup, so
   * MFA / "stay signed in" prompts work without any popup timeout).
   * The page navigates away to Microsoft and returns to redirectUri.
   */
  async loginRedirect(): Promise<void> {
    await this.ready;
    await this.instance.loginRedirect({ scopes: [environment.msal.apiScope] });
  }
}
