namespace BlogBackend.Dtos.Auth
{
    public class AuthResponseDto
    {
        public string? Token { get; set; }
        public int Id { get; set; } // Renamed to Id for consistency
        public string Username { get; set; }
        public string Email { get; set; }
        public string? Message { get; set; }
        public List<string> Roles { get; set; }
    }
}
