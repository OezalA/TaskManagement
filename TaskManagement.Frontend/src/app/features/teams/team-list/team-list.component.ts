import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TeamService, Team, TeamMember } from '@core/services/team.service';
import { UserService } from '@core/services/user.service';

@Component({
  selector: 'app-team-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './team-list.component.html',
  styleUrls: ['./team-list.component.css']
})
export class TeamListComponent implements OnInit {
  teams: Team[] = [];
  allUsers: any[] = [];
  loading = true;
  error = '';

  expandedTeamId: string | null = null;
  teamMembers: Record<string, TeamMember[]> = {};
  loadingMembers: Record<string, boolean> = {};

  showCreateModal = false;
  showDeleteModal = false;
  showAddMemberModal = false;
  newTeamName = '';
  deleteTarget: Team | null = null;
  addMemberTarget: Team | null = null;
  selectedUserId = '';
  saving = false;

  constructor(private teamService: TeamService, private userService: UserService) {}

  ngOnInit(): void {
    this.load();
    this.userService.getUsers().subscribe({ next: (u: any[]) => this.allUsers = u, error: () => {} });
  }

  load(): void {
    this.loading = true;
    this.teamService.getAll().subscribe({
      next: (t: Team[]) => { this.teams = t; this.loading = false; },
      error: () => { this.error = 'Teams konnten nicht geladen werden'; this.loading = false; }
    });
  }

  toggleTeam(team: Team): void {
    if (this.expandedTeamId === team.id) { this.expandedTeamId = null; return; }
    this.expandedTeamId = team.id;
    this.loadMembers(team.id);
  }

  loadMembers(teamId: string): void {
    this.loadingMembers[teamId] = true;
    this.teamService.getMembers(teamId).subscribe({
      next: (m: TeamMember[]) => { this.teamMembers[teamId] = m; this.loadingMembers[teamId] = false; },
      error: () => { this.loadingMembers[teamId] = false; }
    });
  }

  membersOf(teamId: string): TeamMember[] {
    return this.teamMembers[teamId] || [];
  }

  // Create team
  openCreate(): void { this.newTeamName = ''; this.showCreateModal = true; }

  createTeam(): void {
    if (!this.newTeamName.trim()) return;
    this.saving = true;
    this.teamService.create(this.newTeamName).subscribe({
      next: () => { this.showCreateModal = false; this.saving = false; this.load(); },
      error: () => { this.saving = false; }
    });
  }

  // Delete team
  openDelete(t: Team): void { this.deleteTarget = t; this.showDeleteModal = true; }

  confirmDelete(): void {
    if (!this.deleteTarget) return;
    this.saving = true;
    this.teamService.delete(this.deleteTarget.id).subscribe({
      next: () => { this.showDeleteModal = false; this.saving = false; this.deleteTarget = null; this.load(); },
      error: () => { this.saving = false; }
    });
  }

  // Add member
  openAddMember(t: Team): void {
    this.addMemberTarget = t;
    this.selectedUserId = '';
    this.showAddMemberModal = true;
  }

  addMember(): void {
    if (!this.addMemberTarget || !this.selectedUserId) return;
    this.saving = true;
    this.teamService.addMember(this.addMemberTarget.id, this.selectedUserId).subscribe({
      next: () => {
        this.showAddMemberModal = false;
        this.saving = false;
        this.loadMembers(this.addMemberTarget!.id);
        this.addMemberTarget = null;
      },
      error: () => { this.saving = false; }
    });
  }

  removeMember(teamId: string, userId: string): void {
    this.teamService.removeMember(teamId, userId).subscribe({
      next: () => this.loadMembers(teamId),
      error: () => {}
    });
  }

  initials(name: string): string {
    return (name || '?').split(' ').map((w: string) => w[0]).slice(0, 2).join('').toUpperCase();
  }

  availableUsers(teamId: string): any[] {
    const memberIds = this.membersOf(teamId).map(m => m.userId);
    return this.allUsers.filter(u => !memberIds.includes(u.id));
  }
}
