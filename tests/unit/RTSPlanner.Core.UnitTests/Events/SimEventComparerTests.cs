using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using NSubstitute;
using RTSPlanner.Core.Events;
using Xunit;

namespace RTSPlanner.Core.UnitTests.Events;

[ExcludeFromCodeCoverage]
public sealed class SimEventComparerTests
{
    [Fact]
    public void Compare_ShouldReturnZero_WhenBothEventsAreNull()
    {
        var subject = new SimEventComparer();
        
        subject.Compare(null, null)
            .Should()
            .Be(0);
    }
    
    [Fact]
    public void Compare_ShouldReturnOne_WhenFirstEventIsNull()
    {
        var subject = new SimEventComparer();
        var simEvent = Substitute.For<ISimEvent>();
        
        subject.Compare(null, simEvent)
            .Should()
            .Be(1);
    }
    
    [Fact]
    public void Compare_ShouldReturnNegativeOne_WhenSecondEventIsNull()
    {
        var subject = new SimEventComparer();
        var simEvent = Substitute.For<ISimEvent>();
        
        subject.Compare(simEvent, null)
            .Should()
            .Be(-1);
    }
    
    [Fact]
    public void Compare_ShouldReturnZero_WhenBothEventsAreTheSame()
    {
        var subject = new SimEventComparer();
        var simEvent = Substitute.For<ISimEvent>();
        
        subject.Compare(simEvent, simEvent)
            .Should()
            .Be(0);
    }
    
    [Fact]
    public void Compare_ShouldReturnNegativeOne_WhenFirstEventIsBeforeSecondEvent()
    {
        var subject = new SimEventComparer();
        var simEvent1 = Substitute.For<ISimEvent>();
        var simEvent2 = Substitute.For<ISimEvent>();
        
        simEvent1.Time.Returns(TimeSpan.FromMinutes(5));
        simEvent2.Time.Returns(TimeSpan.FromMinutes(10));
        
        subject.Compare(simEvent1, simEvent2)
            .Should()
            .Be(-1);
    }
    
    [Fact]
    public void Compare_ShouldReturnOne_WhenFirstEventIsAfterSecondEvent()
    {
        var subject = new SimEventComparer();
        var simEvent1 = Substitute.For<ISimEvent>();
        var simEvent2 = Substitute.For<ISimEvent>();
        
        simEvent1.Time.Returns(TimeSpan.FromMinutes(10));
        simEvent2.Time.Returns(TimeSpan.FromMinutes(5));
        
        subject.Compare(simEvent1, simEvent2)
            .Should()
            .Be(1);
    }
}