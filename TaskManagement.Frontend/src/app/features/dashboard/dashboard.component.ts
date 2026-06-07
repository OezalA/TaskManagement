import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ProjectService } from '@core/services/project.service';
import { UserService } from '@core/services/user.service';
import { TeamService } from '@core/services/team.service';
import { TaskService } from '@core/services/task.service';
import { AuthService } from '@core/services/auth.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {
  projectsCount = 0;
  usersCount = 0;
  teamsCount = 0;
  thisWeekTasks: any[] = [];
  activeWork: any = null;
  loading = true;
  displayName = '';

  constructor(
    private projectService: ProjectService,
    private userService: UserService,
    private teamService: TeamService,
    private taskService: TaskService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    const claims = this.authService.getTokenClaims();
    this.displayName = claims?.name || claims?.preferred_username || claims?.upn || '';
    this.loadDashboard();
  }

  loadDashboard(): void {
    this.projectService.getProjects().subscribe({
      next: (p: any[]) => this.projectsCount = p.length,
      error: () => {}
    });

    this.userService.getUsers().subscribe({
      next: (u: any[]) => this.usersCount = u.length,
      error: () => {}
    });

    this.teamService.getAll().subscribe({
      next: (t: any[]) => this.teamsCount = t.length,
      error: () => {}
    });

    this.taskService.getActiveWork().subscribe({
      next: (w: any) => this.activeWork = w,
      error: () => this.activeWork = null
    });

    this.taskService.getThisWeekTasks().subscribe({
      next: (tasks: any[]) => {
        this.thisWeekTasks = Array.isArray(tasks) ? tasks : [];
        this.loading = false;
      },
      error: () => { this.loading = false; }
    });
  }

  get greeting(): string {
    const h = new Date().getHours();
    if (h < 12) return 'Good morning';
    if (h < 17) return 'Good afternoon';
    return 'Good evening';
  }

  totalWeekMinutes(): number {
    return this.thisWeekTasks.reduce((s, t) => s + (t.totalMinutes || 0), 0);
  }

  formatTime(minutes: number): string {
    const h = Math.floor(minutes / 60);
    const m = minutes % 60;
    return h > 0 ? `${h}h ${m}m` : `${m}m`;
  }
}
