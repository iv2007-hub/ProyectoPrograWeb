using System.ComponentModel.DataAnnotations;

namespace ProyectoPrograWeb.DTOs;

public class getdonationpostDTo
{
    public string Id { get; set; } = string.Empty;
    public string DonorId { get; set; } = string.Empty;
    public string CategoryId { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string ItemCondition { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Zone { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public List<string> PhotoUrls { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}