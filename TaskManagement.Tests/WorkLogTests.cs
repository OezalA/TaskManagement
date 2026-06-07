using System;
using TaskManagement.Domain.Entities;
using Xunit;

namespace TaskManagement.Tests;

public class WorkLogTests
{
    [Fact]
    public void DurationMinutes_IsNull_WhenWorkIsStillRunning()
    {
        var log = new WorkLog
        {
            StartTime = new DateTime(2026, 6, 7, 9, 0, 0),
            EndTime = null
        };

        Assert.Null(log.DurationMinutes);
    }

    [Fact]
    public void DurationMinutes_ReturnsElapsedMinutes_WhenFinished()
    {
        var start = new DateTime(2026, 6, 7, 9, 0, 0);
        var log = new WorkLog
        {
            StartTime = start,
            EndTime = start.AddMinutes(90)
        };

        Assert.Equal(90, log.DurationMinutes);
    }

    [Fact]
    public void DurationMinutes_TruncatesPartialMinutes()
    {
        var start = new DateTime(2026, 6, 7, 9, 0, 0);
        var log = new WorkLog
        {
            StartTime = start,
            EndTime = start.AddMinutes(45).AddSeconds(59)
        };

        // (int) cast truncates the trailing 59 seconds
        Assert.Equal(45, log.DurationMinutes);
    }

    [Fact]
    public void DurationMinutes_IsZero_WhenStartEqualsEnd()
    {
        var start = new DateTime(2026, 6, 7, 9, 0, 0);
        var log = new WorkLog { StartTime = start, EndTime = start };

        Assert.Equal(0, log.DurationMinutes);
    }
}
