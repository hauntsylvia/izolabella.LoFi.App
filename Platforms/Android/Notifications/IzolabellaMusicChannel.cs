using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace izolabella.LoFi.App.Platforms.Android.Notifications
{
    public class IzolabellaMusicChannel : IzolabellaNotificationChannel
    {
        public IzolabellaMusicChannel(string Name, string Description) : base(Name, Description, "Music")
        {
        }
    }
}
