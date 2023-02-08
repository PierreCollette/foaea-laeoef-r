﻿using FOAEA3.Model;
using FOAEA3.Model.Base;
using FOAEA3.Model.Enums;
using FOAEA3.Model.Interfaces.Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TestData.TestDB
{
    class InMemoryApplication : IApplicationRepository
    {
        public string CurrentSubmitter { get; set; }
        public string UserId { get; set; }
        public Task<bool> ApplicationExistsAsync(string appl_EnfSrv_Cd, string appl_CtrlCd)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CreateApplicationAsync(ApplicationData application)
        {
            throw new NotImplementedException();
        }

        public Task<string> GenerateApplicationControlCodeAsync(string appl_EnfSrv_Cd)
        {
            throw new NotImplementedException();
        }

        public Task<ApplicationData> GetApplicationAsync(string appl_EnfSrv_Cd, string appl_CtrlCd)
        {
            throw new NotImplementedException();
        }

        public Task<List<ApplicationModificationActivitySummaryData>> GetApplicationRecentActivityForSubmitter(string submCd, int days = 0)
        {
            throw new NotImplementedException();
        }

        public Task<List<ApplicationData>> GetApplicationsForAutomationAsync(string appl_EnfSrv_Cd, string medium_Cd, ApplicationState appLiSt_Cd, string appCtgy_Cd, string actvSt_Cd)
        {
            throw new NotImplementedException();
        }

        public Task<bool> GetApplLocalConfirmedSINExistsAsync(string enteredSIN, string debtorSurname, DateTime? debtorBirthDate, string submCd, string ctrlCd, string debtorFirstName = "")
        {
            throw new NotImplementedException();
        }

        public Task<List<ConfirmedSinData>> GetConfirmedSinByDebtorId(string debtorId, bool isActiveOnly)
        {
            throw new NotImplementedException();
        }

        public Task<List<ApplicationConfirmedSINData>> GetConfirmedSINOtherEnforcementOfficeExistsAsync(string appl_EnfSrv_Cd, string subm_SubmCd, string appl_CtrlCd, string appl_Dbtr_Cnfrmd_SIN)
        {
            throw new NotImplementedException();
        }

        public Task<(string errorSameEnfOFf, string errorDiffEnfOff)> GetConfirmedSINRecordsAsync(string subm_SubmCd, string appl_CtrlCd, string appl_Dbtr_Cnfrmd_SIN)
        {
            throw new NotImplementedException();
        }

        public Task<bool> GetConfirmedSINSameEnforcementOfficeExistsAsync(string appl_EnfSrv_Cd, string subm_SubmCd, string appl_CtrlCd, string appl_Dbtr_Cnfrmd_SIN, string categoryCode)
        {
            throw new NotImplementedException();
        }

        public Task<List<ApplicationData>> GetDailyApplCountBySINAsync(string appl_Dbtr_Entrd_SIN, string appl_EnfSrv_Cd, string appl_CtrlCd, string appCtgy_Cd, string appl_Source_RfrNr)
        {
            throw new NotImplementedException();
        }

        public Task<DataList<ApplicationData>> GetRequestedSINApplDataForFileAsync(string enfSrv_Cd, string fileName)
        {
            throw new NotImplementedException();
        }

        public Task<List<ApplicationData>> GetSameCreditorForAppCtgyAsync(string appl_CtrlCd, string subm_SubmCd, string appl_Dbtr_Entrd_SIN, byte appl_SIN_Cnfrmd_Ind, string actvSt_Cd, string appCtgy_Cd)
        {
            throw new NotImplementedException();
        }

        public Task<List<StatsOutgoingProvincialData>> GetStatsProvincialOutgoingDataAsync(int maxRecords, string activeState, string recipientCode, bool isXML = true)
        {
            throw new NotImplementedException();
        }

        public Task UpdateApplicationAsync(ApplicationData application)
        {
            throw new NotImplementedException();
        }

        public Task UpdateSubmitterDefaultControlCodeAsync(string subm_SubmCd, string appl_CtrlCd)
        {
            throw new NotImplementedException();
        }
    }
}
