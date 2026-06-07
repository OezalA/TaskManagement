import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface Tool {
  name: string;
  description: string;
  inputSchema: any;
}

export interface ParseResult {
  success: boolean;
  toolName?: string;
  parameters?: Record<string, any>;
  error?: string;
  query?: string;
}

export interface MCPResponse {
  success: boolean;
  query?: string;
  parsedTool?: string;
  result?: any;
  error?: string;
}

@Injectable({
  providedIn: 'root'
})
export class MCPService {
  private readonly baseUrl = 'api/mcp';

  constructor(private api: ApiService) {}

  // Get available tools
  getTools(): Observable<any> {
    return this.api.get(`${this.baseUrl}/tools`);
  }

  // Execute a tool with parameters
  executeTool(toolName: string, parameters: Record<string, any>): Observable<MCPResponse> {
    return this.api.post<MCPResponse>(`${this.baseUrl}/execute`, {
      toolName,
      parameters
    });
  }

  // Natural language query
  query(text: string): Observable<MCPResponse> {
    return this.api.post<MCPResponse>(`${this.baseUrl}/query`, { text });
  }

  // Natural language query with auto user detection
  queryAuto(text: string): Observable<MCPResponse> {
    return this.api.post<MCPResponse>(`${this.baseUrl}/query-auto`, text);
  }
}
