import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface User {
  id: string;
  firstName: string;
  lastName: string;
  displayName: string;
  email: string;
}

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private readonly baseUrl = 'api/users';

  constructor(private api: ApiService) {}

  getUsers(): Observable<User[]> {
    return this.api.get<User[]>(this.baseUrl);
  }

  getUserById(id: string): Observable<User> {
    return this.api.get<User>(`${this.baseUrl}/${id}`);
  }

  createUser(user: any): Observable<User> {
    return this.api.post<User>(this.baseUrl, user);
  }

  updateUser(id: string, user: any): Observable<User> {
    return this.api.put<User>(`${this.baseUrl}/${id}`, user);
  }

  deleteUser(id: string): Observable<void> {
    return this.api.delete<void>(`${this.baseUrl}/${id}`);
  }

  getWorkLogs(userId: string, week?: string): Observable<any> {
    let url = `${this.baseUrl}/${userId}/worklogs`;
    if (week) {
      url += `?week=${week}`;
    }
    return this.api.get(url);
  }
}
