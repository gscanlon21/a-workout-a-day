using Microsoft.EntityFrameworkCore;

namespace Data.Code.Extensions;

public static class EFCoreExtensions
{
    public static bool IsDuplicateKeyException(this DbUpdateException dbUpdateException, string? tableName = null)
    {
        if (!string.IsNullOrWhiteSpace(tableName))
        {
            return (dbUpdateException.InnerException?.Message.Contains($"_{tableName}_", StringComparison.OrdinalIgnoreCase) ?? false)
                && (dbUpdateException.InnerException?.Message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) ?? false);
        }

        return dbUpdateException.InnerException?.Message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) ?? false;
    }

    public static bool IsForeignKeyException(this DbUpdateException dbUpdateException, string? tableName = null)
    {
        if (!string.IsNullOrWhiteSpace(tableName))
        {
            return (dbUpdateException.InnerException?.Message.Contains($"\"{tableName}\"", StringComparison.OrdinalIgnoreCase) ?? false)
                && (dbUpdateException.InnerException?.Message.Contains("foreign key", StringComparison.OrdinalIgnoreCase) ?? false);
        }

        return dbUpdateException.InnerException?.Message.Contains("foreign key", StringComparison.OrdinalIgnoreCase) ?? false;
    }
}
