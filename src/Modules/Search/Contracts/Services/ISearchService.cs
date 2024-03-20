namespace Pulse.Search.Contracts;

public interface ISearchService
{
    Task<IReadOnlyList<PostResult>> Search(string query);
    Task<IReadOnlyList<UserResult>> AutoComplete(string query);
}

public record PostResult(
    string Id,
    string Content,
    string AuthorId,
    string AuthorUserName,
    string AuthorDisplayName,
    DateTime CreatedAt
);

public record UserResult(string Id, string UserName, string DisplayName);
