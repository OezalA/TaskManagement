using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManagement.Application.Interfaces;
using TaskManagement.Application.Services;
using TaskManagement.MCP;

namespace TaskManagement.Api.Controllers
{
    [Authorize(Policy = "ApiAccess")]
    [ApiController]
    [Route("api/mcp")]
    public class MCPController : ControllerBase
    {
        private readonly MCPToolHandler _toolHandler;
        private readonly IUserResolutionService _userResolutionService;
        private readonly NaturalLanguageParser _nlpParser;

        public MCPController(
            MCPToolHandler toolHandler,
            IUserResolutionService userResolutionService)
        {
            _toolHandler = toolHandler;
            _userResolutionService = userResolutionService;
            _nlpParser = new NaturalLanguageParser();
        }

        /// <summary>
        /// Get MCP tools manifest
        /// </summary>
        [AllowAnonymous]
        [HttpGet("tools")]
        public IActionResult GetTools()
        {
            var manifestPath = System.IO.Path.Combine(
                System.AppDomain.CurrentDomain.BaseDirectory,
                "TaskManagement.MCP",
                "tools-manifest.json"
            );

            if (!System.IO.File.Exists(manifestPath))
            {
                return NotFound("Tools manifest not found");
            }

            var json = System.IO.File.ReadAllText(manifestPath);
            return Content(json, "application/json");
        }

        /// <summary>
        /// Execute an MCP tool
        /// </summary>
        [HttpPost("execute")]
        public async Task<IActionResult> ExecuteTool([FromBody] ToolRequest request)
        {
            if (string.IsNullOrEmpty(request.ToolName))
                return BadRequest("toolName is required");

            try
            {
                var result = await _toolHandler.HandleToolCall(request.ToolName, request.Parameters ?? new());
                return Ok(new { success = true, result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Query-style endpoint for natural language-like requests
        /// Example: "What did user X do this week" or "Hans bu hafta ne yapti?"
        /// </summary>
        [HttpPost("query")]
        public async Task<IActionResult> QueryNaturalLanguage([FromBody] MCPQuery query)
        {
            if (string.IsNullOrEmpty(query.Text))
                return BadRequest("text is required");

            try
            {
                // Parse natural language query
                var parseResult = _nlpParser.ParseQuery(query.Text);
                
                if (!parseResult.Success)
                    return BadRequest(new { success = false, error = parseResult.Error });

                // Resolve user names in parameters
                var resolvedParams = await ResolveParametersAsync(parseResult.Parameters ?? new());

                // Execute the tool
                var result = await _toolHandler.HandleToolCall(parseResult.ToolName!, resolvedParams);
                
                return Ok(new 
                { 
                    success = true, 
                    query = query.Text,
                    parsedTool = parseResult.ToolName,
                    result 
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Natural Language Query with auto-user detection
        /// Uses current user context if not specified
        /// </summary>
        [HttpPost("query-auto")]
        public async Task<IActionResult> QueryWithAutoUser([FromBody] string query)
        {
            if (string.IsNullOrEmpty(query))
                return BadRequest("query is required");

            try
            {
                // Get current user ID from context
                var currentUserService = HttpContext.RequestServices.GetRequiredService<ICurrentUserService>();
                var currentUser = await currentUserService.GetCurrentUserAsync();
                var currentUserId = currentUser?.Id;

                // Parse natural language query
                var parseResult = _nlpParser.ParseQuery(query);
                
                if (!parseResult.Success)
                    return BadRequest(new { success = false, error = parseResult.Error });

                // Resolve user names in parameters, use current user if not specified
                var resolvedParams = await ResolveParametersAsync(parseResult.Parameters ?? new(), currentUserId);

                // Execute the tool
                var result = await _toolHandler.HandleToolCall(parseResult.ToolName!, resolvedParams);
                
                return Ok(new 
                { 
                    success = true, 
                    query,
                    parsedTool = parseResult.ToolName,
                    result 
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Resolve user_name and project_name parameters to IDs
        /// </summary>
        private async Task<Dictionary<string, object>> ResolveParametersAsync(
            Dictionary<string, object> parameters,
            Guid? defaultUserId = null)
        {
            var resolved = new Dictionary<string, object>(parameters);

            // Resolve user_name -> user_id
            if (resolved.TryGetValue("user_name", out var userName) && userName is string userName_str)
            {
                var userId = await _userResolutionService.ResolveByPartialNameAsync(userName_str);
                if (userId.HasValue)
                {
                    resolved["user_id"] = userId.Value.ToString();
                    resolved.Remove("user_name");
                }
                else
                {
                    return resolved; // Return error will be caught by caller
                }
            }
            else if (defaultUserId.HasValue && !resolved.ContainsKey("user_id"))
            {
                // Use default user ID if available
                resolved["user_id"] = defaultUserId.Value.ToString();
            }

            // TODO: Resolve project_name -> project_id when project resolution is needed
            // TODO: Resolve task_name -> task_id when task resolution is needed

            return resolved;
        }
    }

    public class ToolRequest
    {
        public string? ToolName { get; set; }
        public Dictionary<string, object>? Parameters { get; set; }
    }

    public class MCPQuery
    {
        public string? Text { get; set; }
        public string? UserId { get; set; }
    }
}
