using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoPrograWeb.DTOs;
using ProyectoPrograWeb.models;
using ProyectoPrograWeb.services;

namespace ProyectoPrograWeb.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class DeliveryController : ControllerBase
{
    private readonly DeliveryService _deliveryService;

    public DeliveryController(DeliveryService deliveryService)
    {
        _deliveryService = deliveryService;
    }

    // Historial de entregas de un usuario específico (donante o receptor)
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetHistory(string userId)
    {
        try
        {
            var history = await _deliveryService.GetByUser(userId);
            return Ok(history);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al recuperar el historial.", error = ex.Message });
        }
    }

    // Log de impacto completo, para reportes/dashboards de auditoría
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var history = await _deliveryService.GetAll();
            return Ok(history);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al recuperar el log de auditoría.", error = ex.Message });
        }
    }
}
