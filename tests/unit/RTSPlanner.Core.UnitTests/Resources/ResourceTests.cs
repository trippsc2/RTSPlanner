using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using RTSPlanner.Core.Resources;
using Xunit;

namespace RTSPlanner.Core.UnitTests.Resources;

[ExcludeFromCodeCoverage]
public sealed class ResourceTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void Current_ShouldBeInitializedAsStarting(int starting)
    {
        var subject = new Resource(starting: starting);

        subject.Current
            .Should()
            .Be(starting);
    }

    [Fact]
    public void Current_ShouldNotifyWhenChanged()
    {
        var subject = new Resource();

        using var monitor = subject.Monitor();
        
        subject.Add(1);
        
        monitor.Should().RaisePropertyChangeFor(x => x.Current);
    }
    
    [Fact]
    public void Current_ShouldNotNotifyWhenNotChanged()
    {
        var subject = new Resource(maximum: 1, starting: 1);

        using var monitor = subject.Monitor();
        
        subject.Add(1);
        
        monitor.Should().NotRaisePropertyChangeFor(x => x.Current);
    }
    
    [Theory]
    [InlineData(0, 0)]
    [InlineData(0, 1)]
    public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenMaximumIsLessThanOrEqualToMinimum(int maximum, int minimum)
    {
        var ctor = () => new Resource(maximum, minimum: minimum);

        ctor.Should().Throw<ArgumentOutOfRangeException>();
    }
    
    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 2)]
    public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenStartingIsLessThanMinimum(int starting, int minimum)
    {
        var ctor = () => new Resource(starting: starting, minimum: minimum);

        ctor.Should().Throw<ArgumentOutOfRangeException>();
    }
    
    [Theory]
    [InlineData(2, 1)]
    [InlineData(3, 2)]
    public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenStartingIsGreaterThanMaximum(int starting, int maximum)
    {
        var ctor = () => new Resource(starting: starting, maximum: maximum);

        ctor.Should().Throw<ArgumentOutOfRangeException>();
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void Reset_ShouldSetCurrentToStarting(int starting)
    {
        var subject = new Resource(starting: starting);
        
        subject.Add(1);

        subject.Reset();

        subject.Current
            .Should()
            .Be(starting);
    }
    
    [Theory]
    [InlineData(true, 0, 1)]
    [InlineData(false, 1, 1)]
    [InlineData(true, 0, 2)]
    [InlineData(false, 2, 2)]
    public void CanAdd_ShouldReturnExpectedResult(bool expected, int starting, int maximum)
    {
        var subject = new Resource(maximum, starting);

        subject.CanAdd()
            .Should()
            .Be(expected);
    }
    
    [Theory]
    [InlineData(true, 0, 1, 1)]
    [InlineData(false, 1, 1, 1)]
    [InlineData(true, 0, 2, 2)]
    [InlineData(false, 1, 2, 2)]
    public void CanAddWithoutExceedingMaximum_ShouldReturnExpectedResult(
        bool expected,
        int starting,
        int maximum,
        int amount)
    {
        var subject = new Resource(maximum, starting);

        subject.CanAddWithoutExceedingMaximum(amount)
            .Should()
            .Be(expected);
    }
    
    [Theory]
    [InlineData(-1)]
    [InlineData(-2)]
    public void CanAddWithoutExceedingMaximum_ShouldThrowArgumentOutOfRangeException_WhenAmountIsLessThanZero(int amount)
    {
        var subject = new Resource();

        subject.Invoking(x => x.CanAddWithoutExceedingMaximum(amount))
            .Should()
            .Throw<ArgumentOutOfRangeException>();
    }
    
    [Theory]
    [InlineData(1, 0, 2, 1)]
    [InlineData(2, 0, 2, 2)]
    [InlineData(2, 0, 2, 3)]
    [InlineData(2, 1, 3, 1)]
    [InlineData(3, 1, 3, 2)]
    [InlineData(3, 1, 3, 3)]
    public void Add_ShouldIncreaseCurrentByAmount_WhenAmountIsLessThanOrEqualToMaximum(
        int expected,
        int starting,
        int maximum,
        int amount)
    {
        var subject = new Resource(maximum, starting);

        subject.Add(amount);

        subject.Current
            .Should()
            .Be(expected);
    }
    
    [Theory]
    [InlineData(-1)]
    [InlineData(-2)]
    public void Add_ShouldThrowArgumentOutOfRangeException_WhenAmountIsLessThanZero(int amount)
    {
        var subject = new Resource();

        subject.Invoking(x => x.Add(amount))
            .Should()
            .Throw<ArgumentOutOfRangeException>();
    }
    
    [Theory]
    [InlineData(false, 0, 0, 0, 1)]
    [InlineData(true, 0, 0, 1, 1)]
    [InlineData(false, 0, 0, 1, 2)]
    [InlineData(true, 0, 0, 2, 2)]
    [InlineData(true, 1, 0, 0, 1)]
    [InlineData(true, 1, 0, 1, 1)]
    [InlineData(false, 1, 0, 0, 2)]
    [InlineData(true, 1, 0, 1, 2)]
    [InlineData(false, 1, 0, 0, 3)]
    [InlineData(true, 1, 0, 2, 3)]
    [InlineData(false, 1, 1, 0, 1)]
    [InlineData(true, 1, 1, 1, 1)]
    [InlineData(false, 1, 1, 0, 2)]
    [InlineData(false, 1, 1, 1, 2)]
    [InlineData(false, 1, 1, 0, 3)]
    [InlineData(false, 1, 1, 2, 3)]
    public void CanSpend_ShouldReturnExpectedResult(
        bool expected,
        int starting,
        int minimum,
        int addAmount,
        int amount)
    {
        var subject = new Resource(starting: starting, minimum: minimum);

        subject.Add(addAmount);

        subject.CanSpend(amount)
            .Should()
            .Be(expected);
    }
    
    [Theory]
    [InlineData(-1)]
    [InlineData(-2)]
    public void CanSpend_ShouldThrowArgumentOutOfRangeException_WhenAmountIsLessThanZero(int amount)
    {
        var subject = new Resource();

        subject.Invoking(x => x.CanSpend(amount))
            .Should()
            .Throw<ArgumentOutOfRangeException>();
    }
    
    [Theory]
    [InlineData(0, 0, 1, 1)]
    [InlineData(0, 0, 2, 2)]
    [InlineData(0, 1, 0, 1)]
    [InlineData(1, 1, 1, 1)]
    [InlineData(0, 1, 1, 2)]
    [InlineData(0, 1, 2, 3)]
    public void Spend_ShouldDecreaseCurrentByAmount(
        int expected,
        int starting,
        int addAmount,
        int amount)
    {
        var subject = new Resource(starting: starting);

        subject.Add(addAmount);

        subject.Spend(amount);

        subject.Current
            .Should()
            .Be(expected);
    }
    
    [Theory]
    [InlineData(-1)]
    [InlineData(-2)]
    public void Spend_ShouldThrowArgumentOutOfRangeException_WhenAmountIsLessThanZero(int amount)
    {
        var subject = new Resource();

        subject.Invoking(x => x.Spend(amount))
            .Should()
            .Throw<ArgumentOutOfRangeException>();
    }
}