import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { TaskService } from '@core/services/task.service';
import { ProjectService } from '@core/services/project.service';
import { Subscription, interval } from 'rxjs';

@Component({
  selector: 'app-time-tracking',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './time-tracking.component.html',
  styleUrls: ['./time-tracking.component.css']
})
export class TimeTrackingComponent implements OnInit, OnDestroy {
  activeWork: any = null;
  weekTasks: any[] = [];
  projects: any[] = [];
  projectTasks: any[] = [];

  selectedProjectId = '';
  selectedTaskId = '';

  loading = true;
  elapsedSeconds = 0;
  private timerSub?: Subscription;

  constructor(private taskService: TaskService, private projectService: ProjectService) {}

  ngOnInit(): void {
    this.projectService.getProjects().subscribe({
      next: (p: any[]) => this.projects = p,
      error: () => {}
    });

    this.loadData();
  }

  loadData(): void {
    this.loading = true;
    this.taskService.getActiveWork().subscribe({
      next: (w: any) => {
        this.activeWork = w;
        if (w?.startTime) this.startTimer(new Date(w.startTime));
        this.loading = false;
      },
      error: () => { this.activeWork = null; this.loading = false; }
    });

    this.taskService.getThisWeekTasks().subscribe({
      next: (t: any[]) => this.weekTasks = Array.isArray(t) ? t : [],
      error: () => {}
    });
  }

  onProjectChange(): void {
    this.selectedTaskId = '';
    this.projectTasks = [];
    if (!this.selectedProjectId) return;
    this.taskService.getByProject(this.selectedProjectId).subscribe({
      next: (t: any[]) => this.projectTasks = t.filter((x: any) => this.statusNum(x.status) < 2),
      error: () => {}
    });
  }

  startWork(): void {
    if (!this.selectedTaskId) return;
    this.taskService.startWork(this.selectedTaskId).subscribe({
      next: (w: any) => { this.activeWork = w; this.startTimer(new Date(w.startTime)); this.loadData(); },
      error: () => {}
    });
  }

  stopWork(): void {
    if (!this.activeWork) return;
    this.taskService.stopWork(this.activeWork.taskId).subscribe({
      next: () => { this.activeWork = null; this.stopTimer(); this.loadData(); },
      error: () => {}
    });
  }

  private startTimer(startTime: Date): void {
    this.stopTimer();
    this.elapsedSeconds = Math.floor((Date.now() - startTime.getTime()) / 1000);
    this.timerSub = interval(1000).subscribe(() => this.elapsedSeconds++);
  }

  private stopTimer(): void {
    this.timerSub?.unsubscribe();
    this.elapsedSeconds = 0;
  }

  formatElapsed(s: number): string {
    const h = Math.floor(s / 3600);
    const m = Math.floor((s % 3600) / 60);
    const sec = s % 60;
    return [h, m, sec].map(v => String(v).padStart(2, '0')).join(':');
  }

  formatTime(minutes: number): string {
    const h = Math.floor(minutes / 60);
    const m = minutes % 60;
    return h > 0 ? `${h} Std ${m} Min` : `${m} Min`;
  }

  totalWeekMinutes(): number {
    return this.weekTasks.reduce((s: number, t: any) => s + (t.totalMinutes || 0), 0);
  }

  statusNum(s: number | string): number {
    if (typeof s === 'number') return s;
    const map: Record<string, number> = { Todo: 0, InProgress: 1, Done: 2 };
    return map[s] ?? 0;
  }

  projectName(id: string): string {
    return this.projects.find(p => p.id === id)?.name || id;
  }

  ngOnDestroy(): void { this.stopTimer(); }
}
