namespace TestCustom
{
	public partial class ViewCoverFlow : MonoTouch.UIKit.UIView
	{
		public static ViewCoverFlow FromXib(string nibName, MonoTouch.Foundation.NSObject owner)
		{
			return (ViewCoverFlow)MonoLib.UI.XibLoader.LoadNib(nibName, owner);
		}

		public ViewCoverFlow(System.IntPtr handle)
			: base(handle)
		{
		}
		
		public OpenFlowSharp.OpenFlowView FlowView
		{
			get { return flowView; }
		}
	}
	
	public partial class MyFlowView : OpenFlowSharp.OpenFlowView
	{
		[MonoTouch.Foundation.ExportAttribute("initWithCoder:")]
		public MyFlowView(MonoTouch.Foundation.NSCoder coder)
			: base(coder)
		{
		}
	}
	
}

