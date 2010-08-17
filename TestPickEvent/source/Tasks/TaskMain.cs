namespace TestPickEvent
{
	public class TaskMain : MonoLib.Core.Task, OpenFlowSharp.IOpenFlowDataSource
	{
		private const int NumberOfImages = 30;

		public TaskMain ()
		{
		}
		
		public override void Open ()
		{
			_view = new OpenFlowSharp.OpenFlowView(AppDelegate.Instance.Window.Bounds, this);
			LoadAllImages();
			_view.ItemChanged += OnItemChanged;
			_view.ItemPicked += OnItemPicked;

			AppDelegate.Instance.Window.AddSubview(_view);
			IsOpened = true;
		}
		
		public override void Close ()
		{
			IsClosed = true;
			_view.ItemChanged -= OnItemChanged;
			_view.ItemPicked -= OnItemPicked;
			_view.RemoveFromSuperview();
		}

		/// <summary>
		/// imagesフォルダ内の全ての画像を読み込んでFlowViewに設定する。
		/// FlowView作成済みで有ること。
		/// </summary>
		private void LoadAllImages()
		{
			for (int i = 0; i < NumberOfImages; ++i)
			{
				MonoTouch.UIKit.UIImage img = MonoTouch.UIKit.UIImage.FromFile("images/" + i + ".jpg");
				_view[i] = img;
			}
			_view.NumberOfImages = NumberOfImages;
		}

		private void OnItemChanged(OpenFlowSharp.OpenFlowView sender, int index)
		{
			System.Diagnostics.Debug.WriteLine("+ [TaskMain.OnItemChanged] Index:" + index);
		}
		private void OnItemPicked(OpenFlowSharp.OpenFlowView sender, int index)
		{
			System.Diagnostics.Debug.WriteLine("+ [TaskMain.OnItemPicked] Index:" + index);
		}

		#region IOpenFlowDataSource メンバ

		void OpenFlowSharp.IOpenFlowDataSource.RequestImage(OpenFlowSharp.OpenFlowView view, int index)
		{
			System.Diagnostics.Debug.WriteLine("+ [TaskMain.RequestImage] Index:" + index);

			MonoTouch.UIKit.UIImage img = MonoTouch.UIKit.UIImage.FromFile("images/" + index + ".jpg");
			view[index] = img;
		}

		MonoTouch.UIKit.UIImage OpenFlowSharp.IOpenFlowDataSource.GetDefaultImage()
		{
			System.Diagnostics.Debug.WriteLine("+ [TaskMain.GetDefaultImage]");

			return MonoTouch.UIKit.UIImage.FromFile("default.png");
		}

		#endregion

		private OpenFlowSharp.OpenFlowView _view;

	}
}

