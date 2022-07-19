using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;

namespace izolabella.LoFi.App.Platforms.Android.Services.Interfaces
{
    public interface IService
    {
        public void StartForegroundServiceCompat();
    }
}
