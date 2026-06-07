import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { MsalAuthService } from '../../../core/services/msal-auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {
  token = '';
  loading = false;
  windowsLoading = false;
  error = '';
  showManual = false;

  constructor(
    private authService: AuthService,
    private msalAuthService: MsalAuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    // If we already have a token (e.g. just returned from the Microsoft
    // redirect), don't show the login screen — go straight to the app.
    if (this.authService.hasToken()) {
      this.router.navigate(['/dashboard']);
    }
  }

  async loginWithWindows(): Promise<void> {
    this.windowsLoading = true;
    this.error = '';
    try {
      // Full-page redirect to Microsoft. The page navigates away here; on
      // return, MsalAuthService.handleRedirectResult() (APP_INITIALIZER)
      // stores the token and the router lands on the dashboard.
      await this.msalAuthService.loginRedirect();
    } catch (err: any) {
      this.error = err?.message ?? 'Windows authentication failed. Please try again.';
      this.windowsLoading = false;
    }
  }

  login(): void {
    if (!this.token.trim()) {
      this.error = 'Please enter a JWT token';
      return;
    }
    this.loading = true;
    this.error = '';
    try {
      this.authService.setToken(this.token.trim());
      this.router.navigate(['/dashboard']);
    } catch {
      this.error = 'Invalid token format';
      this.loading = false;
    }
  }
}
