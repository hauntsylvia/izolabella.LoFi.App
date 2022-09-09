using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;

namespace izolabella.LoFi.Platforms.Android.Notifications;

public class IzolabellaNotification
{
    public IzolabellaNotification(Context Context, IzolabellaNotificationChannel Channel)
    {
        this.Inner = OperatingSystem.IsAndroidVersionAtLeast(26) ? (new(Context, channelId: Channel.Id)) : throw new PlatformNotSupportedException();
    }

    private Notification.Builder Inner { get; }

    public static Notification.Builder Factory(Context Context, IzolabellaNotificationChannel Channel)
    {
        return OperatingSystem.IsAndroidVersionAtLeast(26) ? new IzolabellaNotification(Context, Channel).Inner
                .SetContentTitle("LoFi . .")
                .SetChannelId(Channel.Id)
                .SetSmallIcon(Resource.Drawable.navigation_empty_icon) : throw new PlatformNotSupportedException();
    }
}
