using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace izolabella.LoFi.Platforms.Android.Notifications;

public class IzolabellaMusicChannel : IzolabellaNotificationChannel
{
    public IzolabellaMusicChannel() : base("LoFi", "The running notifications.", "Music")
    {
    }
}
