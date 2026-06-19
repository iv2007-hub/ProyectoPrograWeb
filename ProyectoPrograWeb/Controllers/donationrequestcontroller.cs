using Microsoft.AspNetCore.Mvc;
using ProyectoPrograWeb.DTOs;
using ProyectoPrograWeb.models;
using ProyectoPrograWeb.services;

namespace ProyectoPrograWeb.Controllers;

[ApiController]
[Route("api/[controller]")]
public class Donationrequestcontroller : ControllerBase
{
    private readonly Donationrequestservice _donationrequestservice;

    public Donationrequestcontroller(Donationrequestservice donationrequestservice)
    {
        _donationrequestservice = donationrequestservice;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDonationRequestDTo dto)
    {
        var result = await _donationrequestservice.CreateRequest(dto);

        if (!result)
        {
            return BadRequest("Ya existe una solicitud pendiente");
        }
        return Ok("Request creado correctamente");
    }
    [HttpGet("post/{postId}")]
    public async Task<IActionResult> GetByPost(string postId)
    {
    var result = await _donationrequestservice.GetByPost(postId);
    return Ok(result);
    }
}