using Nexova.Core.Entities;

namespace Nexova.Queries.Models;

public static class QueryDocumentMappingExtensions
{
    public static QueryDocumentResponse ToResponse(this QueryDocument document, Guid currentUserId) =>
        new(
            document.Id,
            document.UserId,
            document.Name,
            document.Sql,
            document.IsShared,
            document.UserId == currentUserId,
            document.CreatedAt,
            document.UpdatedAt);
}
