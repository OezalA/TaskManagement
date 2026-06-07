import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface WorkLog {
  id: string;
  userId: string;
  taskId: string;
  startTime: string;
  endTime?: string;
  durationMinutes?: number;
  createdAt: string;
}

@Injectable({
  providedIn: 'root'
})
export class WorkLogService {
  private readonly baseUrl = 'api/tasks';

  constructor(private api: ApiService) {}

  startWork(taskId: string): Observable<WorkLog> {
    return this.api.post<WorkLog>(`${this.baseUrl}/${taskId}/start-work`, {});
  }

  stopWork(taskId: string): Observable<WorkLog> {
    return this.api.post<WorkLog>(`${this.baseUrl}/${taskId}/stop-work`, {});
  }

  getActiveWork(): Observable<WorkLog> {
    return this.api.get<WorkLog>(`${this.baseUrl}/active-work`);
  }

  getWorkLogs(taskId: string): Observable<WorkLog[]> {
    return this.api.get<WorkLog[]>(`${this.baseUrl}/${taskId}/work-logs`);
  }
}
