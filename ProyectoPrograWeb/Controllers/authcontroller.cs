using Microsoft.AspNetCore.Mvc;
using ProyectoPrograWeb.services;
using ProyectoPrograWeb.DTOs;

namespace ProyectoPrograWeb.Controllers;

[ApiController]
[Route("api/auth")]
public class Authcontroller :  ControllerBase 
{
    private readonly Authservice _authservice;
    
    public Authcontroller(Authservice authservice)
    {
        _authservice = authservice;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDTo dTo)
    {
        var token = await _authservice.Login(dTo);
        return Ok(new {token});
    }

    [HttpPost("register")]
    public async Task<IActionResult> Registrer([FromBody] registerDTo dTo)
    {
        var user = await _authservice.Register(dTo);
        return Ok(user);
    }
}