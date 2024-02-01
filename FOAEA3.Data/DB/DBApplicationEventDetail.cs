﻿using DBHelper;
using FOAEA3.Data.Base;
using FOAEA3.Model;
using FOAEA3.Model.Enums;
using FOAEA3.Model.Interfaces.Repository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FOAEA3.Data.DB
{

    internal class DBApplicationEventDetail : DBbase, IApplicationEventDetailRepository
    {
        public DBApplicationEventDetail(IDBToolsAsync mainDB) : base(mainDB)
        {

        }

        public async Task<ApplicationEventDetailsList> GetApplicationEventDetails(string appl_EnfSrv_Cd, string appl_CtrlCd, EventQueue queue, string activeState = null)
        {
            var parameters = new Dictionary<string, object>
                {
                    {"Appl_EnfSrv_Cd", appl_EnfSrv_Cd},
                    {"Appl_CtrlCd", appl_CtrlCd }
                };

            ApplicationEventDetailsList data;

            switch (queue)
            {
                case EventQueue.EventSYS_dtl:
                case EventQueue.EventCR_PADR_dtl:
                case EventQueue.EventTrace_ESDC_dtl:
                    data = []; // not implemented yet
                    break;

                case EventQueue.EventLicence_dtl:
                    data = await GetEventDetailsForQueue("EvntLicence_dtl_SelectForApplication", queue, activeState, parameters);
                    break;

                case EventQueue.EventBFN_dtl:
                    if (string.IsNullOrEmpty(activeState))
                        parameters.Add("ActvSt_Cd", "A");
                    else
                        parameters.Add("ActvSt_Cd", activeState);
                    data = await GetEventDetailsForQueue("GetEventBFNDTLforI01", queue, activeState, parameters);
                    break;

                case EventQueue.EventSIN_dtl:
                    data = await GetEventDetailsForQueue("EvntSIN_dtl_SelectForApplication", queue, activeState, parameters);
                    break;

                case EventQueue.EventTrace_dtl:
                    data = await GetEventDetailsForQueue("EvntTrace_dtl_SelectForApplication", queue, activeState, parameters);
                    break;

                default:
                    data = []; // unknown queue?
                    break;
            }

            return data;
        }

        private async Task<ApplicationEventDetailsList> GetEventDetailsForQueue(string procName, EventQueue thisQueue, string activeState,
                                                            Dictionary<string, object> parameters)
        {
            var fixedData = new ApplicationEventDetailsList();

            var data = await MainDB.GetDataFromStoredProcAsync<ApplicationEventDetailData>(procName,
                                                                parameters, FillEventDetailDataFromReader);
            foreach (var item in data)
            {
                item.Queue = thisQueue;
                if (string.IsNullOrEmpty(activeState) ||
                    (!string.IsNullOrEmpty(activeState) && item.ActvSt_Cd == activeState))
                    fixedData.Add(item);
            }

            return fixedData;
        }

        public async Task<ApplicationEventDetailData> GetEventSINDetailDataForEventID(int eventID)
        {
            var parameters = new Dictionary<string, object>
                {
                    {"Event_ID", eventID}
                };

            var data = await MainDB.GetDataFromStoredProcAsync<ApplicationEventDetailData>("EventSIN_dtl_SelectForEventSIN",
                                                                                parameters, FillEventDetailDataFromReader);

            foreach (var item in data)
                item.Queue = EventQueue.EventSIN_dtl;

            return data.FirstOrDefault(); // returns null if no data found
        }

        public async Task<ApplicationEventDetailsList> GetActiveTracingEventDetails(string enfSrv_Cd, string cycle)
        {
            var parameters = new Dictionary<string, object>
            {
                { "EnfSrv_Cd", enfSrv_Cd },
                { "cycle", cycle }
            };

            var result = await MainDB.GetDataFromStoredProcAsync<ApplicationEventDetailData>("MessageBrokerRequestedTRCINEventDetailData", parameters,
                                                                                  FillEventDetailDataFromReader);
            var eventDetailsData = new ApplicationEventDetailsList();

            foreach (var item in result)
            {
                item.Queue = EventQueue.EventTrace_dtl;
                eventDetailsData.Add(item);
            }

            return eventDetailsData;
        }

        public async Task<ApplicationEventDetailsList> GetRequestedLICINLicenceDenialEventDetails(string enfSrv_Cd, string appl_EnfSrv_Cd,
                                                                               string appl_CtrlCd)
        {
            var parameters = new Dictionary<string, object>
            {
                {"EnfSrv_Cd", enfSrv_Cd},
                {"Appl_EnfSrv_Cd", appl_EnfSrv_Cd},
                {"Appl_CtrlCd", appl_CtrlCd}
            };

            var result = await MainDB.GetDataFromStoredProcAsync<ApplicationEventDetailData>("MessageBrokerRequestedLICINEventDetailData", parameters,
                                                                                  FillEventDetailDataFromReader);
            var eventDetailsData = new ApplicationEventDetailsList();
            foreach (var item in result)
            {
                item.Queue = EventQueue.EventLicence_dtl;
                eventDetailsData.Add(item);
            }

            return eventDetailsData;
        }

        public async Task<ApplicationEventDetailsList> GetRequestedSINEventDetailDataForFile(string enfSrv_Cd, string fileName)
        {
            var parameters = new Dictionary<string, object>
            {
                { "EnfSrv_Cd", enfSrv_Cd },
                { "fileName", fileName }
            };

            var resultData = await MainDB.GetDataFromStoredProcAsync<ApplicationEventDetailData>("MessageBrokerRequestedSININEventDetailData", parameters,
                                                                                      FillEventDetailDataFromReader);
            var eventDetailsData = new ApplicationEventDetailsList();

            foreach (var item in resultData)
            {
                item.Queue = EventQueue.EventSIN_dtl;
                eventDetailsData.Add(item);
            }

            return eventDetailsData;
        }

        public async Task<bool> SaveEventDetail(ApplicationEventDetailData eventDetailData)
        {
            bool success = false;

            switch (eventDetailData.Queue)
            {
                case EventQueue.EventSYS_dtl:
                case EventQueue.EventCR_PADR_dtl:
                case EventQueue.EventTrace_ESDC_dtl:
                    // TODO: other event details types
                    break;

                case EventQueue.EventBFN_dtl:
                    if (eventDetailData.Event_dtl_Id == 0)
                        success = await CreateEventDetail(eventDetailData, "EvntBFN");
                    else
                        success = await UpdateBFNEventDetai(eventDetailData);
                    break;

                case EventQueue.EventTrace_dtl:
                    if (eventDetailData.Event_dtl_Id == 0)
                        success = await CreateEventDetail(eventDetailData, "EvntTrace");
                    else
                        success = await UpdateEventDetail(eventDetailData, "MessageBrokerEventTraceDetailUpdate");
                    break;

                case EventQueue.EventLicence_dtl:
                    if (eventDetailData.Event_dtl_Id == 0)
                        success = await CreateEventDetail(eventDetailData, "EvntLicence");
                    else
                        success = await UpdateEventDetail(eventDetailData, "MessageBrokerEventLicenseDetailUpdate");
                    break;

                case EventQueue.EventSIN_dtl:
                    if (eventDetailData.Event_dtl_Id == 0)
                        success = await CreateEventDetail(eventDetailData, "EvntSIN");
                    else
                        success = await UpdateEventDetail(eventDetailData, "MessageBrokerEventSINDetailUpdate");
                    break;

            }

            return success;
        }

        public async Task<bool> SaveEventDetails(ApplicationEventDetailsList eventDetailsData)
        {
            bool success = true;

            foreach (var eventDetail in eventDetailsData)
                if (!(await SaveEventDetail(eventDetail)))
                    success = false;

            return success;
        }

        private async Task<bool> CreateEventDetail(ApplicationEventDetailData eventDetailData, string tableName)
        {
            bool success = false;

            var parameters = new Dictionary<string, object>
                    {
                        { "rvchTableName", tableName},
                        { "rintEvent_id", eventDetailData.Event_Id},
                        { "rdtmTimeStamp", eventDetailData.Event_TimeStamp},
                        { "dchrPriority_Ind", eventDetailData.Event_Priority_Ind},
                        { "ddtmEffctv_Dte", eventDetailData.Event_Effctv_Dte},
                        { "dchrActvSt_Cd", eventDetailData.ActvSt_Cd},
                        { "dsntAppList_Cd", eventDetailData.AppLiSt_Cd}
                    };

            if (eventDetailData.Event_Compl_Dte.HasValue)
                parameters.Add("ddtmCompl_Dte", eventDetailData.Event_Compl_Dte.Value);

            if (eventDetailData.Event_Reas_Cd.HasValue)
                parameters.Add("dintReas_Cd", (int)eventDetailData.Event_Reas_Cd.Value);

            if (!string.IsNullOrWhiteSpace(eventDetailData.Event_Reas_Text))
                parameters.Add("dvchReas_Text", eventDetailData.Event_Reas_Text);

            await MainDB.ExecProcAsync("fp_sub_EvntdtlCreate", parameters);

            if (string.IsNullOrEmpty(MainDB.LastError))
                success = true;

            return success;
        }

        private async Task<bool> UpdateBFNEventDetai(ApplicationEventDetailData eventDetailData)
        {
            bool success = false;

            var parameters = new Dictionary<string, object>
                    {
                        { "rintEvent_dtl_Id", eventDetailData.Event_dtl_Id},
                        { "dintReas_Cd", eventDetailData.Event_Reas_Cd},
                        { "dvchReas_Text", eventDetailData.Event_Reas_Text},
                        { "ddtmEvent_Effctv_Dte", eventDetailData.Event_Effctv_Dte},
                        { "dsmtAppLiSt_Cd", eventDetailData.ActvSt_Cd},
                        { "dchrActvSt_Cd", eventDetailData.AppLiSt_Cd}
                    };

            await MainDB.ExecProcAsync("fp_PrcsSub_EvntUpdate", parameters);

            if (string.IsNullOrEmpty(MainDB.LastError))
                success = true;

            return success;
        }

        private async Task<bool> UpdateEventDetail(ApplicationEventDetailData eventDetailData, string procName)
        {
            bool success = false;

            var parameters = new Dictionary<string, object>
            {
                { "Event_dtl_Id", eventDetailData.Event_dtl_Id },
                { "EnfSrv_Cd", eventDetailData.EnfSrv_Cd },
                { "Event_Id", eventDetailData.Event_Id },
                { "Event_TimeStamp", eventDetailData.Event_TimeStamp },
                { "Event_Priority_Ind", eventDetailData.Event_Priority_Ind },
                { "Event_Effctv_Dte", eventDetailData.Event_Effctv_Dte },
                { "ActvSt_Cd", eventDetailData.ActvSt_Cd },
                { "AppLiSt_Cd", (int) eventDetailData.AppLiSt_Cd }
            };

            if (eventDetailData.Event_Compl_Dte.HasValue)
                parameters.Add("Event_Compl_Dte", eventDetailData.Event_Compl_Dte.Value);

            if (eventDetailData.Event_Reas_Cd.HasValue)
                parameters.Add("Event_Reas_Cd", (int)eventDetailData.Event_Reas_Cd.Value);
            else
                parameters.Add("Event_Reas_Cd", DBNull.Value);

            if (!string.IsNullOrWhiteSpace(eventDetailData.Event_Reas_Text))
                parameters.Add("Event_Reas_Text", eventDetailData.Event_Reas_Text);
            else
                parameters.Add("Event_Reas_Text", DBNull.Value);

            await MainDB.ExecProcAsync(procName, parameters);

            if (string.IsNullOrEmpty(MainDB.LastError))
                success = true;

            return success;
        }

        public async Task UpdateOutboundEventDetail(string activeState, string applicationState, string enfSrvCode,
                                              string writtenFile, List<int> eventIds)
        {
            string xmlEventList = ConvertEventIdsListToXml(eventIds);

            var parameters = new Dictionary<string, object>
            {
                {"actStCode", activeState },
                {"appLiStCode", applicationState },
                {"enfSvrCode", enfSrvCode },
                {"sWrittenFile", writtenFile },
                {"Event_dtl_Ids", xmlEventList }
            };

            await MainDB.ExecProcAsync("MessageBrokerOutboundUpdateEventDetail", parameters);

        }

        private static string ConvertEventIdsListToXml(List<int> eventIds)
        {
            var x = new XElement("NewDataSet", eventIds.Select(eventId => new XElement("FTPEvents",
                                                                                       new XElement("Event_dtl_Id", eventId))));

            return x.ToString();
        }

        private static void FillEventDetailDataFromReader(IDBHelperReader rdr, ApplicationEventDetailData data)
        {
            data.Event_dtl_Id = (int)rdr["Event_dtl_Id"];
            data.EnfSrv_Cd = rdr["EnfSrv_Cd"] as string;
            data.Event_Id = rdr["Event_Id"] as int?; // can be null 
            data.Event_TimeStamp = (DateTime)rdr["Event_TimeStamp"];
            data.Event_Compl_Dte = rdr["Event_Compl_Dte"] as DateTime?; // can be null 
            if (rdr["Event_Reas_Cd"] != null)  // can be null
            {
                int reasonCode = (int)rdr["Event_Reas_Cd"];
                data.Event_Reas_Cd = (EventCode)reasonCode;
            }
            data.Event_Reas_Text = rdr["Event_Reas_Text"] as string; // can be null 
            data.Event_Priority_Ind = rdr["Event_Priority_Ind"] as string; // can be null 
            data.Event_Effctv_Dte = rdr["Event_Effctv_Dte"] as DateTime?; // can be null 
            data.AppLiSt_Cd = (short)rdr["AppLiSt_Cd"];
            data.ActvSt_Cd = rdr["ActvSt_Cd"] as string;
        }
    }
}
