using System;

namespace RTSPlanner.Notify;

/// <summary>
/// An attribute that marks a class as a notifying object.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class NotifyingObjectAttribute : Attribute;