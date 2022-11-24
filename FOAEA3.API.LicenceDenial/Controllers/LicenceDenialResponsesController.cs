﻿using FOAEA3.Business.Areas.Application;
using FOAEA3.Common;
using FOAEA3.Common.Helpers;
using FOAEA3.Model;
using FOAEA3.Model.Constants;
using FOAEA3.Model.Interfaces.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FOAEA3.API.LicenceDenial.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class LicenceDenialResponsesController : FoaeaControllerBase
{
    [HttpGet("Version")]
    public ActionResult<string> GetVersion() => Ok("LicenceDenialResponses API Version 1.0");

    [HttpGet("DB")]
    [Authorize(Roles = Roles.Admin)]
    public ActionResult<string> GetDatabase([FromServices] IRepositories repositories) => Ok(repositories.MainDB.ConnectionString);

    [HttpPost("bulk")]
    public async Task<ActionResult<int>> CreateLicenceDenialResponsesBulk([FromServices] IRepositories repositories)
    {
        var responseData = await APIBrokerHelper.GetDataFromRequestBodyAsync<List<LicenceDenialResponseData>>(Request);

        var licenceDenialManager = new LicenceDenialManager(repositories, config);

        await licenceDenialManager.CreateResponseDataAsync(responseData);

        var rootPath = "https://" + HttpContext.Request.Host.ToString();

        return Created(rootPath, new TraceResponseData());

    }

    [HttpPut("MarkResultsAsViewed")]
    public async Task<ActionResult<int>> MarkLicenceDenialResponsesAsViewed([FromServices] IRepositories repositories,
                                                                [FromQuery] string enfService)
    {
        var licenceDenialManager = new LicenceDenialManager(repositories, config);

        await licenceDenialManager.MarkResponsesAsViewedAsync(enfService);

        return Ok();
    }
}
