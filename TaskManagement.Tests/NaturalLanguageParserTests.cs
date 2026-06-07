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
        Assert.Equal("Query is empty", result.Error);
    }

    [Fact]
    public void ParseQuery_Fails_OnUnrecognizedQuery()
    {
        var result = _parser.ParseQuery("please order me a pizza");

        Assert.False(result.Success);
        Assert.Null(result.ToolName);
    }

    [Theory]
    [InlineData("What did Hans do this week?", "this")]
    [InlineData("What did Hans do last week?", "last")]
    public void ParseQuery_English_WeeklyTasks(string query, string expectedWeek)
    {
        var result = _parser.ParseQuery(query);

        Assert.True(result.Success);
        Assert.Equal("get_user_weekly_tasks", result.ToolName);
        Assert.Equal("Hans", result.Parameters!["user_name"]);
        Assert.Equal(expectedWeek, result.Parameters!["week"]);
    }

    [Theory]
    [InlineData("Hans bu hafta ne yapti?", "this")]
    [InlineData("Hans son hafta ne yapti?", "last")]
    public void ParseQuery_Turkish_WeeklyTasks(string query, string expectedWeek)
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
        var result = _parser.ParseQuery("What did Hans do this week?");

        // Matching is done lowercase, but the captured value keeps its casing.
        Assert.Equal("Hans", result.Parameters!["user_name"]);
    }

    [Fact]
    public void ParseQuery_WorkLogs_DefaultsToAllWeeks()
    {
        var result = _parser.ParseQuery("show Hans's work logs");

        Assert.True(result.Success);
        Assert.Equal("get_user_work_logs", result.ToolName);
        Assert.Equal("Hans", result.Parameters!["user_name"]);
        Assert.Equal("all", result.Parameters!["week"]);
    }

    [Fact]
    public void ParseQuery_ProjectTimeSpent()
    {
        var result = _parser.ParseQuery("How much time spent on Alpha?");

        Assert.True(result.Success);
        Assert.Equal("get_project_time_spent", result.ToolName);
        Assert.Equal("Alpha", result.Parameters!["project_name"]);
    }

    [Fact]
    public void ParseQuery_ActiveWork()
    {
        var result = _parser.ParseQuery("What is Hans working on?");

        Assert.True(result.Success);
        Assert.Equal("get_user_active_work", result.ToolName);
        Assert.Equal("Hans", result.Parameters!["user_name"]);
    }

    [Fact]
    public void ParseQuery_StartWork_ExtractsTaskName()
    {
        var result = _parser.ParseQuery("start work on Login");

        Assert.True(result.Success);
        Assert.Equal("start_task_work", result.ToolName);
        Assert.Equal("Login", result.Parameters!["task_name"]);
    }

    [Fact]
    public void ParseQuery_StopWork()
    {
        var result = _parser.ParseQuery("Stop working");

        Assert.True(result.Success);
        Assert.Equal("stop_task_work", result.ToolName);
    }
}
