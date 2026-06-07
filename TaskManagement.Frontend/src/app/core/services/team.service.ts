import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface Team {
  id: string;
  name: string;
}

export interface TeamMember {
  userId: string;
  userName?: string;
}

@Injectable({ providedIn: 'root' })
export class TeamService {
  private readonly base = 'api/teams';

  constructor(private api: ApiService) {}

  getAll(): Observable<Team[]> {
    return this.api.get<Team[]>(this.base);
  }

  getById(id: string): Observable<Team> {
    return this.api.get<Team>(`${this.base}/${id}`);
  }

  create(name: string): Observable<Team> {
    return this.api.post<Team>(this.base, { name });
  }

  delete(id: string): Observable<void> {
    return this.api.delete<void>(`${this.base}/${id}`);
  }

  getMembers(teamId: string): Observable<TeamMember[]> {
    return this.api.get<TeamMember[]>(`${this.base}/${teamId}/members`);
  }

  addMember(teamId: string, userId: string): Observable<void> {
    return this.api.post<void>(`${this.base}/${teamId}/users/${userId}`, {});
  }

  removeMember(teamId: string, userId: string): Observable<void> {
    return this.api.delete<void>(`${this.base}/${teamId}/users/${userId}`);
  }
}
