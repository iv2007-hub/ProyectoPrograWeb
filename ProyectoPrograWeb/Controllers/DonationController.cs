using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoPrograWeb.DTOs;
using ProyectoPrograWeb.models;
using ProyectoPrograWeb.services;

namespace ProyectoPrograWeb.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DonationController : ControllerBase
{
    private readonly DonationService _donationService;
    private readonly DeliveryService _deliveryService;

    public DonationController(DonationService donationService, DeliveryService deliveryService)
    {
        _donationService = donationService;
        _deliveryService = deliveryService;
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _donationService.GetPostById(id);
        if (result == null)
            return NotFound("Publicacion no encontrada");
        return Ok(result);
    }
    
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDonationPostDTo? dto)
    {
        if (dto == null)
            return BadRequest("Datos invalidos");
        
        var result = await _donationService.CreatePost(dto);
        return Ok(result);
    }
    
    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateDonationPostDTo? dto)
    {
        if (dto == null)
            return BadRequest("Datos invalidos");

        var donorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(donorId))
            return Unauthorized();
        
        var result = await _donationService.UpdatePost(id, donorId, dto);
        if (!result)
            return BadRequest("No se pudo actualizar la publicacion");

        return Ok("Publicacion actualizada correctamente");
    }
    
    [Authorize(Roles = "Admin")]
    [HttpPatch("{id}/deactivate")]
    public async Task<IActionResult> DeactivatePost(string id)
    {
        try
        {
            var result = await _donationService.AdminDeactivatePost(id);
            if (!result)
                return NotFound(new { mensaje = "Publicacion no encontrada" });

            return Ok(new { mensaje = "Publicacion desactivada correctamente" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
    
    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var donorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(donorId))
            return Unauthorized();

        var result = await _donationService.DesactivePost(id, donorId);
        if (!result)
            return BadRequest("No se pudo eliminar la publicacion");

        return Ok("Publicacion eliminada correctamente");
    }
    
    [HttpGet("active")]
    public async Task<IActionResult> GetActive([FromQuery] string? categoryId = null, [FromQuery] string? zone = null)
    {
        var result = await _donationService.GetAllActivePost(categoryId, zone);
        return Ok(result);
    }
    
    [Authorize]
    [HttpPost("{id}/schedule")]
    public async Task<IActionResult> ScheduleDelivery(string id, [FromBody] ScheduleDeliveryDTo dto)
    {
        var donorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(donorId))
            return Unauthorized();

        var result = await _donationService.ScheduleDelivery(id, donorId, dto);
        if (!result)
            return BadRequest(new { mensaje = "No se pudo coordinar la entrega" });

        return Ok(new { mensaje = "Entrega coordinada correctamente" });
    }
    
    [Authorize]
    [HttpGet("my-donations")]
    public async Task<IActionResult> GetMyDonations()
    {
        var donorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(donorId))
            return Unauthorized();
        
        var result = await _donationService.GetMyDonor(donorId);
        return Ok(result);
    }
    
    [Authorize]
    [HttpPost("{id}/reserve")]
    public async Task<IActionResult> Reserve(string id, [FromBody] SelectReceiverDTo dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.RequestId))
            return BadRequest("Request es obligatorio");
        
        var result = await _donationService.ChangeStatusToReserved(id, dto.RequestId);
        if (!result)
            return BadRequest("No se pudo reservar la publicacion");

        return Ok("Solicitud aceptada y publicacion reservada");
    }
    
    [Authorize]
    [HttpPost("{id}/deliver")]
    public async Task<IActionResult> Deliver(string id)
    {
        var result = await _donationService.ChangeStatusToDelivered(id);
        if (!result)
            return BadRequest("No se pudo marcar como entregada");

        return Ok("Publicacion entregada");
    }
    
    [Authorize(Roles = "Admin")]
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory()
    {
        var result = await _donationService.GetHistory();
        return Ok(result);
    }
}