namespace RTSPlanner.Notify;

/// <summary>
/// Specifies the access modifier of a property getter or setter.
/// </summary>
public enum Accessibility
{
    /// <summary>
    /// The getter or setter is accessible to all objects.
    /// </summary>
    Public,
    
    /// <summary>
    /// The getter or setter is accessible to non-derived classes in the same assembly and all derived classes.
    /// The getter or setter is not accessible to non-derived classes in other assemblies.
    /// </summary>
    ProtectedInternal,
    
    /// <summary>
    /// The getter or setter is accessible to derived classes in all assemblies.
    /// The getter or setter is not accessible to non-derived classes in any assembly.
    /// </summary>
    Protected,
    
    /// <summary>
    /// The getter or setter is accessible in the same assembly.
    /// The getter or setter is not accessible in other assemblies.
    /// </summary>
    Internal,
    
    /// <summary>
    /// The getter or setter is accessible to derived classes in the same assembly.
    /// The getter or setter is not accessible to derived classes in other assemblies or to non-derived classes.
    /// </summary>
    PrivateProtected,
    
    /// <summary>
    /// The getter or setter is accessible only within the containing type.
    /// </summary>
    Private
}