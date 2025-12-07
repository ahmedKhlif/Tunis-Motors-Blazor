namespace webappAPI.DTOs
{
    public class RoleDetailsDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int UserCount { get; set; }
        public List<UserManagementDto> Users { get; set; } = new List<UserManagementDto>();
    }
}
