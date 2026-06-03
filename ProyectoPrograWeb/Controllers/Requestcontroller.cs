using Microsoft.AspNetCore.Mvc;
using ProyectoPrograWeb.DTOs;
using ProyectoPrograWeb.services;

namespace ProyectoPrograWeb.Controllers;

/// <summary>
/// Controlador para manejar las solicitudes de donación en DonaCerca.
/// Permite a los receptores solicitar artículos y a los donantes
/// ver y gestionar las solicitudes que han recibido.
/// </summary>
[ApiController]
[Route("api/requests")]
public class RequestController : ControllerBase
{
    private readonly RequestService _requestService;

    public RequestController(RequestService requestService)
    {
        _requestService = requestService;
    }

    /// <summary>
    /// Crea una nueva solicitud de donación.
    /// POST /api/requests
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CrearSolicitud([FromBody] CreateDonationRequestDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resultado = await _requestService.CrearSolicitudAsync(
                dto.PostId,
                dto.ReceiverId,
                dto.ReceiverName
            );

            return CreatedAtAction(nameof(ObtenerSolicitudPorId),
                new { id = resultado.Id }, resultado);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    /// <summary>
    /// Devuelve todas las solicitudes de una publicación.
    /// GET /api/requests/{postId}
    /// </summary>
    [HttpGet("{postId}")]
    public async Task<IActionResult> ObtenerSolicitudesPorPost(string postId)
    {
        try
        {
            var solicitudes = await _requestService.ObtenerSolicitudesPorPostAsync(postId);
            return Ok(solicitudes);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    /// <summary>
    /// Devuelve todas las solicitudes del receptor.
    /// GET /api/requests/mine
    /// </summary>
    [HttpGet("mine")]
    public async Task<IActionResult> ObtenerMisSolicitudes([FromQuery] string receiverId)
    {
        try
        {
            if (string.IsNullOrEmpty(receiverId))
                return BadRequest(new { mensaje = "El id del receptor es requerido" });

            var solicitudes = await _requestService.ObtenerSolicitudesPorReceptorAsync(receiverId);
            return Ok(solicitudes);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    /// <summary>
    /// Busca una solicitud por su ID.
    /// GET /api/requests/detail/{id}
    /// </summary>
    [HttpGet("detail/{id}")]
    public async Task<IActionResult> ObtenerSolicitudPorId(string id)
    {
        try
        {
            var solicitud = await _requestService.ObtenerSolicitudPorIdAsync(id);
            return Ok(solicitud);
        }
        catch (Exception ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }

    /// <summary>
    /// Actualiza el estado de una solicitud.
    /// PUT /api/requests/{id}/status
    /// </summary>
    [HttpPut("{id}/status")]
    public async Task<IActionResult> ActualizarEstado(string id, [FromBody] UpdateDonationRequestDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resultado = await _requestService.ActualizarEstadoSolicitudAsync(id, dto.NuevoEstado);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    /// <summary>
    /// Cancela una solicitud pendiente.
    /// DELETE /api/requests/{id}/cancel
    /// </summary>
    [HttpDelete("{id}/cancel")]
    public async Task<IActionResult> CancelarSolicitud(string id, [FromQuery] string receiverId)
    {
        try
        {
            if (string.IsNullOrEmpty(receiverId))
                return BadRequest(new { mensaje = "El id del receptor es requerido" });

            var resultado = await _requestService.CancelarSolicitudAsync(id, receiverId);
            return Ok(new { mensaje = "Solicitud cancelada correctamente", exito = resultado });
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}