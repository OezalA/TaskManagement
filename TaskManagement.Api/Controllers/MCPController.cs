using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManagement.MCP;

namespace TaskManagement.Api.Controllers
{
    [Authorize(Policy = "ApiAccess")]
    [ApiController]
    [Route("api/mcp")]
    public class MCPController : ControllerBase
    {
        private readonly MCPToolHandler _toolHandler;

        public MCPController(MCPToolHandler toolHandler)
        {
            _toolHandler = toolHandler;
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
        /// Example: "What did user X do this week"
        /// </summary>
        [HttpPost("query")]
        public async Task<IActionResult> QueryNaturalLanguage([FromBody] MCPQuery query)
        {
            if (string.IsNullOrEmpty(query.UserId))
                return BadRequest("userId is required");

            try
            {
                var result = await _toolHandler.HandleToolCall(
                    "get_user_weekly_tasks",
                    new Dictionary<string, object> { { "user_id", query.UserId }, { "week", "this" } }
                );
                return Ok(new { success = true, query = query.Text, result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
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
