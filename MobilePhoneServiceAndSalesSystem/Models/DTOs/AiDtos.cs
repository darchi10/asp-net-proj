using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MobilePhoneServiceAndSalesSystem.Models.DTOs
{
    public static class AiEntityTypes
    {
        public const string Product = "product";
        public const string Phone = "phone";
    }

    public sealed class AiEntityParseRequest
    {
        public string EntityType { get; set; } = string.Empty;
        public string Input { get; set; } = string.Empty;
    }

    public sealed class AiConfirmRequest
    {
        public string EntityType { get; set; } = string.Empty;
        public ProductDto? Product { get; set; }
        public PhoneDto? Phone { get; set; }
    }

    public sealed class AiParseResult
    {
        public string EntityType { get; set; } = string.Empty;
        public ProductDto? Product { get; set; }
        public PhoneAiPreviewDto? Phone { get; set; }
    }

    public sealed class PhoneAiPreviewDto
    {
        [Required]
        [StringLength(100)]
        public string Brand { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Model { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [JsonPropertyName("imei")]
        public string IMEI { get; set; } = string.Empty;

        [Range(1990, 2100)]
        public int YearOfManufacture { get; set; }

        [Required]
        [StringLength(100)]
        public string OperatingSystem { get; set; } = string.Empty;

        public int? CustomerId { get; set; }
        public string? CustomerText { get; set; }
        public string? CustomerSearchTerm { get; set; }
        public int? ExplicitId { get; set; }
        public bool RequiresCustomerSelection { get; set; }
    }
}
