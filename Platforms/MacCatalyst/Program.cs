using ObjCRuntime;
using UIKit;

namespace izolabella.LoFi.App.Platforms.MacCatalyst
{
    public class Program
    {
        // This is the main entry point of the application.
        static void Main(string[] Args)
        {
            // if you want to use a different Application Delegate class from "AppDelegate"
            // you can specify it here.
            UIApplication.Main(Args, null, typeof(AppDelegate));
        }
    }
}