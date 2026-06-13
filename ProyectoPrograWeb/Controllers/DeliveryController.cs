using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoPrograWeb.DTOs;
using ProyectoPrograWeb.models;
using ProyectoPrograWeb.services;

namespace ProyectoPrograWeb.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DeliveryController : ControllerBase
{
    private readonly DeliveryService _deliveryService;

    public DeliveryController(DeliveryService deliveryService)
    {
        _deliveryService = deliveryService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateDelivery([FromBody] DeliveryDTO deliveryDto, [FromQuery] string postId)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

        // 1. Buscar el post REAL en la base de datos, nunca confiar en el query
        var post = await _deliveryService.GetPostById(postId);
        if (post == null)
            return NotFound("La publicación no existe.");

        // 2. Validación de seguridad: solo el donante involucrado o un admin
        if (currentUserRole != "admin" && currentUserId != post.DonorId)
            return Forbid();

        // 3. Validación de doble confirmación (por ahora vienen del DTO)
        if (!deliveryDto.ConfirmedByDonor || !deliveryDto.ConfirmedByReceiver)
            return BadRequest("No se puede registrar la entrega sin la confirmación de ambas partes.");

        // 4. Evitar duplicados: ¿ya existe un log para este post?
        var existing = await _deliveryService.GetByPostId(post.PostId);
        if (existing != null)
            return Conflict("Ya existe un registro de entrega para esta publicación.");

        try
        {
            var record = await _deliveryService.Create(deliveryDto, post);
            return CreatedAtAction(nameof(GetHistory), new { userId = post.ReceiverId }, record);
        }
        catch (Exception ex)
        {
            // _logger.LogError(ex, "Error creando delivery log");
            return StatusCode(500, "Error interno al registrar la auditoría de entrega.");
        }
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetHistory(string userId)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

        if (currentUserRole != "admin" && currentUserId != userId)
            return Forbid();

        try
        {
            var history = await _deliveryService.GetByUser(userId);
            return Ok(history);
        }
        catch (Exception ex)
        {
            // _logger.LogError(ex, "Error recuperando historial");
            return StatusCode(500, "Error al recuperar el historial. {ex.Message} ");
        }
    } 
}
