// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      Mono Runtime Version: 2.0.50727.1433
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------

namespace TestCustom {
	
	
	// Base type probably should be MonoTouch.UIKit.UIView or subclass
	[MonoTouch.Foundation.Register("ViewCoverFlow")]
	public partial class ViewCoverFlow {
		
		private MyFlowView __mt_flowView;
		
		#pragma warning disable 0169
		[MonoTouch.Foundation.Connect("flowView")]
		private MyFlowView flowView {
			get {
				this.__mt_flowView = ((MyFlowView)(this.GetNativeField("flowView")));
				return this.__mt_flowView;
			}
			set {
				this.__mt_flowView = value;
				this.SetNativeField("flowView", value);
			}
		}
	}
	
	// Base type probably should be MonoTouch.UIKit.UIView or subclass
	[MonoTouch.Foundation.Register("MyFlowView")]
	public partial class MyFlowView {
	}
}