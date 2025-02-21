using eCommerce.API.Models;
using eCommerce.API.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

/*
* CRUD
* - GET -> Obter a lista de usuários
* - GET -> Obter o usuário passando o ID
* - POST -> Cadastrar um usuário
* - PUT -> Atualizar um usuário
* - DELETE -> Deletar um usuário
* 
* METHOD HTTP: www.minhaapi.com.br/api/Usuarios
* www.minhaapi.com.br/api/Usuarios/2
*/

namespace eCommerce.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsuariosController : ControllerBase
{
    private IUsuarioRepository _repository;
    public UsuariosController()
    {
        _repository = new UsuarioRepository();
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(_repository.Get());//HTTP - 200 -> Ok     deu certo
    }

    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        var usuario = _repository.Get(id);

        if (usuario == null)
        {
            return NotFound();//ERRO HTTP: 404 - Not Found      usuário não foi encontrado
        }

        return Ok(usuario);
    }

    [HttpPost]
    public IActionResult Insert([FromBody] Usuario usuario)
    {
        try
        {
            _repository.Insert(usuario);
            return Ok(usuario);
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }

    [HttpPut]
    public IActionResult Update([FromBody] Usuario usuario)
    {
        try
        {
            _repository.Update(usuario);
            return Ok(usuario);
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        _repository.Delete(id);
        return Ok();
    }


}

