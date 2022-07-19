using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;

namespace izolabella.LoFi.App.Platforms.Android.Notifications
{
    public class IzolabellaNotification : Notification.Builder
    {
        public IzolabellaNotification(Context Context, IzolabellaNotificationChannel Channel) : base(Context, channelId: Channel.Id)
        {
        }

        public static Notification.Builder Factory(Context Context, IzolabellaNotificationChannel Channel)
        {
            return new IzolabellaNotification(Context, Channel)
                    .SetContentTitle("LoFi . .")
                    .SetChannelId(Channel.Id);
        }
    }
}
