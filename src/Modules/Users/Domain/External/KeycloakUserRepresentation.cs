namespace Pulse.Users.External;

public class KeycloakUserRepresentation
{
    public required string Id { get; set; }
    public required string Username { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public long CreatedTimestamp { get; set; }
}
