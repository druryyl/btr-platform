using System;

namespace BtrReturGudang.Domain.Exceptions
{
    public class DomainException : Exception
    {
        public DomainException(string message) : base(message)
        {
        }

        public DomainException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Thrown when attempting to modify or delete a Return Order that has already been synchronized.
    /// Traceable to: BR-018, BR-020, TD-010.
    /// </summary>
    public class DocumentLockedException : InvalidOperationException
    {
        public DocumentLockedException(string message) : base(message)
        {
        }

        public DocumentLockedException(string returnOrderId, string action)
            : base($"Cannot perform '{action}' on ReturnOrder '{returnOrderId}' because it has already been synchronized and is locked (immutable).")
        {
        }
    }
}
