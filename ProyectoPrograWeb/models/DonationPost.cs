namespace ProyectoPrograWeb.models;

public class DonationPost
{
    public string Id { get; set; } = string.Empty;
    public string PostId { get; set; } = string.Empty; 
    public string DonorId { get; set; } = string.Empty;
    public string ReceiverId { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string CategoryId { get; set; } = string.Empty;
}