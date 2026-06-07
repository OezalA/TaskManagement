import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { Router } from '@angular/router';

interface NavItem {
  label: string;
  path: string;
  icon: string;
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.css']
})
export class SidebarComponent implements OnInit {
  navItems: NavItem[] = [
    { label: 'Dashboard', path: '/dashboard', icon: 'home' },
    { label: 'Projects',  path: '/projects',  icon: 'folder' },
    { label: 'Tasks',     path: '/tasks',      icon: 'check-square' },
    { label: 'Teams',     path: '/teams',      icon: 'users' },
    { label: 'Members',   path: '/users',      icon: 'user' },
    { label: 'Time',      path: '/time-tracking', icon: 'clock' },
    { label: 'AI Query',  path: '/mcp',           icon: 'ai' },
  ];

  displayName = '';

  constructor(private authService: AuthService, private router: Router) {}

  ngOnInit(): void {
    const claims = this.authService.getTokenClaims();
    this.displayName = claims?.name || claims?.preferred_username || claims?.upn || 'User';
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  getInitials(): string {
    return this.displayName.split(' ').map(w => w[0]).slice(0, 2).join('').toUpperCase();
  }
}
