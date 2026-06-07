using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TaskManagement.Application.Services
{
    /// <summary>
    /// Parst deutschsprachige Abfragen und extrahiert MCP-Tool-Parameter.
    /// Beispiel: "Was hat Hans diese Woche gemacht?" -> get_user_weekly_tasks mit week=this
    /// Hinweis: Die internen Parameterwerte (this/last/all) und Tool-Namen bleiben
    /// auf Englisch, da sie vom Tool-Handler konsumiert werden – nur die Eingabesprache ist Deutsch.
    /// </summary>
    public class NaturalLanguageParser
    {
        /// <summary>
        /// Parst die natürlichsprachige Abfrage und extrahiert den MCP-Tool-Aufruf.
        /// </summary>
        public ParseResult ParseQuery(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new ParseResult { Success = false, Error = "Abfrage ist leer" };

            query = query.Trim();

            return MatchPattern(query);
        }

        private ParseResult MatchPattern(string query)
        {
            var queryLower = query.ToLower();

            // Muster: "Was hat <Benutzer> diese/letzte Woche gemacht/getan?"
            if (Regex.IsMatch(queryLower, @"was\s+hat\s+(\w+)\s+(diese|letzte)\s+woche\s+(gemacht|getan)", RegexOptions.IgnoreCase))
            {
                var match = Regex.Match(queryLower, @"was\s+hat\s+(\w+)\s+(diese|letzte)\s+woche\s+(gemacht|getan)", RegexOptions.IgnoreCase);
                var userName = ExtractCapturedValue(query, match, 1);
                var period = match.Groups[2].Value.ToLower();
                var week = period == "letzte" ? "last" : "this";

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

            // Muster: "Zeige (die) Arbeitsprotokolle/Arbeitszeiten von <Benutzer>"
            if (Regex.IsMatch(queryLower, @"arbeits(?:protokolle?|zeiten)\s+von\s+(\w+)", RegexOptions.IgnoreCase))
            {
                var match = Regex.Match(queryLower, @"arbeits(?:protokolle?|zeiten)\s+von\s+(\w+)", RegexOptions.IgnoreCase);
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

            // Muster: "Wie viel Zeit wurde für <Projekt> aufgewendet?"
            if (Regex.IsMatch(queryLower, @"wie\s+viel\s+zeit\s+(?:wurde\s+)?(?:f(?:ü|ue)r)\s+(\w+)", RegexOptions.IgnoreCase))
            {
                var match = Regex.Match(queryLower, @"wie\s+viel\s+zeit\s+(?:wurde\s+)?(?:f(?:ü|ue)r)\s+(\w+)", RegexOptions.IgnoreCase);
                var projectName = ExtractCapturedValue(query, match, 1);

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

            // Muster: "Woran arbeitet <Benutzer> (gerade)?"
            if (Regex.IsMatch(queryLower, @"woran\s+arbeitet\s+(\w+)", RegexOptions.IgnoreCase))
            {
                var match = Regex.Match(queryLower, @"woran\s+arbeitet\s+(\w+)", RegexOptions.IgnoreCase);
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

            // Muster: "Starte (die) Arbeit an <Aufgabe>" / "Beginne mit <Aufgabe>"
            if (Regex.IsMatch(queryLower, @"(?:starte\s+(?:die\s+)?arbeit\s+an|beginne\s+(?:mit\s+)?)(.*?)(?:\?|$)", RegexOptions.IgnoreCase))
            {
                var match = Regex.Match(queryLower, @"(?:starte\s+(?:die\s+)?arbeit\s+an|beginne\s+(?:mit\s+)?)(.*?)(?:\?|$)", RegexOptions.IgnoreCase);
                var taskName = ExtractCapturedValue(query, match, 1)?.Trim();

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

            // Muster: "Arbeit stoppen" / "Stoppe" / "Höre auf zu arbeiten"
            if (Regex.IsMatch(queryLower, @"(arbeit\s+stoppen|stopp(?:e|en)?|aufh(?:ö|oe)ren|h(?:ö|oe)re?\s+auf)", RegexOptions.IgnoreCase))
            {
                return new ParseResult
                {
                    Success = true,
                    ToolName = "stop_task_work",
                    Parameters = new Dictionary<string, object>(),
                    Query = query
                };
            }

            // Kein Muster passt
            return new ParseResult
            {
                Success = false,
                Error = $"Abfrage konnte nicht verarbeitet werden: {query}",
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

            // Originale Groß-/Kleinschreibung aus der ursprünglichen Zeichenkette beibehalten
            var originalMatch = Regex.Match(original, Regex.Escape(captured), RegexOptions.IgnoreCase);
            return originalMatch.Success ? originalMatch.Value : captured;
        }
    }

    /// <summary>
    /// Ergebnis der natürlichsprachigen Abfrageverarbeitung
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
                return $"Verarbeitung fehlgeschlagen: {Error}";

            var paramStr = string.Join(", ", Parameters?.Select(p => $"{p.Key}={p.Value}") ?? Enumerable.Empty<string>());
            return $"Tool: {ToolName}, Parameter: {paramStr}";
        }
    }
}
