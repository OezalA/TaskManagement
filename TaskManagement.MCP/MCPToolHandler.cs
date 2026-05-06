using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace TaskManagement.MCP
{
    /// <summary>
    /// MCP Handler - Maps tool calls to TaskManagement API endpoints
    /// </summary>
    public class MCPToolHandler
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;

        public MCPToolHandler(HttpClient httpClient, string apiBaseUrl)
        {
            _httpClient = httpClient;
            _apiBaseUrl = apiBaseUrl.TrimEnd('/');
        }

        public async Task<string> HandleToolCall(string toolName, Dictionary<string, object> parameters)
        {
            return toolName switch
            {
                "get_user_weekly_tasks" => await GetUserWeeklyTasks(parameters),
                "get_user_work_logs" => await GetUserWorkLogs(parameters),
                "get_task_time_spent" => await GetTaskTimeSpent(parameters),
                "get_project_time_spent" => await GetProjectTimeSpent(parameters),
                "get_user_active_work" => await GetUserActiveWork(parameters),
                "start_task_work" => await StartTaskWork(parameters),
                "stop_task_work" => await StopTaskWork(parameters),
                _ => throw new ArgumentException($"Unknown tool: {toolName}")
            };
        }

        private async Task<string> GetUserWeeklyTasks(Dictionary<string, object> parameters)
        {
            var userId = GetStringParam(parameters, "user_id");
            var week = GetStringParam(parameters, "week", "this");

            var url = $"{_apiBaseUrl}/api/tasks/this-week";
            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
                return $"Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}";

            var content = await response.Content.ReadAsStringAsync();
            return content;
        }

        private async Task<string> GetUserWorkLogs(Dictionary<string, object> parameters)
        {
            var userId = GetStringParam(parameters, "user_id");
            var week = GetStringParam(parameters, "week", "all");

            var url = $"{_apiBaseUrl}/api/users/{userId}/worklogs";
            if (!string.IsNullOrEmpty(week) && week != "all")
                url += $"?week={week}";

            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
                return $"Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}";

            var content = await response.Content.ReadAsStringAsync();
            return content;
        }

        private async Task<string> GetTaskTimeSpent(Dictionary<string, object> parameters)
        {
            var taskId = GetStringParam(parameters, "task_id");
            var userId = GetStringParam(parameters, "user_id", null);

            string url;
            if (!string.IsNullOrEmpty(userId))
                url = $"{_apiBaseUrl}/api/tasks/{taskId}/total-time";
            else
                url = $"{_apiBaseUrl}/api/tasks/{taskId}/total-time-all-users";

            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
                return $"Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}";

            var content = await response.Content.ReadAsStringAsync();
            return content;
        }

        private async Task<string> GetProjectTimeSpent(Dictionary<string, object> parameters)
        {
            var projectId = GetStringParam(parameters, "project_id");
            var userId = GetStringParam(parameters, "user_id", null);

            string url;
            if (!string.IsNullOrEmpty(userId))
                url = $"{_apiBaseUrl}/api/projects/{projectId}/total-time";
            else
                url = $"{_apiBaseUrl}/api/projects/{projectId}/total-time-all-users";

            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
                return $"Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}";

            var content = await response.Content.ReadAsStringAsync();
            return content;
        }

        private async Task<string> GetUserActiveWork(Dictionary<string, object> parameters)
        {
            var userId = GetStringParam(parameters, "user_id");
            
            var url = $"{_apiBaseUrl}/api/tasks/active-work";
            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
                return $"Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}";

            var content = await response.Content.ReadAsStringAsync();
            return content;
        }

        private async Task<string> StartTaskWork(Dictionary<string, object> parameters)
        {
            var taskId = GetStringParam(parameters, "task_id");
            var userId = GetStringParam(parameters, "user_id");

            var url = $"{_apiBaseUrl}/api/tasks/{taskId}/start-work";
            var content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);
            
            if (!response.IsSuccessStatusCode)
                return $"Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}";

            var responseContent = await response.Content.ReadAsStringAsync();
            return responseContent;
        }

        private async Task<string> StopTaskWork(Dictionary<string, object> parameters)
        {
            var taskId = GetStringParam(parameters, "task_id");
            var userId = GetStringParam(parameters, "user_id");

            var url = $"{_apiBaseUrl}/api/tasks/{taskId}/stop-work";
            var content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);
            
            if (!response.IsSuccessStatusCode)
                return $"Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}";

            var responseContent = await response.Content.ReadAsStringAsync();
            return responseContent;
        }

        private string GetStringParam(Dictionary<string, object> parameters, string key, string? defaultValue = null)
        {
            if (parameters.TryGetValue(key, out var value))
            {
                return value?.ToString() ?? defaultValue ?? "";
            }
            return defaultValue ?? "";
        }
    }
}
