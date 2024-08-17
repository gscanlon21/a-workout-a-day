using Microsoft.EntityFrameworkCore;

namespace Data.Code.Extensions;

public static class EFCoreExtensions
{
    public static bool IsDuplicateKeyException(this DbUpdateException dbUpdateException)
    {
        return dbUpdateException.InnerException?.Message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) ?? false;
    }
}
