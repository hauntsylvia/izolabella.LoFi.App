using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using izolabella.Util;

namespace izolabella.LoFi.App.Platforms.Android.Notifications
{
    public abstract class IzolabellaNotificationChannel
    {
        public IzolabellaNotificationChannel(string Name, string Description, string? Id = null)
        {
            this.Name = Name;
            this.Description = Description;
            this.Id =  Id ?? IdGenerator.CreateNewId().ToString(CultureInfo.InvariantCulture);
        }

        public string Name { get; }

        public string Description { get; }

        public string Id { get; }

        public NotificationChannel Build()
        {
            return new NotificationChannel(this.Id, this.Name, NotificationImportance.Max);
        }
    }
}
