using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoPrograWeb.DTOs;
using ProyectoPrograWeb.services;


namespace ProyectoPrograWeb.Controllers;



public class ExperimentController
{
 
[ApiController]
    [Route("api/[controller]")]
    // Authorize a nivel de clase significa que TODOS los endpoints de este controller
    // requieren un token JWT válido — sin token, el servidor devuelve 401 directamente
    [Authorize]
    public class _ExperimentController : ControllerBase
    {
        // El servicio que maneja la lógica de experimentos
        // Lo recibimos por inyección de dependencias igual que en AuthController
        private readonly ExperimentService _experimentService;

        public _ExperimentController(ExperimentService experimentService)
        {
            _experimentService = experimentService;
        }

        // HttpPost sin ruta adicional responde directamente en: POST /api/Experiment
        [HttpPost]
        public async Task<IActionResult> Create(
            // FromBody convierte el JSON del cuerpo de la petición al DTO
            // El DTO solo trae Title, Result y Success — el UserId lo sacamos del token
            [FromBody] ExperimentDTo dto)
        {
            try
            {
                // User es una propiedad de ControllerBase que contiene los claims del token JWT
                // ClaimTypes.NameIdentifier es el claim donde guardamos el Id del usuario
                // Así el usuario no puede falsificar su propio Id en el body
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // Si por alguna razón el claim no existe, rechazamos la petición
                // Esto no debería pasar si el token es válido, pero es buena práctica verificar
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var experiment = await _experimentService.Create(dto, userId);

                // Devolvemos el experimento creado completo con su Id generado
                return Ok(experiment);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // HttpGet sin ruta adicional responde en: GET /api/Experiment
        [HttpGet]
        public async Task<IActionResult> GetMyExperiments()
        {
            try
            {
                // Igual que en Create, sacamos el UserId del token JWT
                // No lo pedimos por URL ni por body — viene implícito en el token
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                // Traemos solo los experimentos que pertenecen a este usuario
                var experiments = await _experimentService.GetByUser(userId);

                // Ok() con la lista — puede ser una lista vacía si no tiene experimentos aún
                return Ok(experiments);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }   
}