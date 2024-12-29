namespace InventoryManagement.Core.Exceptions;

public class InventoryException : Exception
{
    public InventoryException(string message) : base(message)
    {
    }
}