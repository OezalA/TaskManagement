# Task Management Frontend

Modern Angular 17+ frontend for the Task Management API with NLP support for natural language queries.

## Features

### 🎯 Core Features
- **Dashboard** - Overview of tasks, projects, and team metrics
- **Task Management** - View, create, edit, and delete tasks
- **Project Management** - Manage projects and track time
- **User Management** - View team members and their work logs
- **Team Collaboration** - Team management and member coordination
- **Time Tracking** - Track work hours with start/stop functionality
- **Work Logs** - Detailed time tracking entries and reporting

### 🤖 AI/NLP Features
- **Natural Language Queries** - Ask questions in natural language
- **Multi-language Support** - Turkish and English
- **Auto User Detection** - Use current user context for queries
- **Smart Parsing** - Regex-based intent parsing
- **Examples**:
  - "What did I do this week?"
  - "Hans bu hafta ne yapti?" (Turkish: What did Hans do this week?)
  - "Show my work logs"
  - "How much time on ProjectX?"

### 🔐 Security
- **JWT Authentication** - Secure token-based authentication
- **Azure Entra ID Integration** - Support for Microsoft identity
- **Route Guards** - Protected routes for authenticated users
- **Authorization Interceptor** - Automatic token injection

### 🎨 Design
- **Material Design** - Clean, modern UI
- **Responsive Layout** - Works on desktop and mobile
- **Gradient Themes** - Professional color schemes
- **Real-time Feedback** - Loading states and error handling

## Project Structure

```
TaskManagement.Frontend/
├── src/
│   ├── app/
│   │   ├── core/
│   │   │   ├── services/        # API services
│   │   │   │   ├── api.service.ts
│   │   │   │   ├── task.service.ts
│   │   │   │   ├── project.service.ts
│   │   │   │   ├── user.service.ts
│   │   │   │   ├── team.service.ts
│   │   │   │   ├── worklog.service.ts
│   │   │   │   ├── mcp.service.ts
│   │   │   │   └── auth.service.ts
│   │   │   ├── interceptors/    # HTTP interceptors
│   │   │   │   └── auth.interceptor.ts
│   │   │   └── guards/          # Route guards
│   │   │       └── auth.guard.ts
│   │   ├── shared/
│   │   │   └── components/      # Shared components
│   │   │       └── navbar/
│   │   ├── features/
│   │   │   ├── auth/            # Authentication
│   │   │   ├── dashboard/       # Dashboard
│   │   │   ├── tasks/           # Task management
│   │   │   ├── projects/        # Project management
│   │   │   ├── users/           # User management
│   │   │   ├── time-tracking/   # Time tracking
│   │   │   └── nlp-query/       # NLP interface
│   │   ├── app.routes.ts        # Routing configuration
│   │   ├── app.component.ts     # Root component
│   │   └── app.component.css    # Global styles
│   ├── environments/             # Environment configs
│   │   └── environment.ts
│   ├── main.ts                  # Bootstrap
│   └── index.html               # HTML entry
├── package.json                 # Dependencies
├── tsconfig.json               # TypeScript config
└── README.md                   # This file
```

## Installation

### Prerequisites
- Node.js 18+
- npm 9+
- Angular CLI 17+

### Setup

```bash
# Navigate to frontend directory
cd TaskManagement.Frontend

# Install dependencies
npm install

# Start development server
npm start

# Build for production
npm run build
```

## Running the Application

### Development Server
```bash
npm start
```

The application will be available at `http://localhost:4200`

### Production Build
```bash
npm run build
```

Output will be in `dist/` directory.

## API Configuration

Update the API base URL in `src/environments/environment.ts`:

```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5253'  // Change to your API URL
};
```

## Authentication

### Getting a JWT Token

1. **Azure Entra ID Authentication:**
   ```powershell
   # Using Azure CLI
   az login
   
   # Get token
   az account get-access-token --resource "api://55739de4-48cd-4dc1-9067-06867aa3c9b3"
   ```

2. **Manual Token Request:**
   - Go to login page (`http://localhost:4200/login`)
   - Paste your JWT token
   - Click "Sign In"

3. **Token Requirements:**
   - Must contain `access_api` scope
   - Must be valid (not expired)
   - Format: `Bearer <token>`

## Available Endpoints

### Tasks
- `GET /api/tasks` - List all tasks
- `GET /api/tasks/:id` - Get task details
- `POST /api/tasks` - Create task
- `PUT /api/tasks/:id` - Update task
- `DELETE /api/tasks/:id` - Delete task
- `POST /api/tasks/:id/start-work` - Start work tracking
- `POST /api/tasks/:id/stop-work` - Stop work tracking
- `GET /api/tasks/active-work` - Get active work log
- `GET /api/tasks/this-week` - Get this week's tasks

