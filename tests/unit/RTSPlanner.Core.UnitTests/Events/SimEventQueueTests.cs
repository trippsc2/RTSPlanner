using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using NSubstitute;
using RTSPlanner.Core.Events;
using Xunit;

namespace RTSPlanner.Core.UnitTests.Events;

[ExcludeFromCodeCoverage]
public sealed class SimEventQueueTests
{
    [Fact]
    public void AddEvent_ShouldAddEventToQueue()
    {
        var subject = new SimEventQueue();
        var simEvent = Substitute.For<ISimEvent>();
        
        subject.AddEvent(simEvent);
        
        subject.DequeueNextEvent()
            .Should()
            .Be(simEvent);
    }
    
    [Fact]
    public void AddEvent_ShouldAddEventToQueueInOrder()
    {
        var subject = new SimEventQueue();
        var simEvent1 = Substitute.For<ISimEvent>();
        var simEvent2 = Substitute.For<ISimEvent>();
        
        simEvent1.Time.Returns(TimeSpan.FromMinutes(5));
        simEvent2.Time.Returns(TimeSpan.FromMinutes(10));
        
        subject.AddEvent(simEvent2);
        subject.AddEvent(simEvent1);
        
        subject.DequeueNextEvent()
            .Should()
            .Be(simEvent1);
        
        subject.DequeueNextEvent()
            .Should()
            .Be(simEvent2);
    }
    
    [Fact]
    public void AddEvents_ShouldAddEventsToQueueInOrder()
    {
        var subject = new SimEventQueue();
        var simEvent1 = Substitute.For<ISimEvent>();
        var simEvent2 = Substitute.For<ISimEvent>();
        
        simEvent1.Time.Returns(TimeSpan.FromMinutes(5));
        simEvent2.Time.Returns(TimeSpan.FromMinutes(10));
        
        subject.AddEvents(new[] { simEvent2, simEvent1 });
        
        subject.DequeueNextEvent()
            .Should()
            .Be(simEvent1);
        
        subject.DequeueNextEvent()
            .Should()
            .Be(simEvent2);
    }
    
    [Fact]
    public void RemoveEvent_ShouldRemoveEventFromQueue()
    {
        var subject = new SimEventQueue();
        var simEvent = Substitute.For<ISimEvent>();
        
        subject.AddEvent(simEvent);
        subject.RemoveEvent(simEvent);
        
        subject.DequeueNextEvent()
            .Should()
            .BeNull();
    }
    
    [Fact]
    public void DequeueNextEvent_ShouldReturnNullIfQueueIsEmpty()
    {
        var subject = new SimEventQueue();
        
        subject.DequeueNextEvent()
            .Should()
            .BeNull();
    }
    
    [Fact]
    public void DequeueNextEvent_ShouldReturnNextEventInQueue()
    {
        var subject = new SimEventQueue();
        var simEvent1 = Substitute.For<ISimEvent>();
        var simEvent2 = Substitute.For<ISimEvent>();
        
        simEvent1.Time.Returns(TimeSpan.FromMinutes(5));
        simEvent2.Time.Returns(TimeSpan.FromMinutes(10));
        
        subject.AddEvent(simEvent2);
        subject.AddEvent(simEvent1);
        
        subject.DequeueNextEvent()
            .Should()
            .Be(simEvent1);
        
        subject.DequeueNextEvent()
            .Should()
            .Be(simEvent2);
    }
    
    [Fact]
    public void DequeueNextEvent_ShouldRemoveEventFromQueue()
    {
        var subject = new SimEventQueue();
        var simEvent = Substitute.For<ISimEvent>();
        
        subject.AddEvent(simEvent);
        subject.DequeueNextEvent();
        
        subject.DequeueNextEvent()
            .Should()
            .BeNull();
    }
    
    [Fact]
    public void Clear_ShouldRemoveAllEventsFromQueue()
    {
        var subject = new SimEventQueue();
        var simEvent1 = Substitute.For<ISimEvent>();
        var simEvent2 = Substitute.For<ISimEvent>();
        
        subject.AddEvent(simEvent1);
        subject.AddEvent(simEvent2);
        subject.Clear();
        
        subject.DequeueNextEvent()
            .Should()
            .BeNull();
    }
}