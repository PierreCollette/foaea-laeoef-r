﻿using FOAEA3.Business.Areas.Application;
using FOAEA3.Common;
using FOAEA3.Common.Helpers;
using FOAEA3.Model;
using FOAEA3.Model.Constants;
using FOAEA3.Model.Enums;
using FOAEA3.Model.Interfaces.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FOAEA3.API.LicenceDenial.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class LicenceDenialTerminationsController : FoaeaControllerBase
{
    [HttpGet("Version")]
    public ActionResult<string> GetVersion() => Ok("LicenceDenialTerminations API Version 1.0");

    [HttpGet("DB")]
    [Authorize(Roles = Roles.Admin)]
    public ActionResult<string> GetDatabase([FromServices] IRepositories repositories) => Ok(repositories.MainDB.ConnectionString);

    [HttpGet("{key}", Name = "GetApplication")]
    public async Task<ActionResult<LicenceDenialApplicationData>> GetApplication([FromRoute] ApplKey key,
                                                                     [FromServices] IRepositories repositories)
    {
        var manager = new LicenceDenialTerminationManager(repositories, config, User);

        bool success = await manager.LoadApplication(key.EnfSrv, key.CtrlCd);
        if (success)
        {
            if (manager.LicenceDenialTerminationApplication.AppCtgy_Cd == "L03")
                return Ok(manager.LicenceDenialTerminationApplication);
            else
                return NotFound($"Error: requested L03 application but found {manager.LicenceDenialTerminationApplication.AppCtgy_Cd} application.");
        }
        else
            return NotFound();

    }

    [HttpPost]
    public async Task<ActionResult<LicenceDenialApplicationData>> CreateApplication([FromServices] IRepositories db,
                                                                                    [FromQuery] string controlCodeForL01)
    {
        var application = await APIBrokerHelper.GetDataFromRequestBody<LicenceDenialApplicationData>(Request);
        var requestDate = application.LicSusp_TermRequestDte ?? DateTime.Now;

        if (!APIHelper.ValidateRequest(application, applKey: null, out string error))
            return UnprocessableEntity(error);

        var licenceDenialTerminationManager = new LicenceDenialTerminationManager(application, db, config, User);
        var submitter = (await db.SubmitterTable.GetSubmitter(application.Subm_SubmCd)).FirstOrDefault();
        if (submitter is not null)
        {
            licenceDenialTerminationManager.CurrentUser.Submitter = submitter;
            db.CurrentSubmitter = submitter.Subm_SubmCd;
        }

        bool isCreated = await licenceDenialTerminationManager.CreateApplication(controlCodeForL01, requestDate);
        if (isCreated)
        {
            var appKey = $"{application.Appl_EnfSrv_Cd}-{application.Appl_CtrlCd}";
            return CreatedAtRoute("GetApplication", new { key = appKey }, application);
        }
        else
            return UnprocessableEntity(application);
    }

    [HttpPut("{key}")]
    [Produces("application/json")]
    public async Task<ActionResult<LicenceDenialApplicationData>> UpdateApplication(
                                                [FromRoute] string key,
                                                [FromServices] IRepositories repositories)
    {
        var applKey = new ApplKey(key);

        var application = await APIBrokerHelper.GetDataFromRequestBody<LicenceDenialApplicationData>(Request);

        if (!APIHelper.ValidateRequest(application, applKey, out string error))
            return UnprocessableEntity(error);

        var licenceDenialManager = new LicenceDenialTerminationManager(application, repositories, config, User);

        await licenceDenialManager.UpdateApplication();

        if (!application.Messages.ContainsMessagesOfType(MessageType.Error))
            return Ok(application);
        else
            return UnprocessableEntity(application);

    }

    [HttpPut("{key}/Transfer")]
    public async Task<ActionResult<LicenceDenialApplicationData>> Transfer([FromRoute] string key,
                                                 [FromServices] IRepositories repositories,
                                                 [FromQuery] string newRecipientSubmitter,
                                                 [FromQuery] string newIssuingSubmitter)
    {
        var applKey = new ApplKey(key);

        var application = await APIBrokerHelper.GetDataFromRequestBody<LicenceDenialApplicationData>(Request);

        if (!APIHelper.ValidateRequest(application, applKey, out string error))
            return UnprocessableEntity(error);

        var appManager = new LicenceDenialTerminationManager(application, repositories, config, User);

        await appManager.TransferApplication(newIssuingSubmitter, newRecipientSubmitter);

        return Ok(application);
    }

    [HttpPut("{key}/cancel")]
    [Produces("application/json")]
    public async Task<ActionResult<InterceptionApplicationData>> CancelApplication([FromRoute] string key,
                                                                       [FromServices] IRepositories repositories)
    {
        var applKey = new ApplKey(key);

        var application = await APIBrokerHelper.GetDataFromRequestBody<LicenceDenialApplicationData>(Request);

        if (!APIHelper.ValidateRequest(application, applKey, out string error))
            return UnprocessableEntity(error);

        var licenceDenialTerminationManager = new LicenceDenialTerminationManager(application, repositories, config, User);
        await licenceDenialTerminationManager.CancelApplication();

        if (!licenceDenialTerminationManager.LicenceDenialTerminationApplication.Messages.ContainsMessagesOfType(MessageType.Error))
            return Ok(application);
        else
            return UnprocessableEntity(application);

    }


    [HttpPut("{key}/ProcessLicenceDenialTerminationResponse")]
    public async Task<ActionResult<LicenceDenialApplicationData>> ProcessLicenceDenialTerminationResponse(
                                                                        [FromRoute] ApplKey key,
                                                                        [FromServices] IRepositories repositories)
    {
        var application = new LicenceDenialApplicationData();

        var appManager = new LicenceDenialTerminationManager(application, repositories, config, User);

        if (await appManager.ProcessLicenceDenialTerminationResponse(key.EnfSrv, key.CtrlCd))
            return Ok(application);
        else
            return UnprocessableEntity(application);
    }

}
