namespace webappAPI.DTOs
{
    public class CarSearchDto
    {
        public string? Make { get; set; }
        public string? Model { get; set; }
        public decimal MinPrice { get; set; } = 0;
        public decimal MaxPrice { get; set; } = 0;
        public int MinYear { get; set; } = 0;
        public int MaxYear { get; set; } = 0;
        public int MaxMileage { get; set; } = 0;
        public string? FuelType { get; set; }
        public string? Transmission { get; set; }
        public string? Color { get; set; }
        public string? Location { get; set; }
    }
}