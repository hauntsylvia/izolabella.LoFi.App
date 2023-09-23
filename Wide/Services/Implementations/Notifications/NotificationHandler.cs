using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if ANDROID
using Android.Content;
using izolabella.LoFi.Platforms.Android;
using izolabella.LoFi.Platforms.Android.Notifications;
using Android.App;
#endif

namespace izolabella.LoFi.Wide.Services.Implementations.Notifications
{
    public class NotificationHandler
    {
        public NotificationHandler()
        {

        }

#if ANDROID
        public static void SendNotification(string Text, IzolabellaNotificationChannel Channel, NotificationManager Manager, Context Context)
        {
            Intent OnClickIntent = new(Context, typeof(MainActivity));
            TaskStackBuilder? StackBuilder = TaskStackBuilder.Create(Context);
            StackBuilder?.AddParentStack(Java.Lang.Class.FromType(typeof(MainActivity)));
            StackBuilder?.AddNextIntent(OnClickIntent);

            PendingIntent? OnClickPendingIntent = StackBuilder?.GetPendingIntent(0, PendingIntentFlags.Mutable);

            Manager.Notify(0, IzolabellaNotification.Factory(Context, Channel)
                .SetContentIntent(OnClickPendingIntent)
                .SetContentText(Text)
                .SetVisibility(NotificationVisibility.Public)
                .SetCategory("LoFi").Build());
        }
#endif
    }
}