### Projects
- `GET /api/projects` - List all projects
- `GET /api/projects/:id` - Get project details
- `POST /api/projects` - Create project
- `PUT /api/projects/:id` - Update project
- `DELETE /api/projects/:id` - Delete project
- `GET /api/projects/:id/total-time` - Get project time tracking

### Users
- `GET /api/users` - List all users
- `GET /api/users/:id` - Get user details
- `GET /api/users/:userId/worklogs?week=this|last|all` - Get work logs

### Teams
- `GET /api/teams` - List all teams
- `GET /api/teams/:id` - Get team details

### MCP (AI Query)
- `GET /api/mcp/tools` - Get available tools
- `POST /api/mcp/execute` - Execute tool
- `POST /api/mcp/query` - Natural language query
- `POST /api/mcp/query-auto` - Auto-detect user query

## NLP Query Examples

### English Queries
```
"What did I do this week?"
"Show my work logs"
"How much time on ProjectX?"
"What am I working on?"
"Show John's work logs"
```

### Turkish Queries
```
"Hans bu hafta ne yapti?" (What did Hans do this week?)
"Geçen hafta ne yaptin?" (What did you do last week?)
"Projem'de ne kadar zaman harcadim?" (How much time did I spend on project?)
```

## Services

### TaskService
Manages all task-related API calls
```typescript
constructor(private taskService: TaskService) {}

this.taskService.getTasks().subscribe(tasks => {
  // Handle tasks
});

this.taskService.startWork(taskId).subscribe(log => {
  // Work started
});
```

### MCPService
Handles natural language queries
```typescript
constructor(private mcpService: MCPService) {}

this.mcpService.queryAuto("What did I do this week?").subscribe(response => {
  // Handle response
});
```

### AuthService
Manages authentication
```typescript
constructor(private authService: AuthService) {}

this.authService.setToken(token);
this.authService.logout();
```

## Styling

Global styles in `src/app/app.component.css` with:
- Material Design principles
- CSS Grid and Flexbox
- Responsive breakpoints
- Color scheme:
  - Primary: `#667eea` (Indigo)
  - Secondary: `#764ba2` (Purple)
  - Accent: `#f093fb` (Pink)

## Components

### Login Component
- JWT token input
- Form validation
- Error handling
- Integration instructions

### Dashboard Component
- Overview cards (Tasks, Projects, Users)
- This week's tasks
- Quick action buttons
- Real-time metrics

### Task List Component
- Table with sorting/filtering
- Task status badges
- Edit/delete actions
- Create new task button

### NLP Query Component
- Natural language input
- Query parsing visualization
- Result formatting
- Example queries
- Multi-language support

## Development

### Adding a New Service
```typescript
// src/app/core/services/new.service.ts
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

@Injectable({ providedIn: 'root' })
export class NewService {
  constructor(private api: ApiService) {}
  
  getData(): Observable<any> {
    return this.api.get('api/endpoint');
  }
}
```

### Adding a New Component
```typescript
// src/app/features/new/new.component.ts
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-new',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './new.component.html',
  styleUrls: ['./new.component.css']
})
export class NewComponent implements OnInit {
  ngOnInit() {}
}
```

## Testing

```bash
# Run unit tests
npm test

# Run tests with coverage
ng test --code-coverage
```

## Troubleshooting

### 401 Unauthorized
- Check JWT token is valid
- Verify token has `access_api` scope
- Re-authenticate and get new token

### CORS Issues
- Ensure API has CORS enabled
- Check `apiUrl` in environment config

### NLP Query Not Working
- Verify API is running
- Check browser console for errors
- Ensure query format is recognized

## Browser Support
- Chrome (latest)
- Firefox (latest)
- Safari (latest)
- Edge (latest)

## Performance

- Lazy loading of feature modules
- OnPush change detection (where applicable)
- Lazy loaded routes
- Optimized bundle size
- Tree-shaking enabled

## Contributing

1. Follow Angular style guide
2. Use standalone components
3. Implement error handling
4. Add loading states
5. Test new features

## License

MIT

## Support

For issues and questions:
1. Check API logs
2. Review browser console
3. Check network requests (F12 DevTools)
4. Verify authentication status

## Next Steps

- [ ] Add more NLP patterns
- [ ] Implement project/task creation forms
- [ ] Add analytics dashboard
- [ ] Implement real-time updates (WebSockets)
- [ ] Add export functionality
- [ ] Implement notifications
