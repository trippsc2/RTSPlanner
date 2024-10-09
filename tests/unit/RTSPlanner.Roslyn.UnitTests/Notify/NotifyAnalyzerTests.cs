using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using RTSPlanner.Roslyn.Notify;
using Xunit;

namespace RTSPlanner.Roslyn.UnitTests.Notify;

[ExcludeFromCodeCoverage]
public sealed class NotifyAnalyzerTests
{
    [Fact]
    public async Task ShouldProduceNoErrors_WhenMarkedPartialClassWithMarkedField()
    {
        // lang=cs
        const string source =
            """
            using System;
            using RTSPlanner.Roslyn.Notify;
            
            namespace RTSPlanner.Roslyn.Notify
            {
                internal enum Accessibility
                {
                    Public,
                    ProtectedInternal,
                    Protected,
                    Internal,
                    PrivateProtected,
                    Private
                }
                
                [AttributeUsage(AttributeTargets.Class)]
                internal sealed class NotifyingObjectAttribute : Attribute
                {
                }
                
                [AttributeUsage(AttributeTargets.Field)]
                internal sealed class NotifyingPropertyAttribute : Attribute
                {
                    public Accessibility GetterAccessibility { get; set; } = Accessibility.Public;
                    public Accessibility SetterAccessibility { get; set; } = Accessibility.Public;
                }
            }
            
            namespace TestProject
            {
                [NotifyingObject]
                public partial class TestClass
                {
                    [NotifyingProperty]
                    private string _test;
                }
            }
            """;
        
        await AnalyzerTest<NotifyAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task ShouldProduceNoErrors_WhenMarkedPartialClassWithNoMarkedFields()
    {
        // lang=cs
        const string source =
            """
            using System;
            using RTSPlanner.Roslyn.Notify;
            
            namespace RTSPlanner.Roslyn.Notify
            {
                internal enum Accessibility
                {
                    Public,
                    ProtectedInternal,
                    Protected,
                    Internal,
                    PrivateProtected,
                    Private
                }
                
                [AttributeUsage(AttributeTargets.Class)]
                internal sealed class NotifyingObjectAttribute : Attribute
                {
                }
                
                [AttributeUsage(AttributeTargets.Field)]
                internal sealed class NotifyingPropertyAttribute : Attribute
                {
                    public Accessibility GetterAccessibility { get; set; } = Accessibility.Public;
                    public Accessibility SetterAccessibility { get; set; } = Accessibility.Public;
                }
            }
            
            namespace TestProject
            {
                [NotifyingObject]
                public partial class TestClass
                {
                    private string _test;
                }
            }
            """;

        await AnalyzerTest<NotifyAnalyzer>.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task ShouldProduceWarning_WhenUnmarkedClassWithMarkedField()
    {
        // lang=cs
        const string source =
            """
            using System;
            using RTSPlanner.Roslyn.Notify;
            
            namespace RTSPlanner.Roslyn.Notify
            {
                internal enum Accessibility
                {
                    Public,
                    ProtectedInternal,
                    Protected,
                    Internal,
                    PrivateProtected,
                    Private
                }
                
                [AttributeUsage(AttributeTargets.Class)]
                internal sealed class NotifyingObjectAttribute : Attribute
                {
                }
                
                [AttributeUsage(AttributeTargets.Field)]
                internal sealed class NotifyingPropertyAttribute : Attribute
                {
                    public Accessibility GetterAccessibility { get; set; } = Accessibility.Public;
                    public Accessibility SetterAccessibility { get; set; } = Accessibility.Public;
                }
            }
            
            namespace TestProject
            {
                public class TestClass
                {
                    [NotifyingProperty]
                    private string _test;
                }
            }
            """;
        
        await AnalyzerTest<NotifyAnalyzer>.VerifyAnalyzerAsync(
            source,
            new DiagnosticResult(NotifyAnalyzer.NotifyingPropertyOutsideOfNotifyingObject)
                .WithSpan(33, 10, 33, 27)
                .WithArguments("_test"));
    }

    [Fact]
    public async Task ShouldProduceError_WhenMarkedNonPartialClass()
    {
        // lang=cs
        const string source =
            """
            using System;
            using RTSPlanner.Roslyn.Notify;
            
            namespace RTSPlanner.Roslyn.Notify
            {
                internal enum Accessibility
                {
                    Public,
                    ProtectedInternal,
                    Protected,
                    Internal,
                    PrivateProtected,
                    Private
                }
                
                [AttributeUsage(AttributeTargets.Class)]
                internal sealed class NotifyingObjectAttribute : Attribute
                {
                }
                
                [AttributeUsage(AttributeTargets.Field)]
                internal sealed class NotifyingPropertyAttribute : Attribute
                {
                    public Accessibility GetterAccessibility { get; set; } = Accessibility.Public;
                    public Accessibility SetterAccessibility { get; set; } = Accessibility.Public;
                }
            }
            
            namespace TestProject
            {
                [NotifyingObject]
                public class TestClass;
            }
            """;

        await AnalyzerTest<NotifyAnalyzer>.VerifyAnalyzerAsync(
            source,
            new DiagnosticResult(NotifyAnalyzer.NotifyingObjectNotPartialType)
                .WithSpan(31, 6, 31, 21)
                .WithArguments("TestClass"));
    }

    [Fact]
    public async Task ShouldProduceError_WhenMarkedStaticPartialClass()
    {
        // lang=cs
        const string source =
            """
            using System;
            using RTSPlanner.Roslyn.Notify;
            
            namespace RTSPlanner.Roslyn.Notify
            {
                internal enum Accessibility
                {
                    Public,
                    ProtectedInternal,
                    Protected,
                    Internal,
                    PrivateProtected,
                    Private
                }
                
                [AttributeUsage(AttributeTargets.Class)]
                internal sealed class NotifyingObjectAttribute : Attribute
                {
                }
                
                [AttributeUsage(AttributeTargets.Field)]
                internal sealed class NotifyingPropertyAttribute : Attribute
                {
                    public Accessibility GetterAccessibility { get; set; } = Accessibility.Public;
                    public Accessibility SetterAccessibility { get; set; } = Accessibility.Public;
                }
            }
            
            namespace TestProject
            {
                [NotifyingObject]
                public static partial class TestClass;
            }
            """;

        await AnalyzerTest<NotifyAnalyzer>.VerifyAnalyzerAsync(
            source,
            new DiagnosticResult(NotifyAnalyzer.NotifyingObjectStaticType)
                .WithSpan(31, 6, 31, 21)
                .WithArguments("TestClass"));
    }

    [Theory]
    [InlineData(Accessibility.ProtectedInternal, Accessibility.Public)]
    [InlineData(Accessibility.Protected, Accessibility.Public)]
    [InlineData(Accessibility.Protected, Accessibility.ProtectedInternal)]
    [InlineData(Accessibility.Protected, Accessibility.Internal)]
    [InlineData(Accessibility.Internal, Accessibility.Public)]
    [InlineData(Accessibility.Internal, Accessibility.ProtectedInternal)]
    [InlineData(Accessibility.Internal, Accessibility.Protected)]
    [InlineData(Accessibility.PrivateProtected, Accessibility.Public)]
    [InlineData(Accessibility.PrivateProtected, Accessibility.ProtectedInternal)]
    [InlineData(Accessibility.PrivateProtected, Accessibility.Protected)]
    [InlineData(Accessibility.PrivateProtected, Accessibility.Internal)]
    [InlineData(Accessibility.Private, Accessibility.Public)]
    [InlineData(Accessibility.Private, Accessibility.ProtectedInternal)]
    [InlineData(Accessibility.Private, Accessibility.Protected)]
    [InlineData(Accessibility.Private, Accessibility.Internal)]
    [InlineData(Accessibility.Private, Accessibility.PrivateProtected)]
    public async Task ShouldProduceError_WhenSetterLessAccessibleThanGetter(
        Accessibility getterAccessibility,
        Accessibility setterAccessibility)
    {
        // lang=cs
        var source =
            $$"""
              using System;
              using RTSPlanner.Roslyn.Notify;
              
              namespace RTSPlanner.Roslyn.Notify
              {
                  internal enum Accessibility
                  {
                      Public,
                      ProtectedInternal,
                      Protected,
                      Internal,
                      PrivateProtected,
                      Private
                  }
                  
                  [AttributeUsage(AttributeTargets.Class)]
                  internal sealed class NotifyingObjectAttribute : Attribute
                  {
                  }
                  
                  [AttributeUsage(AttributeTargets.Field)]
                  internal sealed class NotifyingPropertyAttribute : Attribute
                  {
                      public Accessibility GetterAccessibility { get; set; } = Accessibility.Public;
                      public Accessibility SetterAccessibility { get; set; } = Accessibility.Public;
                  }
              }
              
              namespace TestProject
              {
                  [NotifyingObject]
                  public partial class TestClass
                  {
                      [NotifyingProperty(
                          GetterAccessibility = Accessibility.{{getterAccessibility}},
                          SetterAccessibility = Accessibility.{{setterAccessibility}})]
                      private string _test;
                  }
              }
              """;

        var endingCharacter = 50 + setterAccessibility.ToString().Length;

        await AnalyzerTest<NotifyAnalyzer>.VerifyAnalyzerAsync(
            source,
            new DiagnosticResult(NotifyAnalyzer.NotifyingPropertySetterLessAccessibleThanGetter)
                .WithSpan(34, 10, 36, endingCharacter)
                .WithArguments("_test"));
    }
    
    [Theory]
    [InlineData(Accessibility.Public, Accessibility.Public)]
    [InlineData(Accessibility.Public, Accessibility.ProtectedInternal)]
    [InlineData(Accessibility.Public, Accessibility.Protected)]
    [InlineData(Accessibility.Public, Accessibility.Internal)]
    [InlineData(Accessibility.Public, Accessibility.PrivateProtected)]
    [InlineData(Accessibility.Public, Accessibility.Private)]
    [InlineData(Accessibility.ProtectedInternal, Accessibility.ProtectedInternal)]
    [InlineData(Accessibility.ProtectedInternal, Accessibility.Protected)]
    [InlineData(Accessibility.ProtectedInternal, Accessibility.Internal)]
    [InlineData(Accessibility.ProtectedInternal, Accessibility.PrivateProtected)]
    [InlineData(Accessibility.ProtectedInternal, Accessibility.Private)]
    [InlineData(Accessibility.Protected, Accessibility.Protected)]
    [InlineData(Accessibility.Protected, Accessibility.PrivateProtected)]
    [InlineData(Accessibility.Protected, Accessibility.Private)]
    [InlineData(Accessibility.Internal, Accessibility.Internal)]
    [InlineData(Accessibility.Internal, Accessibility.PrivateProtected)]
    [InlineData(Accessibility.Internal, Accessibility.Private)]
    [InlineData(Accessibility.PrivateProtected, Accessibility.PrivateProtected)]
    [InlineData(Accessibility.PrivateProtected, Accessibility.Private)]
    [InlineData(Accessibility.Private, Accessibility.Private)]
    public async Task ShouldProduceNoErrors_WhenSetterMoreAccessibleThanGetter(
        Accessibility getterAccessibility,
        Accessibility setterAccessibility)
    {
        // lang=cs
        var source =
            $$"""
              using System;
              using RTSPlanner.Roslyn.Notify;
              
              namespace RTSPlanner.Roslyn.Notify
              {
                  internal enum Accessibility
                  {
                      Public,
                      ProtectedInternal,
                      Protected,
                      Internal,
                      PrivateProtected,
                      Private
                  }
                  
                  [AttributeUsage(AttributeTargets.Class)]
                  internal sealed class NotifyingObjectAttribute : Attribute
                  {
                  }
                  
                  [AttributeUsage(AttributeTargets.Field)]
                  internal sealed class NotifyingPropertyAttribute : Attribute
                  {
                      public Accessibility GetterAccessibility { get; set; } = Accessibility.Public;
                      public Accessibility SetterAccessibility { get; set; } = Accessibility.Public;
                  }
              }
              
              namespace TestProject
              {
                  [NotifyingObject]
                  public partial class TestClass
                  {
                      [NotifyingProperty(
                          GetterAccessibility = Accessibility.{{getterAccessibility}},
                          SetterAccessibility = Accessibility.{{setterAccessibility}})]
                      private string _test;
                  }
              }
              """;

        await AnalyzerTest<NotifyAnalyzer>.VerifyAnalyzerAsync(source);
    }
    
    [Fact]
    public async Task ShouldProduceError_WhenFieldNameDoesNotStartWithUnderscore()
    {
        // lang=cs
        const string source =
            """
            using System;
            using RTSPlanner.Roslyn.Notify;
            
            namespace RTSPlanner.Roslyn.Notify
            {
                internal enum Accessibility
                {
                    Public,
                    ProtectedInternal,
                    Protected,
                    Internal,
                    PrivateProtected,
                    Private
                }
                
                [AttributeUsage(AttributeTargets.Class)]
                internal sealed class NotifyingObjectAttribute : Attribute
                {
                }
                
                [AttributeUsage(AttributeTargets.Field)]
                internal sealed class NotifyingPropertyAttribute : Attribute
                {
                    public Accessibility GetterAccessibility { get; set; } = Accessibility.Public;
                    public Accessibility SetterAccessibility { get; set; } = Accessibility.Public;
                }
            }
            
            namespace TestProject
            {
                [NotifyingObject]
                public partial class TestClass
                {
                    [NotifyingProperty]
                    private string test;
                }
            }
            """;
        
        await AnalyzerTest<NotifyAnalyzer>.VerifyAnalyzerAsync(
            source,
            new DiagnosticResult(NotifyAnalyzer.NotifyingFieldNameMustStartWithUnderscore)
                .WithSpan(34, 10, 34, 27)
                .WithArguments("test"));
    }
}