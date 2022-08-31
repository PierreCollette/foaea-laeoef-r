﻿using FOAEA3.Model;
using FOAEA3.Model.Interfaces;
using System.Threading.Tasks;

namespace FOAEA3.Business.Utilities
{
    internal class EnforcementServiceManager
    {
        private IRepositories DB { get; }

        public EnforcementServiceManager(IRepositories repositories)
        {
            DB = repositories;
        }

        public async Task<EnfSrvData> GetEnforcementServiceAsync(string enfSrvCd)
        {
            IEnfSrvRepository db = DB.EnfSrvTable;
            var result = await db.GetEnfServiceAsync(enfSrvCd);
            if (result.Count == 1)
                return result[0];
            else
                return null;
        }

    }
}
