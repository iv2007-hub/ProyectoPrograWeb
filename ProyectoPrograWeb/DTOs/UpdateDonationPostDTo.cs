using System.ComponentModel.DataAnnotations;

namespace ProyectoPrograWeb.DTOs;

public class UpdateDonationPostDTo
{  
    [Required]
    public string ItemName  { get; set; } = string.Empty;
    [Required]
    public string ItemCondition  { get; set; } = string.Empty;
    [Required]
    public string Description  { get; set; } = string.Empty;
    [Required]
    public string Zone { get; set; } = string.Empty;
    [Required]
    public string CategoryId  { get; set; } = string.Empty;
    public List<string> PhotoUrls { get; set; } = new();
}