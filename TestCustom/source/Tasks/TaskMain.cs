namespace TestCustom
{
	public class TaskMain : MonoLib.Core.Task, OpenFlowSharp.IOpenFlowDataSource
	{
		public override void Open()
		{
			InitializeImageList();

			_view = new OpenFlowSharp.OpenFlowView()
			{
				Frame = AppDelegate.Instance.Window.Frame,
				Bounds = AppDelegate.Instance.Window.Bounds,
				BackgroundColor = MonoTouch.UIKit.UIColor.Blue,
				DataSource = this
			};
			_view.ItemChanged += OnItemChanged;
			_view.ItemPicked += OnItemPicked;
			//_view.Initialize();
			//_view.NumberOfImages = NumberOfImages(_view);
			AppDelegate.Instance.Window.AddSubview(_view);
			IsOpened = true;
		}

		public override void Close()
		{
			_view.ItemChanged -= OnItemChanged;
			_view.ItemPicked -= OnItemPicked;
			_view.RemoveFromSuperview();
			IsClosed = true;
		}

		void OnItemChanged(OpenFlowSharp.OpenFlowView sender, int index)
		{
		}
		void OnItemPicked(OpenFlowSharp.OpenFlowView sender, int index)
		{
		}

		#region IOpenFlowDataSource メンバ

		public int NumberOfImages(OpenFlowSharp.OpenFlowView openFlowView)
		{
			System.Diagnostics.Debug.WriteLine("+ [NumberOfImages]");
			return _imageList.Count;
		}

		public MonoTouch.UIKit.UIImage RequestImage(OpenFlowSharp.OpenFlowView openFlowView, int index)
		{
			System.Diagnostics.Debug.WriteLine("+ [RequestImage] Index:" + index);
			if (0 == index % 2)
			{
				return null;
			}
			return _imageList[index];
		}

		public MonoTouch.UIKit.UIImage RequestDefaultImage(OpenFlowSharp.OpenFlowView openFlowView)
		{
			System.Diagnostics.Debug.WriteLine("+ [RequestDefaultImage]");
			return _defaultImage;
		}

		#endregion

		/// <summary>
		/// 表示用画像を用意する。
		/// </summary>
		private void InitializeImageList()
		{
			_defaultImage = MonoTouch.UIKit.UIImage.FromFile("default.png");
			_imageList = new System.Collections.Generic.List<MonoTouch.UIKit.UIImage>(30);
			int i = 0;
			while (_imageList.Count < _imageList.Capacity)
			{
				MonoTouch.UIKit.UIImage img = MonoTouch.UIKit.UIImage.FromFile("images/" + i + ".jpg");
				_imageList.Add(img);
				++i;
			}
		}

		private OpenFlowSharp.OpenFlowView _view;
		private MonoTouch.UIKit.UIImage _defaultImage;
		private System.Collections.Generic.List<MonoTouch.UIKit.UIImage> _imageList;
	}
}

