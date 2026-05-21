using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TaskManagement.Application.Services
{
    /// <summary>
    /// Parses natural language queries and extracts MCP tool parameters
    /// Example: "Hans bu hafta ne yapti?" -> get_user_weekly_tasks with week=this
    /// </summary>
    public class NaturalLanguageParser
    {
        // Pattern definitions for different query types
        private static readonly Dictionary<string, (string ToolName, Func<Match, Dictionary<string, object>> ExtractParams)> Patterns = new()
        {
            // "What did <user> do this week?" / "<user> bu hafta ne yapti?"
            ["what_did_user_do"] = (
                "get_user_weekly_tasks",
                m => new Dictionary<string, object>
                {
                    { "week", "this" }
                }
            ),

            // "What did <user> do last week?" / "<user> geçen hafta ne yapti?"
            ["what_did_user_do_last_week"] = (
                "get_user_weekly_tasks",
                m => new Dictionary<string, object>
                {
                    { "week", "last" }
                }
            ),

            // "How much time did <user> spend on <task>?" / "<user> <task>'te ne kadar zaman harcadi?"
            ["time_on_task"] = (
                "get_task_time_spent",
                m => new Dictionary<string, object>
                {
                    // Task name from capture group
                }
            ),

            // "How much time was spent on <project>?" / "<project>'te ne kadar zaman harcandi?"
            ["time_on_project"] = (
                "get_project_time_spent",
                m => new Dictionary<string, object>()
            ),

            // "Show <user>'s work logs" / "<user>'in çalişma günlüklerini göster"
            ["work_logs"] = (
                "get_user_work_logs",
                m => new Dictionary<string, object>
                {
                    { "week", "all" }
                }
            ),

            // "What is <user> working on?" / "<user> şu anda ne üzerinde çalişiyor?"
            ["active_work"] = (
                "get_user_active_work",
                m => new Dictionary<string, object>()
            ),

            // "Start work on <task>" / "<task>'te çalişmaya başla"
            ["start_work"] = (
                "start_task_work",
                m => new Dictionary<string, object>()
            ),

            // "Stop working" / "Çalişmayı durdur"
            ["stop_work"] = (
                "stop_task_work",
                m => new Dictionary<string, object>()
            ),
        };

        /// <summary>
        /// Parse natural language query and extract MCP tool call
        /// </summary>
        public ParseResult ParseQuery(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new ParseResult { Success = false, Error = "Query is empty" };

            query = query.Trim();

            // Try to match each pattern
            var result = MatchPattern(query);

            return result;
        }

        private ParseResult MatchPattern(string query)
        {
            var queryLower = query.ToLower();

            // Pattern: "what did X do (this/last) week?"
            if (Regex.IsMatch(queryLower, @"what\s+did\s+(\w+)\s+do\s+(this|last)?\s*week", RegexOptions.IgnoreCase))
            {
                var match = Regex.Match(queryLower, @"what\s+did\s+(\w+)\s+do\s+(this|last)?\s*week", RegexOptions.IgnoreCase);
                var userName = ExtractCapturedValue(query, match, 1);
                var week = match.Groups[2].Value ?? "this";

                return new ParseResult
                {
                    Success = true,
                    ToolName = "get_user_weekly_tasks",
                    Parameters = new Dictionary<string, object>
                    {
                        { "user_name", userName },
                        { "week", week.ToLower() }
                    },
                    Query = query
                };
            }

            // Pattern: "<user> bu hafta ne yapti?" (Turkish)
            if (Regex.IsMatch(queryLower, @"(\w+)\s+(bu|geçen|son)\s+hafta.*ne\s+yapti", RegexOptions.IgnoreCase))
            {
                var match = Regex.Match(queryLower, @"(\w+)\s+(bu|geçen|son)\s+hafta.*ne\s+yapti", RegexOptions.IgnoreCase);
                var userName = ExtractCapturedValue(query, match, 1);
                var period = match.Groups[2].Value.ToLower();
                var week = period switch
                {
                    "bu" => "this",
                    "geçen" or "son" => "last",
                    _ => "this"
                };

                return new ParseResult
                {
                    Success = true,
                    ToolName = "get_user_weekly_tasks",
                    Parameters = new Dictionary<string, object>
                    {
                        { "user_name", userName },
                        { "week", week }
                    },
                    Query = query
                };
            }

            // Pattern: "show <user>'s work logs / work history"
            if (Regex.IsMatch(queryLower, @"show\s+(\w+)'?s?\s+(work\s+)?logs", RegexOptions.IgnoreCase))
            {
                var match = Regex.Match(queryLower, @"show\s+(\w+)'?s?\s+(work\s+)?logs", RegexOptions.IgnoreCase);
                var userName = ExtractCapturedValue(query, match, 1);

                return new ParseResult
                {
                    Success = true,
                    ToolName = "get_user_work_logs",
                    Parameters = new Dictionary<string, object>
                    {
                        { "user_name", userName },
                        { "week", "all" }
                    },
                    Query = query
                };
            }

            // Pattern: "How much time on/spent on <project>?"
            if (Regex.IsMatch(queryLower, @"(how\s+)?much\s+time\s+(on|spent\s+on)\s+(\w+)", RegexOptions.IgnoreCase))
            {
                var match = Regex.Match(queryLower, @"(how\s+)?much\s+time\s+(on|spent\s+on)\s+(\w+)", RegexOptions.IgnoreCase);
                var projectName = ExtractCapturedValue(query, match, 3);

                return new ParseResult
                {
                    Success = true,
                    ToolName = "get_project_time_spent",
                    Parameters = new Dictionary<string, object>
                    {
                        { "project_name", projectName }
                    },
                    Query = query
                };
            }

            // Pattern: "What is/are <user> working on?"
            if (Regex.IsMatch(queryLower, @"what\s+is\s+(\w+)\s+working\s+on", RegexOptions.IgnoreCase))
            {
                var match = Regex.Match(queryLower, @"what\s+is\s+(\w+)\s+working\s+on", RegexOptions.IgnoreCase);
                var userName = ExtractCapturedValue(query, match, 1);

                return new ParseResult
                {
                    Success = true,
                    ToolName = "get_user_active_work",
                    Parameters = new Dictionary<string, object>
                    {
                        { "user_name", userName }
                    },
                    Query = query
                };
            }

            // Pattern: "start work on <task>" / "<task>'te çalişmaya başla"
            if (Regex.IsMatch(queryLower, @"(start\s+work\s+on|start\s+)(.*?)(\?|$)", RegexOptions.IgnoreCase))
            {
                var match = Regex.Match(queryLower, @"(start\s+work\s+on|start\s+)(.*?)(\?|$)", RegexOptions.IgnoreCase);
                var taskName = ExtractCapturedValue(query, match, 2)?.Trim();

                if (!string.IsNullOrEmpty(taskName))
                {
                    return new ParseResult
                    {
                        Success = true,
                        ToolName = "start_task_work",
                        Parameters = new Dictionary<string, object>
                        {
                            { "task_name", taskName }
                        },
                        Query = query
                    };
                }
            }

            // Pattern: "stop work / stop working / stop" (Turkish: "çalişmayı durdur")
            if (Regex.IsMatch(queryLower, @"(stop\s+(work|working)|çalişmayı\s+durdur|dur)", RegexOptions.IgnoreCase))
            {
                return new ParseResult
                {
                    Success = true,
                    ToolName = "stop_task_work",
                    Parameters = new Dictionary<string, object>(),
                    Query = query
                };
            }

            // No pattern matched
            return new ParseResult
            {
                Success = false,
                Error = $"Could not parse query: {query}",
                Query = query
            };
        }

        private static string? ExtractCapturedValue(string original, Match match, int groupIndex)
        {
            if (groupIndex < 0 || groupIndex >= match.Groups.Count)
                return null;

            var captured = match.Groups[groupIndex].Value;
            if (string.IsNullOrEmpty(captured))
                return null;

            // Return original case-preserved value from original string
            var originalMatch = Regex.Match(original, Regex.Escape(captured), RegexOptions.IgnoreCase);
            return originalMatch.Success ? originalMatch.Value : captured;
        }
    }

    /// <summary>
    /// Result of parsing natural language query
    /// </summary>
    public class ParseResult
    {
        public bool Success { get; set; }
        public string? ToolName { get; set; }
        public Dictionary<string, object>? Parameters { get; set; }
        public string? Error { get; set; }
        public string? Query { get; set; }

        public override string ToString()
        {
            if (!Success)
                return $"Parse failed: {Error}";

            var paramStr = string.Join(", ", Parameters?.Select(p => $"{p.Key}={p.Value}") ?? Enumerable.Empty<string>());
            return $"Tool: {ToolName}, Params: {paramStr}";
        }
    }
}
