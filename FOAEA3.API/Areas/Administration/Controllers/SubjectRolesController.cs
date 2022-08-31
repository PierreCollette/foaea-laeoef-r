﻿using FOAEA3.Model;
using FOAEA3.Model.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FOAEA3.API.Areas.Administration.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class SubjectRolesController : ControllerBase
{
    [HttpGet("Version")]
    public ActionResult<string> GetVersion() => Ok("SubjectRoles API Version 1.0");

    [HttpGet]
    public async Task<ActionResult<List<SubjectRoleData>>> GetSubjectRoles([FromServices] IRepositories repositories, [FromQuery] string subjectName)
    {
        return Ok(await repositories.SubjectRoleRepository.GetSubjectRolesAsync(subjectName));
    }

    [HttpGet("{subjectName}")]
    public async Task<ActionResult<List<string>>> GetAssumedRolesForSubject([FromServices] IRepositories repositories, [FromRoute] string subjectName)
    {
        return Ok(await repositories.SubjectRoleRepository.GetAssumedRolesForSubjectAsync(subjectName));
    }
}
