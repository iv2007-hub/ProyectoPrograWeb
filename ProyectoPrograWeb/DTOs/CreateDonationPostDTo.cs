using System.ComponentModel.DataAnnotations;

namespace ProyectoPrograWeb.DTOs;

public class CreateDonationPostDTo
{
    [Required]
    public string DonorId { get; set; } = string.Empty;
    [Required]
    public string DonorName { get; set; } = string.Empty;
    [Required]
    public string CategoryId { get; set; } = string.Empty;
    [Required]
    [MinLength(3)]
    public string ItemName { get; set; } = string.Empty;
    [Required]
    public string Description { get; set; } = string.Empty;
    [Required]
    public string ItemCondition { get; set; } = string.Empty;
    [Required]
    public string Zone { get; set; } = string.Empty;

    public List<string> PhotoUrls { get; set; } = new();
}