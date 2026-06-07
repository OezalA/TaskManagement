import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UserService } from '@core/services/user.service';

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './user-list.component.html',
  styleUrls: ['./user-list.component.css']
})
export class UserListComponent implements OnInit {
  users: any[] = [];
  loading = true;
  error = '';

  selectedUser: any = null;
  userTasks: any[] = [];
  userWorkLogs: any = null;
  loadingDetail = false;
  selectedWeek = '';

  constructor(private userService: UserService) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading = true;
    this.userService.getUsers().subscribe({
      next: (u: any[]) => { this.users = u; this.loading = false; },
      error: () => { this.error = 'Mitglieder konnten nicht geladen werden'; this.loading = false; }
    });
  }

  selectUser(u: any): void {
    if (this.selectedUser?.id === u.id) { this.selectedUser = null; return; }
    this.selectedUser = u;
    this.loadUserDetail(u.id);
  }

  loadUserDetail(userId: string): void {
    this.loadingDetail = true;
    this.userTasks = [];
    this.userWorkLogs = null;

    this.userService.getWorkLogs(userId, this.selectedWeek || undefined).subscribe({
      next: (w: any) => { this.userWorkLogs = w; this.loadingDetail = false; },
      error: () => { this.loadingDetail = false; }
    });
  }

  initials(name: string): string {
    return (name || '?').split(' ').map((w: string) => w[0]).slice(0, 2).join('').toUpperCase();
  }

  formatTime(minutes: number): string {
    const h = Math.floor(minutes / 60);
    const m = minutes % 60;
    return h > 0 ? `${h} Std ${m} Min` : `${m} Min`;
  }

  formatDate(d: string): string {
    if (!d) return '';
    return new Date(d).toLocaleDateString('de-DE', { year: 'numeric', month: 'short', day: 'numeric' });
  }
}
