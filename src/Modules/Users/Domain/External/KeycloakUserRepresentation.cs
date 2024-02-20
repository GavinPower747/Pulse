namespace Pulse.Users.External;

public class KeycloakUserRepresentation
{
    public required string Id { get; set; }
    public required string Username { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public long CreatedTimestamp { get; set; }
}
