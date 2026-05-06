# Phase 14 - MCP Integration Guide

## Overview
This phase exposes the TaskManagement API as LLM-compatible tools through the Model Context Protocol (MCP). LLMs can now query and execute tasks through natural language.

## MCP Endpoints

### 1. Get Available Tools
```
GET /api/mcp/tools
```
Returns the JSON manifest of all available MCP tools.

**Response:**
```json
{
  "tools": [
    {
      "name": "get_user_weekly_tasks",
      "description": "...",
      "inputSchema": { ... }
    },
    ...
  ]
}
```

### 2. Execute Tool
```
POST /api/mcp/execute
Content-Type: application/json
Authorization: Bearer <JWT_TOKEN>
```

**Request:**
```json
{
  "toolName": "get_user_weekly_tasks",
  "parameters": {
    "user_id": "550e8400-e29b-41d4-a716-446655440000",
    "week": "this"
  }
}
```

**Response:**
```json
{
  "success": true,
  "result": "[{...}]"  // JSON string with task data
}
```

### 3. Natural Language Query (Experimental)
```
POST /api/mcp/query
Content-Type: application/json
Authorization: Bearer <JWT_TOKEN>
```

**Request:**
```json
{
  "text": "What did Hans do this week?",
  "userId": "550e8400-e29b-41d4-a716-446655440000"
}
```

## Available Tools

### 1. get_user_weekly_tasks
Get all tasks a user worked on this week or last week.

**Parameters:**
- `user_id` (string, required): User UUID
- `week` (string): "this", "last" (default: "this")

**Returns:** Array of `WeeklyTaskDto` with task details and time spent

### 2. get_user_work_logs
Get detailed work log entries for a user.

**Parameters:**
- `user_id` (string, required): User UUID
- `week` (string): "this", "last", "all" (default: "all")

**Returns:** Object with userId, week, count, totalMinutes, and workLogs array

### 3. get_task_time_spent
Get total time spent on a task.

**Parameters:**
- `task_id` (string, required): Task UUID
- `user_id` (string, optional): Filter to specific user

**Returns:** `TaskTimeDto` with taskId, taskTitle, totalMinutes, logCount

### 4. get_project_time_spent
Get total time spent on a project.

**Parameters:**
- `project_id` (string, required): Project UUID
- `user_id` (string, optional): Filter to specific user

**Returns:** `ProjectTimeDto` with projectId, projectName, totalMinutes, taskCount, logCount

### 5. get_user_active_work
Get the currently active work log for a user.

**Parameters:**
- `user_id` (string, required): User UUID

**Returns:** Active work log details or null if no active work

### 6. start_task_work
Start tracking work time on a task.

**Parameters:**
- `task_id` (string, required): Task UUID
- `user_id` (string, required): User UUID

**Returns:** Newly created work log entry

### 7. stop_task_work
Stop tracking work time on the current task.

**Parameters:**
- `task_id` (string, required): Task UUID
- `user_id` (string, required): User UUID

**Returns:** Updated work log with end time and duration

## Authentication

All endpoints (except `/api/mcp/tools`) require a valid JWT token with the `ApiAccess` policy.

**Header:**
```
Authorization: Bearer <JWT_TOKEN>
```

The JWT token must contain:
- `scp` or `scope` claim with value containing `access_api`

### Getting a JWT Token

1. Authenticate with Azure Entra ID using your credentials
2. Request a token for the Task Management API
3. Include token in `Authorization` header

Example (PowerShell):
```powershell
$token = Get-AzAccessToken -ResourceUrl "api://55739de4-48cd-4dc1-9067-06867aa3c9b3"
$bearerToken = $token.Token
```

## Example Flows

### Flow 1: "What did Hans do this week?"

1. **Resolve** Hans's user ID (e.g., from user directory)
2. **Call** `get_user_weekly_tasks` with `user_id=hans-uuid`, `week=this`
3. **Parse** the response to get task list
4. **Format** results as natural language response

