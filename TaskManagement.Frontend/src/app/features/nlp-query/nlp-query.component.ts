import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MCPService } from '@core/services/mcp.service';
import { UserService } from '@core/services/user.service';
import { ProjectService } from '@core/services/project.service';

interface HistoryEntry {
  query: string;
  tool: string;
  result: any;
  error?: string;
  ts: Date;
}

@Component({
  selector: 'app-nlp-query',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './nlp-query.component.html',
  styleUrls: ['./nlp-query.component.css']
})
export class NLPQueryComponent implements OnInit {
  mode: 'nl' | 'tool' = 'nl';

  // NL query
  query = '';
  loading = false;
  result: any = null;
  parsedTool = '';
  error = '';

  // Direct tool
  tools: { name: string; label: string; params: ToolParam[] }[] = [
    {
      name: 'get_user_work_logs',
      label: 'Arbeitsprotokolle',
      params: [
        { key: 'user_id', label: 'Benutzer', type: 'user', required: true, value: '' },
        { key: 'week', label: 'Zeitraum', type: 'select', required: false, value: 'all', options: ['all', 'this', 'last'] }
      ]
    },
    {
      name: 'get_user_weekly_tasks',
      label: 'Wöchentliche Aufgaben',
      params: [
        { key: 'user_id', label: 'Benutzer', type: 'user', required: true, value: '' },
        { key: 'week', label: 'Woche', type: 'select', required: false, value: 'this', options: ['this', 'last'] }
      ]
    },
    {
      name: 'get_user_active_work',
      label: 'Aktive Arbeit',
      params: [
        { key: 'user_id', label: 'Benutzer', type: 'user', required: true, value: '' }
      ]
    },
    {
      name: 'get_project_time_spent',
      label: 'Aufgewendete Projektzeit',
      params: [
        { key: 'project_id', label: 'Projekt', type: 'project', required: true, value: '' }
      ]
    }
  ];
  selectedTool = this.tools[0];
  toolLoading = false;
  toolResult: any = null;
  toolError = '';

  users: any[] = [];
  projects: any[] = [];
  history: HistoryEntry[] = [];

  chips = [
    'Was hat Hans diese Woche gemacht?',
    'Zeige die Arbeitsprotokolle von Anna',
    'Woran arbeitet Hans?',
    'Wie viel Zeit wurde für Backend aufgewendet?'
  ];

  constructor(
    private mcpService: MCPService,
    private userService: UserService,
    private projectService: ProjectService
  ) {}

  ngOnInit(): void {
    this.userService.getUsers().subscribe({ next: u => this.users = u, error: () => {} });
    this.projectService.getProjects().subscribe({ next: p => this.projects = p, error: () => {} });
  }

  setMode(m: 'nl' | 'tool'): void {
    this.mode = m;
    this.result = null;
    this.toolResult = null;
    this.error = '';
    this.toolError = '';
  }

  useChip(q: string): void {
    this.query = q;
    this.submitQuery();
  }

  submitQuery(): void {
    if (!this.query.trim()) return;
    this.loading = true;
    this.error = '';
    this.result = null;
    this.parsedTool = '';

    this.mcpService.query(this.query).subscribe({
      next: res => {
        this.loading = false;
        if (res.success) {
          this.parsedTool = res.parsedTool || '';
          this.result = this.parseResult(res.result);
          this.history.unshift({ query: this.query, tool: this.parsedTool, result: this.result, ts: new Date() });
          if (this.history.length > 8) this.history.pop();
        } else {
          this.error = res.error || 'Abfrage konnte nicht verarbeitet werden.';
        }
      },
      error: err => {
        this.loading = false;
        this.error = err?.error?.error || err?.message || 'Anfrage fehlgeschlagen.';
      }
    });
  }

  runTool(): void {
    const params: Record<string, any> = {};
    for (const p of this.selectedTool.params) {
      if (p.value) params[p.key] = p.value;
    }
    this.toolLoading = true;
    this.toolError = '';
    this.toolResult = null;

    this.mcpService.executeTool(this.selectedTool.name, params).subscribe({
      next: res => {
        this.toolLoading = false;
        if (res.success) {
          this.toolResult = this.parseResult(res.result);
          this.history.unshift({ query: `[Tool] ${this.selectedTool.label}`, tool: this.selectedTool.name, result: this.toolResult, ts: new Date() });
          if (this.history.length > 8) this.history.pop();
        } else {
          this.toolError = res.error || 'Tool-Ausführung fehlgeschlagen.';
        }
      },
      error: err => {
        this.toolLoading = false;
        this.toolError = err?.error?.error || err?.message || 'Anfrage fehlgeschlagen.';
      }
    });
  }

  private parseResult(raw: any): any {
    if (!raw) return null;
    if (typeof raw === 'string') {
      try { return JSON.parse(raw); } catch { return { _raw: raw }; }
    }
    return raw;
  }

  selectHistory(h: HistoryEntry): void {
    if (h.query.startsWith('[Tool]')) return;
    this.query = h.query;
    this.mode = 'nl';
    this.result = h.result;
    this.parsedTool = h.tool;
    this.error = '';
  }

  userName(id: string): string {
    return this.users.find(u => u.id === id)?.displayName || id;
  }

  formatTime(d: string): string {
    if (!d) return '';
    return new Date(d).toLocaleString('de-DE', { day: '2-digit', month: 'short', hour: '2-digit', minute: '2-digit' });
  }

  resultIsWorkLogs(): boolean {
    return this.activeResult?.workLogs !== undefined;
  }

  resultIsTasks(): boolean {
    return this.activeResult?.tasks !== undefined;
  }

  resultIsTime(): boolean {
    return this.activeResult?.totalMinutes !== undefined;
  }

  get activeResult(): any {
    return this.mode === 'nl' ? this.result : this.toolResult;
  }

  get activeError(): string {
    return this.mode === 'nl' ? this.error : this.toolError;
  }

  get activeLoading(): boolean {
    return this.mode === 'nl' ? this.loading : this.toolLoading;
  }

  get isRaw(): boolean {
    const r = this.activeResult;
    if (!r) return false;
    return !r.workLogs && !r.tasks && r.totalMinutes === undefined;
  }
}

interface ToolParam {
  key: string;
  label: string;
  type: 'user' | 'project' | 'select' | 'text';
  required: boolean;
  value: string;
  options?: string[];
}
