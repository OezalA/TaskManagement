import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface Task {
  id: string;
  title: string;
  description: string;
  status: number;
  projectId: string;
  project?: { id: string; name: string };
  assignedUserId?: string;
  assignedUser?: { id: string; displayName: string };
  dueDate?: string;
  createdAt: string;
}

export interface CreateTaskRequest {
  title: string;
  description?: string;
  projectId: string;
  dueDate?: string;
}

export interface UpdateTaskRequest {
  title?: string;
  description?: string;
  status?: number;
  assignedUserId?: string;
  dueDate?: string;
}

@Injectable({ providedIn: 'root' })
export class TaskService {
  private readonly base = 'api/tasks';

  constructor(private api: ApiService) {}

  getByProject(projectId: string): Observable<Task[]> {
    return this.api.get<Task[]>(`${this.base}/by-project/${projectId}`);
  }

  getById(id: string): Observable<Task> {
    return this.api.get<Task>(`${this.base}/${id}`);
  }

  create(req: CreateTaskRequest): Observable<Task> {
    return this.api.post<Task>(this.base, req);
  }

  patchTask(id: string, updates: UpdateTaskRequest): Observable<void> {
    return this.api.patch<void>(`${this.base}/${id}`, updates);
  }

  assignUser(taskId: string, userId: string): Observable<void> {
    return this.api.put<void>(`${this.base}/${taskId}/assign/${userId}`, {});
  }

  markAsDone(id: string): Observable<void> {
    return this.api.put<void>(`${this.base}/${id}/done`, {});
  }

  delete(id: string): Observable<void> {
    return this.api.delete<void>(`${this.base}/${id}`);
  }

  getThisWeekTasks(): Observable<any[]> {
    return this.api.get<any[]>(`${this.base}/this-week`);
  }

  getActiveWork(): Observable<any> {
    return this.api.get<any>(`${this.base}/active-work`);
  }

  startWork(taskId: string): Observable<any> {
    return this.api.post<any>(`${this.base}/${taskId}/start-work`, {});
  }

  stopWork(taskId: string): Observable<any> {
    return this.api.post<any>(`${this.base}/${taskId}/stop-work`, {});
  }

  getWorkLogs(taskId: string): Observable<any[]> {
    return this.api.get<any[]>(`${this.base}/${taskId}/work-logs`);
  }

  getTotalTime(taskId: string): Observable<any> {
    return this.api.get<any>(`${this.base}/${taskId}/total-time`);
  }

  // Backwards-compat alias used in dashboard
  getTasksByProject(projectId: string): Observable<Task[]> {
    return this.getByProject(projectId);
  }
}
