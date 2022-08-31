﻿using DBHelper;
using FOAEA3.Data.Base;
using FOAEA3.Model.Interfaces;
using FOAEA3.Model;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace FOAEA3.Data.DB
{
    public class DBProvince : DBbase, IProvinceRepository
    {
        public DBProvince(IDBToolsAsync mainDB) : base(mainDB)
        {

        }
        public async Task<List<ProvinceData>> GetProvincesAsync()
        {
            var data = await MainDB.GetAllDataAsync<ProvinceData>("Prv", FillProvinceDataFromReader);

            if (!string.IsNullOrEmpty(MainDB.LastError))
                throw new Exception(MainDB.LastError);

            return data;
        }

        private void FillProvinceDataFromReader(IDBHelperReader rdr, ProvinceData data)
        {
            data.PrvCd = rdr["Prv_Cd"] as string;
            data.PrvTxtE = rdr["Prv_Txt_E"] as string; // can be null
            data.PrvTxtF = rdr["Prv_Txt_F"] as string; // can be null
            data.PrvCtryCd = rdr["Prv_Ctry_Cd"] as string; // can be null
        }
    }

}
