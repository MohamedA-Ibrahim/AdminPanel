namespace AdminPanel.Models;

public class GetUsersFilter
{
    public string? Search { get; set; }
    public bool OrderASC { get; set; }
    public GetUserOrderBy? OrderBy { get; set; }
}

public enum GetUserOrderBy
{
    FirstName,
    LastName,
    Email,
    Phone
}
