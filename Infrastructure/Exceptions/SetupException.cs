using System;

namespace PlaywrightDemo.Infrastructure.Exceptions;

public class SetupException : Exception
{
    public SetupException(string message) : base(message) { }
    public SetupException(string message, Exception inner) : base(message, inner) { }
}