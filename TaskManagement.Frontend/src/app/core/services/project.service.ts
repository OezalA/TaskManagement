import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface Project {
  id: string;
  name: string;
  description: string;
  status: string;
  createdAt: string;
  lead: string;
}

@Injectable({
  providedIn: 'root'
})
export class ProjectService {
  private readonly baseUrl = 'api/projects';

  constructor(private api: ApiService) {}

  getProjects(): Observable<Project[]> {
    return this.api.get<Project[]>(this.baseUrl);
  }

  getProjectById(id: string): Observable<Project> {
    return this.api.get<Project>(`${this.baseUrl}/${id}`);
  }

  createProject(project: any): Observable<Project> {
    return this.api.post<Project>(this.baseUrl, project);
  }

  updateProject(id: string, project: any): Observable<Project> {
    return this.api.put<Project>(`${this.baseUrl}/${id}`, project);
  }

  deleteProject(id: string): Observable<void> {
    return this.api.delete<void>(`${this.baseUrl}/${id}`);
  }

  getTotalTime(projectId: string): Observable<any> {
    return this.api.get(`${this.baseUrl}/${projectId}/total-time`);
  }

  getTotalTimeAllUsers(projectId: string): Observable<any> {
    return this.api.get(`${this.baseUrl}/${projectId}/total-time-all-users`);
  }
}
