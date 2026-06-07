import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ProjectService } from '@core/services/project.service';

@Component({
  selector: 'app-project-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './project-list.component.html',
  styleUrls: ['./project-list.component.css']
})
export class ProjectListComponent implements OnInit {
  projects: any[] = [];
  loading = true;
  error = '';

  showCreateModal = false;
  showEditModal = false;
  showDeleteModal = false;

  newProject = { name: '', description: '' };
  editProject: any = null;
  deleteTarget: any = null;
  saving = false;

  constructor(private projectService: ProjectService) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading = true;
    this.projectService.getProjects().subscribe({
      next: (p: any[]) => { this.projects = p; this.loading = false; },
      error: () => { this.error = 'Failed to load projects'; this.loading = false; }
    });
  }

  openCreate(): void {
    this.newProject = { name: '', description: '' };
    this.showCreateModal = true;
  }

  create(): void {
    if (!this.newProject.name.trim()) return;
    this.saving = true;
    this.projectService.createProject(this.newProject).subscribe({
      next: () => { this.showCreateModal = false; this.saving = false; this.load(); },
      error: () => { this.saving = false; }
    });
  }

  openEdit(p: any): void {
    this.editProject = { ...p };
    this.showEditModal = true;
  }

  update(): void {
    if (!this.editProject?.name?.trim()) return;
    this.saving = true;
    this.projectService.updateProject(this.editProject.id, this.editProject).subscribe({
      next: () => { this.showEditModal = false; this.saving = false; this.load(); },
      error: () => { this.saving = false; }
    });
  }

  openDelete(p: any): void {
    this.deleteTarget = p;
    this.showDeleteModal = true;
  }

  confirmDelete(): void {
    if (!this.deleteTarget) return;
    this.saving = true;
    this.projectService.deleteProject(this.deleteTarget.id).subscribe({
      next: () => { this.showDeleteModal = false; this.saving = false; this.deleteTarget = null; this.load(); },
      error: () => { this.saving = false; }
    });
  }

  formatDate(d: string): string {
    return new Date(d).toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' });
  }
}
