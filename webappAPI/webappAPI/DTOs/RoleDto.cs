namespace webappAPI.DTOs
{
    public class RoleDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int UserCount { get; set; }
        public List<string> Users { get; set; } = new List<string>();
    }
}