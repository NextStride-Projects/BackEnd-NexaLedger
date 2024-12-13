namespace AuthAPI.Models;

public class AdminLoginRequest
{
    public required string AdminKey { get; set; }
    public required string Ip { get; set; }
}
