
using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace TestCustom
{
	public class Application
	{
		static void Main (string[] args)
		{
			UIApplication.Main (args);
		}
	}

	// The name AppDelegate is referenced in the MainWindow.xib file.
	public partial class AppDelegate : MonoLib.Applet.MonoLibAppDelegate
	{
		// This method is invoked when the application has loaded its UI and its ready to run
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			// If you have defined a view, add it here:
			// window.AddSubview (navigationController.View);

			FramePerSeconds = 30;
			StartMainLoop();

			//MonoLib.Core.TaskManager.Instance.InitTask(new TaskMain());
			MonoLib.Core.TaskManager.Instance.InitTask(new TaskXibTest());
			
			window.MakeKeyAndVisible ();
			
			return true;
		}

		public override void Run()
		{
			MonoLib.Core.TaskManager.Instance.Proc();
			MonoLib.Core.TaskManager.Instance.Draw();
		}

		public static AppDelegate Instance
		{
			get { return (AppDelegate)UIApplication.SharedApplication.Delegate; }
		}

		public UIWindow Window
		{
			get { return window; }
		}
	}
}

