using TaskManagement.Application.Services;
using Xunit;

namespace TaskManagement.Tests;

public class NaturalLanguageParserTests
{
    private readonly NaturalLanguageParser _parser = new();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ParseQuery_Fails_OnEmptyInput(string query)
    {
        var result = _parser.ParseQuery(query);

        Assert.False(result.Success);
        Assert.Equal("Abfrage ist leer", result.Error);
    }

    [Fact]
    public void ParseQuery_Fails_OnUnrecognizedQuery()
    {
        var result = _parser.ParseQuery("bestell mir bitte eine pizza");

        Assert.False(result.Success);
        Assert.Null(result.ToolName);
    }

    [Theory]
    [InlineData("Was hat Hans diese Woche gemacht?", "this")]
    [InlineData("Was hat Hans letzte Woche gemacht?", "last")]
    public void ParseQuery_German_WeeklyTasks(string query, string expectedWeek)
    {
        var result = _parser.ParseQuery(query);

        Assert.True(result.Success);
        Assert.Equal("get_user_weekly_tasks", result.ToolName);
        Assert.Equal("Hans", result.Parameters!["user_name"]);
        Assert.Equal(expectedWeek, result.Parameters!["week"]);
    }

    [Fact]
    public void ParseQuery_PreservesOriginalCasingOfUserName()
    {
        var result = _parser.ParseQuery("Was hat Hans diese Woche gemacht?");

        // Die Suche erfolgt in Kleinschreibung, der erfasste Wert behält seine Schreibweise.
        Assert.Equal("Hans", result.Parameters!["user_name"]);
    }

    [Fact]
    public void ParseQuery_WorkLogs_DefaultsToAllWeeks()
    {
        var result = _parser.ParseQuery("Zeige die Arbeitsprotokolle von Hans");

        Assert.True(result.Success);
        Assert.Equal("get_user_work_logs", result.ToolName);
        Assert.Equal("Hans", result.Parameters!["user_name"]);
        Assert.Equal("all", result.Parameters!["week"]);
    }

    [Fact]
    public void ParseQuery_ProjectTimeSpent()
    {
        var result = _parser.ParseQuery("Wie viel Zeit wurde für Alpha aufgewendet?");

        Assert.True(result.Success);
        Assert.Equal("get_project_time_spent", result.ToolName);
        Assert.Equal("Alpha", result.Parameters!["project_name"]);
    }

    [Fact]
    public void ParseQuery_ActiveWork()
    {
        var result = _parser.ParseQuery("Woran arbeitet Hans?");

        Assert.True(result.Success);
        Assert.Equal("get_user_active_work", result.ToolName);
        Assert.Equal("Hans", result.Parameters!["user_name"]);
    }

    [Fact]
    public void ParseQuery_StartWork_ExtractsTaskName()
    {
        var result = _parser.ParseQuery("Starte die Arbeit an Login");

        Assert.True(result.Success);
        Assert.Equal("start_task_work", result.ToolName);
        Assert.Equal("Login", result.Parameters!["task_name"]);
    }

    [Fact]
    public void ParseQuery_StopWork()
    {
        var result = _parser.ParseQuery("Arbeit stoppen");

        Assert.True(result.Success);
        Assert.Equal("stop_task_work", result.ToolName);
    }
}
