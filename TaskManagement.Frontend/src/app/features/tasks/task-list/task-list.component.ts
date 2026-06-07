import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ProjectService } from '@core/services/project.service';
import { TaskService, Task } from '@core/services/task.service';

@Component({
  selector: 'app-task-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './task-list.component.html',
  styleUrls: ['./task-list.component.css']
})
export class TaskListComponent implements OnInit {
  projects: any[] = [];
  tasks: Task[] = [];
  selectedProjectId = '';
  loading = false;
  error = '';

  statusFilter: number | '' = '';

  constructor(
    private projectService: ProjectService,
    private taskService: TaskService
  ) {}

  ngOnInit(): void {
    this.projectService.getProjects().subscribe({
      next: (p: any[]) => { this.projects = p; if (p.length > 0) { this.selectedProjectId = p[0].id; this.loadTasks(); } },
      error: () => { this.error = 'Projekte konnten nicht geladen werden'; }
    });
  }

  loadTasks(): void {
    if (!this.selectedProjectId) return;
    this.loading = true;
    this.taskService.getByProject(this.selectedProjectId).subscribe({
      next: (t: Task[]) => { this.tasks = t; this.loading = false; },
      error: () => { this.error = 'Aufgaben konnten nicht geladen werden'; this.loading = false; }
    });
  }

  get filteredTasks(): Task[] {
    if (this.statusFilter === '') return this.tasks;
    return this.tasks.filter(t => this.statusNum(t.status) === this.statusFilter);
  }

  statusNum(s: number | string): number {
    if (typeof s === 'number') return s;
    const map: Record<string, number> = { Todo: 0, InProgress: 1, Done: 2 };
    return map[s] ?? 0;
  }

  statusLabel(s: number | string): string {
    return ['Zu erledigen', 'In Bearbeitung', 'Erledigt'][this.statusNum(s)] ?? '';
  }

  statusClass(s: number | string): string {
    return ['badge-todo', 'badge-inprogress', 'badge-done'][this.statusNum(s)] ?? '';
  }

  formatDate(d: string): string {
    if (!d) return '';
    return new Date(d).toLocaleDateString('de-DE', { month: 'short', day: 'numeric', year: 'numeric' });
  }

  isOverdue(d: string): boolean {
    return !!d && new Date(d) < new Date();
  }

  setStatus(task: Task, status: number): void {
    this.taskService.patchTask(task.id, { status }).subscribe({
      next: () => this.loadTasks(),
      error: () => {}
    });
  }

  deleteTask(id: string): void {
    if (!confirm('Diese Aufgabe löschen?')) return;
    this.taskService.delete(id).subscribe({
      next: () => this.loadTasks(),
      error: () => {}
    });
  }

  selectedProject(): any {
    return this.projects.find(p => p.id === this.selectedProjectId);
  }
}
