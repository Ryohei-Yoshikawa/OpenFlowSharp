namespace TestCustom
{
	public class TaskXibTest : MonoLib.Core.Task, OpenFlowSharp.IOpenFlowDataSource
	{
		public override void Open()
		{
			_view = ViewCoverFlow.FromXib("ViewCoverFlow", AppDelegate.Instance);
			_view.FlowView.ItemChanged += OnItemChanged;
			_view.FlowView.ItemPicked += OnItemPicked;
			_view.FlowView.DataSource = this;
			//_view.FlowView.Initialize();
			//_view.FlowView.NumberOfImages = NumberOfImages(_view.FlowView);
			AppDelegate.Instance.Window.AddSubview(_view);

			IsOpened = true;
		}

		public override void Close()
		{
			IsClosed = true;

			_view.FlowView.ItemChanged -= OnItemChanged;
			_view.FlowView.ItemPicked -= OnItemPicked;
			_view.RemoveFromSuperview();
		}

		void OnItemChanged(OpenFlowSharp.OpenFlowView sender, int index)
		{
			System.Diagnostics.Debug.WriteLine("+ [OnItemChanged] Index:" + index);
		}
		void OnItemPicked(OpenFlowSharp.OpenFlowView sender, int index)
		{
			System.Diagnostics.Debug.WriteLine("+ [OnItemPicked] Index:" + index);
		}

		#region IOpenFlowDataSource メンバ

		public int NumberOfImages(OpenFlowSharp.OpenFlowView openFlowView)
		{
			return 30;
		}

		public MonoTouch.UIKit.UIImage RequestImage(OpenFlowSharp.OpenFlowView openFlowView, int index)
		{
			return MonoTouch.UIKit.UIImage.FromFile("images/" + index + ".jpg");
		}

		public MonoTouch.UIKit.UIImage RequestDefaultImage(OpenFlowSharp.OpenFlowView openFlowView)
		{
			return MonoTouch.UIKit.UIImage.FromFile("default.png");
		}

		#endregion

		private ViewCoverFlow _view;
	}
}
