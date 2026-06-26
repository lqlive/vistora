namespace Nexova.Queries.Models;

public sealed record QueryDocumentRequest(
    string Name,
    string Sql,
    bool IsShared);
