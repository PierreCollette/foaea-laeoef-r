﻿using FOAEA3.Business.Areas.Application;
using FOAEA3.Common;
using FOAEA3.Common.Helpers;
using FOAEA3.Model;
using FOAEA3.Model.Base;
using FOAEA3.Model.Constants;
using FOAEA3.Model.Enums;
using FOAEA3.Model.Interfaces.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FOAEA3.API.Tracing.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class TracingsController : FoaeaControllerBase
{
    [HttpGet("Version")]
    public ActionResult<string> GetVersion() => Ok("Tracings API Version 1.0");

    [HttpGet("DB")]
    [Authorize(Roles = Roles.Admin)]
    public ActionResult<string> GetDatabase([FromServices] IRepositories repositories) => Ok(repositories.MainDB.ConnectionString);

    [HttpGet("{key}", Name = "GetTracing")]
    public async Task<ActionResult<TracingApplicationData>> GetApplication(
                                    [FromRoute] string key,
                                    [FromServices] IRepositories repositories)
    {
        var applKey = new ApplKey(key);

        var manager = new TracingManager(repositories, config, User);

        bool success = await manager.LoadApplicationAsync(applKey.EnfSrv, applKey.CtrlCd);
        if (success)
        {
            if (manager.TracingApplication.AppCtgy_Cd == "T01")
                return Ok(manager.TracingApplication);
            else
                return NotFound($"Error: requested T01 application but found {manager.TracingApplication.AppCtgy_Cd} application.");
        }
        else
            return NotFound();

    }

    [HttpPost]
    public async Task<ActionResult<TracingApplicationData>> CreateApplication([FromServices] IRepositories db)
    {
        var tracingData = await APIBrokerHelper.GetDataFromRequestBodyAsync<TracingApplicationData>(Request);

        if (!APIHelper.ValidateApplication(tracingData, applKey: null, out string error))
            return UnprocessableEntity(error);

        var tracingManager = new TracingManager(tracingData, db, config, User);

        var submitter = (await db.SubmitterTable.GetSubmitterAsync(tracingData.Subm_SubmCd)).FirstOrDefault();
        if (submitter is not null)
        {
            tracingManager.CurrentUser.Submitter = submitter;
            db.CurrentSubmitter = submitter.Subm_SubmCd;
        }

        var appl = tracingManager.TracingApplication;

        bool isCreated = await tracingManager.CreateApplicationAsync();
        if (isCreated)
        {
            var appKey = $"{appl.Appl_EnfSrv_Cd}-{appl.Appl_CtrlCd}";

            return CreatedAtRoute("GetTracing", new { key = appKey }, appl);
        }
        else
        {
            return UnprocessableEntity(appl);
        }

    }

    [HttpPut("{key}")]
    [Produces("application/json")]
    public async Task<ActionResult<TracingApplicationData>> UpdateApplication(
                                                            [FromRoute] string key,
                                                            [FromQuery] string command,
                                                            [FromQuery] string enforcementServiceCode,
                                                            [FromServices] IRepositories repositories)
    {
        var applKey = new ApplKey(key);

        var application = await APIBrokerHelper.GetDataFromRequestBodyAsync<TracingApplicationData>(Request);

        if (!APIHelper.ValidateApplication(application, applKey, out string error))
            return UnprocessableEntity(error);

        var tracingManager = new TracingManager(application, repositories, config, User);

        if (string.IsNullOrEmpty(command))
            command = "";

        switch (command.ToLower())
        {
            case "":
                await tracingManager.UpdateApplicationAsync();
                break;

            case "partiallyserviceapplication":
                await tracingManager.PartiallyServiceApplicationAsync(enforcementServiceCode);
                break;

            //case "fullyserviceapplication":
            //    await tracingManager.FullyServiceApplicationAsync(enforcementServiceCode);
            //    break;

            default:
                application.Messages.AddSystemError($"Unknown command: {command}");
                return UnprocessableEntity(application);
        }

        if (!tracingManager.TracingApplication.Messages.ContainsMessagesOfType(MessageType.Error))
            return Ok(application);
        else
            return UnprocessableEntity(application);

    }

    [HttpPut("{key}/Transfer")]
    public async Task<ActionResult<TracingApplicationData>> Transfer([FromRoute] string key,
                                                 [FromServices] IRepositories repositories,
                                                 [FromQuery] string newRecipientSubmitter,
                                                 [FromQuery] string newIssuingSubmitter)
    {
        var applKey = new ApplKey(key);

        var application = await APIBrokerHelper.GetDataFromRequestBodyAsync<TracingApplicationData>(Request);

        if (!APIHelper.ValidateApplication(application, applKey, out string error))
            return UnprocessableEntity(error);

        var appManager = new TracingManager(application, repositories, config, User);

        await appManager.TransferApplicationAsync(newIssuingSubmitter, newRecipientSubmitter);

        return Ok(application);
    }

    [HttpPut("{key}/SINbypass")]
    public async Task<ActionResult<TracingApplicationData>> SINbypass([FromRoute] string key,
                                                          [FromServices] IRepositories repositories)
    {
        var applKey = new ApplKey(key);

        var sinBypassData = await APIBrokerHelper.GetDataFromRequestBodyAsync<SINBypassData>(Request);

        var application = new TracingApplicationData();

        var appManager = new TracingManager(application, repositories, config, User);

        await appManager.LoadApplicationAsync(applKey.EnfSrv, applKey.CtrlCd);

        var sinManager = new ApplicationSINManager(application, appManager);
        await sinManager.SINconfirmationBypassAsync(sinBypassData.NewSIN, repositories.CurrentSubmitter, false, sinBypassData.Reason);

        return Ok(application);
    }

    [HttpPut("{key}/CertifyAffidavit")]
    public async Task<ActionResult<TracingApplicationData>> CertifyAffidavit([FromRoute] string key,
                                                                 [FromServices] IRepositories repositories)
    {
        var applKey = new ApplKey(key);

        var application = new TracingApplicationData();

        var appManager = new TracingManager(application, repositories, config, User);

        await appManager.LoadApplicationAsync(applKey.EnfSrv, applKey.CtrlCd);

        await appManager.CertifyAffidavitAsync(repositories.CurrentSubmitter);

        return Ok(application);
    }

    [HttpGet("WaitingForAffidavits")]
    public async Task<ActionResult<DataList<TracingApplicationData>>> GetApplicationsWaitingForAffidavit(
                                                            [FromServices] IRepositories repositories)
    {
        var manager = new TracingManager(repositories, config, User);

        var data = await manager.GetApplicationsWaitingForAffidavitAsync();

        return Ok(data);
    }

    [HttpGet("TraceToApplication")]
    public async Task<ActionResult<List<TraceCycleQuantityData>>> GetTraceToApplData(
                                                            [FromServices] IRepositories repositories)
    {
        var manager = new TracingManager(repositories, config, User);

        var data = await manager.GetTraceToApplDataAsync();

        return Ok(data);
    }

}
