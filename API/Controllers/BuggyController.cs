using System;
using API.Data;
using API.Entities;
using API.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class BuggyController(DataContext context) : BaseApiController
{
    [Authorize]
    [HttpGet("auth")]
    public ActionResult<string> GetAuth() 
    {
        return "Secret text for authenticated users";
    }
    
    [HttpGet("not-found")]
    public ActionResult<AppUser> GetNotFound() 
    {
        //ActionResult za vraćanje NotFound
        var thing = context.Users.Find(-1);
        if (thing == null) return NotFound();
        return thing;
    }
    
    [HttpGet("not-found-exception")]
    public ActionResult<AppUser> GetNotFoundException() 
    {
        //NotFoundException koji će uhvatiti middleware
        throw new NotFoundException("Traženi korisnik nije pronađen!");
    }
    
    [HttpGet("server-error")]
    public ActionResult<AppUser> GetServerError() 
    {
        //500 Internal Server Error
        throw new Exception("Dogodila se kritična greška na serveru");
    }
    
    [HttpGet("bad-request")]
    public ActionResult<string> GetBadRequest() 
    {
        //BadRequest preko ActionResult
        return BadRequest("Ovo je loš zahtev");
    }
    
    [HttpGet("bad-request-exception")]
    public ActionResult<string> GetBadRequestException() 
    {
        //BadRequestException koji će uhvatiti middleware
        throw new BadRequestException("Parametri zahteva nisu validni");
    }
    
    [HttpGet("unauthorized-exception")]
    public ActionResult<string> GetUnauthorizedException() 
    {
        //401 Unauthorized
        throw new UnauthorizedException("Niste autorizovani za pristup ovom resursu");
    }
    
    [HttpGet("forbidden-exception")]
    public ActionResult<string> GetForbiddenException() 
    {
        //403 Forbidden
        throw new ForbiddenException("Nemate dozvolu za pristup ovom resursu");
    }
    
    [HttpGet("timeout-exception")]
    public ActionResult<string> GetTimeoutException() 
    {
        //408 Request Timeout
        throw new TimeoutException("Zahtev je istekao");
    }
    
    [HttpGet("argument-exception")]
    public ActionResult<string> GetArgumentException() 
    {
        //400 Bad Request
        throw new ArgumentException("Nevažeći argument prosleđen");
    }
    
    [HttpGet("conflict-exception")]
    public ActionResult<string> GetConflictException() 
    {
        //409 Conflict
        throw new ConflictException("Došlo je do konflikta kod resursa");
    }
}