### Flow 2: "How much time was spent on Project X?"

1. **Resolve** Project X's project ID
2. **Call** `get_project_time_spent` with `project_id=x-uuid`
3. **Parse** totalMinutes and format as "X hours and Y minutes"
4. **Return** formatted response

### Flow 3: "Start working on Task Y"

1. **Resolve** Task Y's task ID and user ID
2. **Call** `start_task_work` with `task_id=y-uuid`, `user_id=user-uuid`
3. **Confirm** "Work started on Task Y"

### Flow 4: "Stop working"

1. **Get** current user's active work log via `get_user_active_work`
2. **Extract** task_id from active work log
3. **Call** `stop_task_work` with `task_id`, `user_id`
4. **Return** duration and task details

## Testing

### Using REST Client in VS Code

Open `TaskManagement.Api.http` and use the HTTP requests to test:

```
# Get tools
GET http://localhost:5275/api/mcp/tools

# Execute tool (requires auth token)
POST http://localhost:5275/api/mcp/execute
Authorization: Bearer <YOUR_TOKEN>
Content-Type: application/json

{
  "toolName": "get_user_weekly_tasks",
  "parameters": {
    "user_id": "550e8400-e29b-41d4-a716-446655440000",
    "week": "this"
  }
}
```

### Using cURL

```bash
# Get tools
curl http://localhost:5275/api/mcp/tools

# Execute tool
curl -X POST http://localhost:5275/api/mcp/execute \
  -H "Authorization: Bearer <JWT_TOKEN>" \
  -H "Content-Type: application/json" \
  -d '{
    "toolName": "get_user_weekly_tasks",
    "parameters": {
      "user_id": "550e8400-e29b-41d4-a716-446655440000",
      "week": "this"
    }
  }'
```

## Error Handling

### 401 Unauthorized
- Missing or invalid JWT token
- Token missing required `ApiAccess` policy

### 400 Bad Request
- Invalid toolName
- Missing required parameters
- Tool execution error

### Response Format:
```json
{
  "success": false,
  "error": "Error description"
}
```

## Implementation Details

### File Structure
- `TaskManagement.MCP/MCPToolHandler.cs` - Maps tool calls to API endpoints
- `TaskManagement.MCP/tools-manifest.json` - MCP tool definitions
- `TaskManagement.Api/Controllers/MCPController.cs` - API endpoints
- `TaskManagement.Api/appsettings.json` - MCP configuration

### Configuration

In `appsettings.json`:
```json
{
  "MCPSettings": {
    "ApiBaseUrl": "http://localhost:5253"
  }
}
```

Update `ApiBaseUrl` to match your API's actual base address.

## Next Steps

1. **LLM Integration**: Integrate these MCP tools with LLM providers (OpenAI, Azure OpenAI, etc.)
2. **Natural Language Parsing**: Implement patterns to map natural language queries to tool calls
3. **User Resolution**: Create service to resolve user names/emails to UUIDs
4. **Caching**: Add response caching for frequently accessed data
5. **Monitoring**: Log all tool calls for audit trails
6. **Rate Limiting**: Add rate limiting for MCP endpoints

## Troubleshooting

### "Unknown tool" Error
- Verify `toolName` matches exactly (case-sensitive)
- Check `tools-manifest.json` for available tools

### "Cannot read properties" Error
- Ensure parameters are in correct format
- Validate parameter types (string, number, etc.)

### No Response from Endpoint
- Check API is running on correct port
- Verify `MCPSettings:ApiBaseUrl` in appsettings.json
- Check firewall/network settings

## Security Considerations

1. **JWT Validation**: All tool execution requires valid JWT token
2. **Scope Validation**: Token must contain `access_api` scope
3. **User Context**: All queries are scoped to authenticated user
4. **Audit Logging**: Consider logging all tool calls for compliance
5. **Rate Limiting**: Implement rate limits to prevent abuse
