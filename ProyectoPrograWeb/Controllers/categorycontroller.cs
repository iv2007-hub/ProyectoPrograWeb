using Microsoft.AspNetCore.Mvc;
using ProyectoPrograWeb.DTOs;
using ProyectoPrograWeb.services;

namespace ProyectoPrograWeb.controllers;

[ApiController]
[Route("api/[controller]")] // Esto hace que la URL sea automáticamente: api/Category
public class categorycontroller : ControllerBase
{
    private readonly ICategoryService _categoryService;
    
    public categorycontroller(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    // Endpoint para obtener todas las categorias
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var categories = await _categoryService.GetAllAsync();
            return Ok(categories); // Devuelve un HTTP 200 con la lista de firestore
        }
        catch (Exception ex)
        {
            // Si algo falla con firebase, devolvemos un HTTP 500 con el error
            return StatusCode(500, new { message = "Error al obtener categorías", error = ex.Message });
        }
    }

    // Endpoint para crear una categoria
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] categorycreateDTo dto)
    {
        try
        {
            // Validamos que el frontend no nos mande un nombre vacío
            if (string.IsNullOrEmpty(dto.Name))
            {
                return BadRequest(new { message = "El nombre de la categoría es obligatorio." });
            }

            var result = await _categoryService.CreateAsync(dto);
            
            // Devuelve un HTTP 201 (Created) con el objeto recién guardado en firebase
            return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al crear la categoría", error = ex.Message });
        }
    }
}