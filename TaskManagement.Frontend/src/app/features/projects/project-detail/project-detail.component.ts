import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ProjectService } from '@core/services/project.service';
import { TaskService, Task } from '@core/services/task.service';
import { UserService } from '@core/services/user.service';

@Component({
  selector: 'app-project-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './project-detail.component.html',
  styleUrls: ['./project-detail.component.css']
})
export class ProjectDetailComponent implements OnInit {
  project: any = null;
  tasks: Task[] = [];
  users: any[] = [];
  loading = true;
  error = '';
  projectId = '';

  showCreateTask = false;
  showEditTask = false;
  showDeleteTask = false;

  newTask = { title: '', description: '', dueDate: '' };
  editingTask: any = null;
  deleteTarget: any = null;
  saving = false;
  saveError = '';

  activeWorkTaskId: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private projectService: ProjectService,
    private taskService: TaskService,
    private userService: UserService
  ) {}

  ngOnInit(): void {
    this.projectId = this.route.snapshot.paramMap.get('id') || '';
    this.load();
    this.userService.getUsers().subscribe({ next: (u: any[]) => this.users = u, error: () => {} });
    this.taskService.getActiveWork().subscribe({ next: (w: any) => this.activeWorkTaskId = w?.taskId || null, error: () => {} });
  }

  load(): void {
    this.loading = true;
    this.projectService.getProjectById(this.projectId).subscribe({
      next: (p: any) => { this.project = p; },
      error: () => { this.error = 'Failed to load project'; }
    });
    this.taskService.getByProject(this.projectId).subscribe({
      next: (t: Task[]) => { this.tasks = t; this.loading = false; },
      error: () => { this.error = 'Failed to load tasks'; this.loading = false; }
    });
  }

  get todoTasks(): Task[] { return this.tasks.filter(t => this.statusNum(t.status) === 0); }
  get inProgressTasks(): Task[] { return this.tasks.filter(t => this.statusNum(t.status) === 1); }
  get doneTasks(): Task[] { return this.tasks.filter(t => this.statusNum(t.status) === 2); }

  statusNum(s: number | string): number {
    if (typeof s === 'number') return s;
    const map: Record<string, number> = { Todo: 0, InProgress: 1, Done: 2 };
    return map[s] ?? 0;
  }

  statusLabel(s: number | string): string {
    const n = this.statusNum(s);
    return ['Todo', 'In Progress', 'Done'][n] ?? 'Unknown';
  }

  statusClass(s: number | string): string {
    return ['badge-todo', 'badge-inprogress', 'badge-done'][this.statusNum(s)] ?? 'badge-default';
  }

  assignedName(task: Task): string {
    if (task.assignedUser) return task.assignedUser.displayName;
    if (task.assignedUserId) {
      const u = this.users.find(x => x.id === task.assignedUserId);
      return u?.displayName || '?';
    }
    return '';
  }

  initials(name: string): string {
    return name.split(' ').map(w => w[0]).slice(0, 2).join('').toUpperCase();
  }

  formatDate(d: string): string {
    if (!d) return '';
    return new Date(d).toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
  }

  isOverdue(d: string): boolean {
    if (!d) return false;
    return new Date(d) < new Date();
  }

  // Create task
  openCreate(): void {
    this.newTask = { title: '', description: '', dueDate: '' };
    this.showCreateTask = true;
  }

  createTask(): void {
    if (!this.newTask.title.trim()) return;
    this.saving = true;
    this.taskService.create({ ...this.newTask, projectId: this.projectId }).subscribe({
      next: () => { this.showCreateTask = false; this.saving = false; this.load(); },
      error: () => { this.saving = false; }
    });
  }

  // Edit task
  openEdit(t: Task): void {
    this.editingTask = {
      ...t,
      dueDate: t.dueDate ? t.dueDate.split('T')[0] : ''
    };
    this.saveError = '';
    this.showEditTask = true;
  }

  updateTask(): void {
    if (!this.editingTask) return;
    this.saving = true;
    this.saveError = '';
    this.taskService.patchTask(this.editingTask.id, {
      title: this.editingTask.title,
      description: this.editingTask.description,
      status: this.statusNum(this.editingTask.status),
      assignedUserId: this.editingTask.assignedUserId || undefined,
      dueDate: this.editingTask.dueDate || undefined
    }).subscribe({
      next: () => { this.showEditTask = false; this.saving = false; this.load(); },
      error: (err: any) => {
        this.saving = false;
        this.saveError = err?.error?.Message || err?.message || 'Failed to save. Please try again.';
      }
    });
  }

  // Quick assign directly from kanban card
  quickAssign(task: Task, userId: string): void {
    if (!userId) return;
    this.taskService.assignUser(task.id, userId).subscribe({
      next: () => this.load(),
      error: () => {
        // If already assigned to this user, try patchTask instead
        this.taskService.patchTask(task.id, { assignedUserId: userId }).subscribe({
          next: () => this.load(),
          error: () => {}
        });
      }
    });
  }

  // Status change
  setStatus(task: Task, status: number): void {
    this.taskService.patchTask(task.id, { status }).subscribe({
      next: () => this.load(),
      error: () => {}
    });
  }

  // Assign user
  assignUser(task: Task, userId: string): void {
    if (!userId) return;
    this.taskService.assignUser(task.id, userId).subscribe({
      next: () => this.load(),
      error: () => {}
    });
  }

  // Delete
  openDelete(t: Task): void {
    this.deleteTarget = t;
    this.showDeleteTask = true;
  }

  confirmDelete(): void {
    if (!this.deleteTarget) return;
    this.saving = true;
    this.taskService.delete(this.deleteTarget.id).subscribe({
      next: () => { this.showDeleteTask = false; this.saving = false; this.deleteTarget = null; this.load(); },
      error: () => { this.saving = false; }
    });
  }

  // Time tracking
  isActive(task: Task): boolean {
    return this.activeWorkTaskId === task.id;
  }

  toggleWork(task: Task): void {
    if (this.isActive(task)) {
      this.taskService.stopWork(task.id).subscribe({
        next: () => { this.activeWorkTaskId = null; },
        error: () => {}
      });
    } else {
      this.taskService.startWork(task.id).subscribe({
        next: () => { this.activeWorkTaskId = task.id; },
        error: () => {}
      });
    }
  }
}
