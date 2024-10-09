using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using RTSPlanner.Core.Resources;
using Xunit;

namespace RTSPlanner.Core.UnitTests.Resources;

[ExcludeFromCodeCoverage]
public sealed class CapacityResourceTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void Used_ShouldBeInitializedAsStartingUsed(int startingUsed)
    {
        var subject = new CapacityResource(startingUsed, 1, 1);

        subject.Used
            .Should()
            .Be(startingUsed);
    }
    
    [Fact]
    public void Used_ShouldNotifyWhenChanged()
    {
        var subject = new CapacityResource(0, 1, 1);

        using var monitor = subject.Monitor();
        
        subject.Use(1);
        
        monitor.Should().RaisePropertyChangeFor(x => x.Used);
    }
    
    [Fact]
    public void Used_ShouldNotNotifyWhenNotChanged()
    {
        var subject = new CapacityResource(1, 1, 1);

        using var monitor = subject.Monitor();
        
        subject.Use(0);
        
        monitor.Should().NotRaisePropertyChangeFor(x => x.Used);
    }
    
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void Maximum_ShouldBeInitializedAsStartingMaximum(int startingMaximum)
    {
        var subject = new CapacityResource(0, startingMaximum, 3);

        subject.Maximum
            .Should()
            .Be(startingMaximum);
    }
    
    [Fact]
    public void Maximum_ShouldNotifyWhenChanged()
    {
        var subject = new CapacityResource(0, 1, 10, 1);

        using var monitor = subject.Monitor();
        
        subject.FinishMaximumProduction(1);
        
        monitor.Should().RaisePropertyChangeFor(x => x.Maximum);
    }
    
    [Fact]
    public void Maximum_ShouldNotNotifyWhenNotChanged()
    {
        var subject = new CapacityResource(0, 1, 1, 1);

        using var monitor = subject.Monitor();
        
        subject.FinishMaximumProduction(1);
        
        monitor.Should().NotRaisePropertyChangeFor(x => x.Maximum);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-2)]
    public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenStartingMaximumIsNegative(int startingMaximum)
    {
        var ctor = () => new CapacityResource(0, startingMaximum, 1);

        ctor.Should().Throw<ArgumentOutOfRangeException>();
    }
    
    [Theory]
    [InlineData(-1)]
    [InlineData(-2)]
    public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenStartingUsedIsNegative(int startingUsed)
    {
        var ctor = () => new CapacityResource(startingUsed, 1, 1);

        ctor.Should().Throw<ArgumentOutOfRangeException>();
    }
    
    [Theory]
    [InlineData(2, 1)]
    [InlineData(3, 2)]
    public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenStartingUsedIsGreaterThanStartingMaximum(
        int startingUsed,
        int startingMaximum)
    {
        var ctor = () => new CapacityResource(startingUsed, startingMaximum, 3);

        ctor.Should().Throw<ArgumentOutOfRangeException>();
    }
    
    [Theory]
    [InlineData(-1)]
    [InlineData(-2)]
    public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenStartingMaximumInProductionIsNegative(int startingMaximumInProduction)
    {
        var ctor = () => new CapacityResource(0, 1, 1, startingMaximumInProduction);

        ctor.Should().Throw<ArgumentOutOfRangeException>();
    }
    
    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 2)]
    public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenHardMaximumIsLessThanStartingMaximum(int hardMaximum, int startingMaximum)
    {
        var ctor = () => new CapacityResource(0, startingMaximum, hardMaximum);

        ctor.Should().Throw<ArgumentOutOfRangeException>();
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void Reset_ShouldSetUsedToStartingUsed(int startingUsed)
    {
        var subject = new CapacityResource(startingUsed, 2, 3);

        subject.Use(1);
        
        subject.Reset();
        
        subject.Used
            .Should()
            .Be(startingUsed);
    }
    
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void Reset_ShouldSetMaximumToStartingMaximum(int startingMaximum)
    {
        var subject = new CapacityResource(0, startingMaximum, 3);

        subject.StartMaximumProduction(1);
        subject.FinishMaximumProduction(1);
        
        subject.Reset();
        
        subject.Maximum
            .Should()
            .Be(startingMaximum);
    }
    
    [Fact]
    public void Reset_ShouldSetMaximumInProductionToStartingMaximumInProduction()
    {
        var subject = new CapacityResource(0, 1, 2, 1);

        subject.FinishMaximumProduction(1);
        
        subject.Reset();
        
        subject.FinishMaximumProduction(1);
        
        subject.Maximum
            .Should()
            .Be(2);
    }
    
    [Theory]
    [InlineData(true, 0, 1, 0, 1, 1)]
    [InlineData(false, 0, 1, 0, 1, 2)]
    [InlineData(true, 0, 1, 1, 1, 1)]
    [InlineData(false, 0, 1, 1, 1, 2)]
    [InlineData(false, 1, 1, 0, 1, 1)]
    [InlineData(false, 1, 1, 1, 1, 1)]
    [InlineData(true, 0, 1, 0, 2, 1)]
    [InlineData(false, 0, 1, 0, 2, 2)]
    [InlineData(true, 0, 1, 1, 2, 1)]
    [InlineData(true, 0, 1, 1, 2, 2)]
    [InlineData(false, 1, 1, 0, 2, 1)]
    [InlineData(true, 1, 1, 1, 2, 1)]
    [InlineData(false, 1, 1, 1, 2, 2)]
    [InlineData(true, 0, 2, 0, 2, 1)]
    [InlineData(true, 0, 2, 0, 2, 2)]
    [InlineData(true, 0, 2, 1, 2, 1)]
    [InlineData(true, 0, 2, 1, 2, 2)]
    [InlineData(false, 0, 2, 1, 2, 3)]
    [InlineData(true, 1, 2, 0, 2, 1)]
    [InlineData(false, 1, 2, 0, 2, 2)]
    [InlineData(true, 1, 2, 1, 2, 1)]
    [InlineData(false, 1, 2, 1, 2, 2)]
    [InlineData(true, 0, 2, 0, 3, 1)]
    [InlineData(true, 0, 2, 0, 3, 2)]
    [InlineData(false, 0, 2, 0, 3, 3)]
    [InlineData(true, 0, 2, 1, 3, 1)]
    [InlineData(true, 0, 2, 1, 3, 2)]
    [InlineData(true, 0, 2, 1, 3, 3)]
    [InlineData(false, 0, 2, 1, 3, 4)]
    [InlineData(true, 1, 2, 0, 3, 1)]
    [InlineData(false, 1, 2, 0, 3, 2)]
    [InlineData(true, 1, 2, 1, 3, 1)]
    [InlineData(true, 1, 2, 1, 3, 2)]
    [InlineData(false, 1, 2, 1, 3, 3)]
    public void CanUse_ShouldReturnExpectedResult(
        bool expected,
        int startingUsed,
        int startingMaximum,
        int startMaximumInProduction,
        int hardMaximum,
        int amount)
    {
        var subject = new CapacityResource(startingUsed, startingMaximum, hardMaximum, startMaximumInProduction);

        subject.CanUse(amount)
            .Should()
            .Be(expected);
    }
    
    [Theory]
    [InlineData(-1)]
    [InlineData(-2)]
    public void CanUse_ShouldThrowArgumentOutOfRangeException_WhenAmountIsLessThanZero(int amount)
    {
        var subject = new CapacityResource(0, 1, 1);

        subject.Invoking(x => x.CanUse(amount))
            .Should()
            .Throw<ArgumentOutOfRangeException>();
    }
    
    [Theory]
    [InlineData(true, 0, 1, 0, 1, 1)]
    [InlineData(false, 0, 1, 0, 1, 2)]
    [InlineData(true, 0, 1, 1, 1, 1)]
    [InlineData(false, 0, 1, 1, 1, 2)]
    [InlineData(false, 1, 1, 0, 1, 1)]
    [InlineData(false, 1, 1, 1, 1, 1)]
    [InlineData(true, 0, 1, 0, 2, 1)]
    [InlineData(false, 0, 1, 0, 2, 2)]
    [InlineData(true, 0, 1, 1, 2, 1)]
    [InlineData(false, 0, 1, 1, 2, 2)]
    [InlineData(false, 1, 1, 0, 2, 1)]
    [InlineData(false, 1, 1, 1, 2, 1)]
    [InlineData(true, 0, 2, 0, 2, 1)]
    [InlineData(true, 0, 2, 0, 2, 2)]
    [InlineData(true, 0, 2, 1, 2, 1)]
    [InlineData(true, 0, 2, 1, 2, 2)]
    [InlineData(false, 0, 2, 1, 2, 3)]
    [InlineData(true, 1, 2, 0, 2, 1)]
    [InlineData(false, 1, 2, 0, 2, 2)]
    [InlineData(true, 1, 2, 1, 2, 1)]
    [InlineData(false, 1, 2, 1, 2, 2)]
    [InlineData(true, 0, 2, 0, 3, 1)]
    [InlineData(true, 0, 2, 0, 3, 2)]
    [InlineData(false, 0, 2, 0, 3, 3)]
    [InlineData(true, 0, 2, 1, 3, 1)]
    [InlineData(true, 0, 2, 1, 3, 2)]
    [InlineData(false, 0, 2, 1, 3, 3)]
    [InlineData(true, 1, 2, 0, 3, 1)]
    [InlineData(false, 1, 2, 0, 3, 2)]
    [InlineData(true, 1, 2, 1, 3, 1)]
    [InlineData(false, 1, 2, 1, 3, 2)]
    public void CanUseNow_ShouldReturnExpectedResult(
        bool expected,
        int startingUsed,
        int startingMaximum,
        int startMaximumInProduction,
        int hardMaximum,
        int amount)
    {
        var subject = new CapacityResource(startingUsed, startingMaximum, hardMaximum, startMaximumInProduction);

        subject.CanUseNow(amount)
            .Should()
            .Be(expected);
    }
    
    [Theory]
    [InlineData(-1)]
    [InlineData(-2)]
    public void CanUseNow_ShouldThrowArgumentOutOfRangeException_WhenAmountIsLessThanZero(int amount)
    {
        var subject = new CapacityResource(0, 1, 1);

        subject.Invoking(x => x.CanUseNow(amount))
            .Should()
            .Throw<ArgumentOutOfRangeException>();
    }
    
    [Theory]
    [InlineData(2, 1, 1)]
    [InlineData(3, 2, 1)]
    [InlineData(3, 1, 2)]
    [InlineData(4, 2, 2)]
    public void Use_ShouldSetUsedToExpectedValue(int expected, int startingUsed, int amount)
    {
        var subject = new CapacityResource(startingUsed, 10, 10);

        subject.Use(amount);
        
        subject.Used
            .Should()
            .Be(expected);
    }
    
    [Theory]
    [InlineData(-1)]
    [InlineData(-2)]
    public void Use_ShouldThrowArgumentOutOfRangeException_WhenAmountIsLessThanZero(int amount)
    {
        var subject = new CapacityResource(0, 1, 1);

        subject.Invoking(x => x.Use(amount))
            .Should()
            .Throw<ArgumentOutOfRangeException>();
    }
    
    [Theory]
    [InlineData(0, 1, 2)]
    [InlineData(1, 1, 1)]
    [InlineData(1, 2, 2)]
    [InlineData(2, 2, 1)]
    public void Use_ShouldThrowApplicationException_WhenAmountIsNotAvailable(
        int startingUsed,
        int startingMaximum,
        int amount)
    {
        var subject = new CapacityResource(startingUsed, startingMaximum, 3);

        subject.Invoking(x => x.Use(amount))
            .Should()
            .Throw<ApplicationException>();
    }
    
    [Theory]
    [InlineData(0, 1, 1)]
    [InlineData(1, 1, 0)]
    [InlineData(0, 2, 2)]
    [InlineData(1, 2, 1)]
    [InlineData(2, 2, 0)]
    public void Free_ShouldNotThrowApplicationException_WhenAmountIsAvailable(
        int expected,
        int startingUsed,
        int amount)
    {
        var subject = new CapacityResource(startingUsed, 10, 10);

        subject.Free(amount);
        
        subject.Used
            .Should()
            .Be(expected);
    }
    
    [Theory]
    [InlineData(-1)]
    [InlineData(-2)]
    public void Free_ShouldThrowArgumentOutOfRangeException_WhenAmountIsLessThanZero(int amount)
    {
        var subject = new CapacityResource(0, 1, 1);

        subject.Invoking(x => x.Free(amount))
            .Should()
            .Throw<ArgumentOutOfRangeException>();
    }
    
    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 2)]
    public void Free_ShouldThrowApplicationException_WhenAmountIsGreaterThanUsed(int startingUsed, int amount)
    {
        var subject = new CapacityResource(startingUsed, 10, 10);

        subject.Invoking(x => x.Free(amount))
            .Should()
            .Throw<ApplicationException>();
    }
    
    [Fact]
    public void StartMaximumProduction_ShouldSetMaximumInProductionToExpectedValue()
    {
        var subject = new CapacityResource(0, 1, 10);

        subject.CanUse(2)
            .Should()
            .BeFalse();

        subject.StartMaximumProduction(1);
        
        subject.CanUse(2)
            .Should()
            .BeTrue();
    }
    
    [Theory]
    [InlineData(-1)]
    [InlineData(-2)]
    public void StartMaximumProduction_ShouldThrowArgumentOutOfRangeException_WhenAmountIsLessThanZero(int amount)
    {
        var subject = new CapacityResource(0, 1, 1);

        subject.Invoking(x => x.StartMaximumProduction(amount))
            .Should()
            .Throw<ArgumentOutOfRangeException>();
    }
    
    [Fact]
    public void CancelMaximumProduction_ShouldSetMaximumInProductionToExpectedValue()
    {
        var subject = new CapacityResource(0, 1, 10, 1);

        subject.CanUse(2)
            .Should()
            .BeTrue();

        subject.CancelMaximumProduction(1);
        
        subject.CanUse(2)
            .Should()
            .BeFalse();
    }
    
    [Theory]
    [InlineData(-1)]
    [InlineData(-2)]
    public void CancelMaximumProduction_ShouldThrowArgumentOutOfRangeException_WhenAmountIsLessThanZero(int amount)
    {
        var subject = new CapacityResource(0, 1, 10);

        subject.Invoking(x => x.CancelMaximumProduction(amount))
            .Should()
            .Throw<ArgumentOutOfRangeException>();
    }
    
    [Fact]
    public void CancelMaximumProduction_ShouldThrowApplicationException_WhenAmountIsGreaterThanMaximumInProduction()
    {
        var subject = new CapacityResource(0, 1, 10);

        subject.Invoking(x => x.CancelMaximumProduction(2))
            .Should()
            .Throw<ApplicationException>();
    }
    
    [Fact]
    public void FinishMaximumProduction_ShouldSetMaximumInProductionToExpectedValue()
    {
        var subject = new CapacityResource(0, 1, 10, 1);

        subject.CanUse(2)
            .Should()
            .BeTrue();

        subject.CanUseNow(2)
            .Should()
            .BeFalse();

        subject.FinishMaximumProduction(1);
        
        subject.CanUse(2)
            .Should()
            .BeTrue();

        subject.CanUseNow(2)
            .Should()
            .BeTrue();
    }
    
    [Theory]
    [InlineData(-1)]
    [InlineData(-2)]
    public void FinishMaximumProduction_ShouldThrowArgumentOutOfRangeException_WhenAmountIsLessThanZero(int amount)
    {
        var subject = new CapacityResource(0, 1, 10);

        subject.Invoking(x => x.FinishMaximumProduction(amount))
            .Should()
            .Throw<ArgumentOutOfRangeException>();
    }
    
    [Fact]
    public void FinishMaximumProduction_ShouldThrowApplicationException_WhenAmountIsGreaterThanMaximumInProduction()
    {
        var subject = new CapacityResource(0, 1, 10);

        subject.Invoking(x => x.FinishMaximumProduction(2))
            .Should()
            .Throw<ApplicationException>();
    }
}