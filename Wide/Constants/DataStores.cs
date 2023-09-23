using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using izolabella.Storage.Objects.DataStores;

namespace izolabella.LoFi.Wide.Constants
{
    public static class DataStores
    {
        public static string AppName => "izolabella.LApp";

        public static MauiDataStore CredStore => new(AppName, "C");
    }
}