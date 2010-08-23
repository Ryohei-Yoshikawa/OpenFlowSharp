// Copyright (c) 2009 Alex Fajkowski, Apparent Logic LLC
// C# port Copyright (C) 2010 Miguel de Icaza.
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
//
namespace OpenFlowSharp
{
	/// <summary>
	/// カバーフロー用データソースクラス。
	/// </summary>
	/// <remarks>
	/// </remarks>
	public interface IOpenFlowDataSource
	{
		/// <summary>
		/// カバーフローが保持する画像の枚数を返す。
		/// </summary>
		/// <param name="openFlowView"></param>
		/// <returns></returns>
		int NumberOfImages(OpenFlowView openFlowView);

		/// <summary>
		/// index番目に表示する画像を返す。
		/// </summary>
		/// <param name="view"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		MonoTouch.UIKit.UIImage RequestImage(OpenFlowView openFlowView, int index);

		/// <summary>
		/// デフォルトで表示する画像を返す。
		/// </summary>
		/// <param name="openFlowView"></param>
		/// <returns></returns>
		MonoTouch.UIKit.UIImage RequestDefaultImage(OpenFlowView openFlowView);
	}

	/// <summary>
	/// カバーフロークラス。
	/// </summary>
	[MonoTouch.Foundation.Register("OpenFlowView")]
	public partial class OpenFlowView : MonoTouch.UIKit.UIView
	{
		private const float kReflectionFraction = 0.85f;
		private const int kCoverBuffer = 6;

		/// <summary>
		/// コンストラクタ。
		/// </summary>
		public OpenFlowView()
			: base()
		{
			Initialize();
		}
		/// <summary>
		/// コンストラクタ。
		/// ビュー単体をNSBundle.MainBundle.LoadNib()によって読み込んだとき用。
		/// </summary>
		/// <param name="handle"></param>
		public OpenFlowView(System.IntPtr handle)
			: base(handle)
		{
			Initialize();
		}
		/// <summary>
		/// コンストラクタ。
		/// UIViewController等のアウトレットとして暗黙にコールされる。
		/// </summary>
		/// <param name="coder"></param>
		[MonoTouch.Foundation.ExportAttribute("initWithCoder:")]
		public OpenFlowView(MonoTouch.Foundation.NSCoder coder)
			: base(coder)
		{
			Initialize();
		}
		/// <summary>
		/// コンストラクタ。
		/// </summary>
		/// <param name="frame"></param>
		public OpenFlowView(System.Drawing.RectangleF frame)
			: base(frame)
		{
			Initialize();
		}

		/// <summary>
		/// カバーフローを初期化する。
		/// </summary>
		private void Initialize()
		{
			_coverImages = new System.Collections.Generic.Dictionary<int, CoverImage>();
			_onscreenCovers = new System.Collections.Generic.Dictionary<int, ItemView>();
			_offscreenCovers = new System.Collections.Generic.Stack<ItemView>();
#if false
			// デフォルト画像の作成
			_defaultImage = new CoverImage(_dataSource.RequestDefaultImage(this));
#endif
			// スクロールビューの作成
			_scrollView = new MonoTouch.UIKit.UIScrollView(Frame)
			{
				UserInteractionEnabled = false,
				MultipleTouchEnabled = false,
				AutoresizingMask = MonoTouch.UIKit.UIViewAutoresizing.FlexibleHeight | MonoTouch.UIKit.UIViewAutoresizing.FlexibleWidth
			};
			AddSubview(_scrollView);

			MultipleTouchEnabled = false;
			UserInteractionEnabled = true;
			AutosizesSubviews = true;

			//Layer.Position = new System.Drawing.PointF((float)(Frame.Size.Width / 2), (float)(Frame.Height / 2));
			Layer.Position = new System.Drawing.PointF((float)(Frame.Left + Frame.Size.Width / 2), (float)(Frame.Top + Frame.Height / 2));

			// Initialize the visible and selected cover range.
			_lowerVisibleCover = _upperVisibleCover = -1;
			_selectedCoverView = null;

			// Set up transforms
			UpdateTransforms();

			// Set some perspective
			var sublayerTransform = MonoTouch.CoreAnimation.CATransform3D.Identity;
			sublayerTransform.m34 = -0.01f;
			_scrollView.Layer.SublayerTransform = sublayerTransform;

			Bounds = Frame;
		}

		/// <summary>
		/// アイテム変更デリゲート型。
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="index"></param>
		public delegate void ItemChangedDelegate(OpenFlowView sender, int index);

		/// <summary>
		/// アイテム変更デリゲート
		/// </summary>
		public event ItemChangedDelegate ItemChanged
		{
			add
			{
				_changedDelegate += value;
			}
			remove
			{
				_changedDelegate -= value;
			}
		}
		private ItemChangedDelegate _changedDelegate;

		/// <summary>
		/// アイテム選択デリゲート型。
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="index"></param>
		public delegate void ItemPickedDelegate(OpenFlowView sender, int index);

		/// <summary>
		/// アイテム選択デリゲート。
		/// </summary>
		public event ItemPickedDelegate ItemPicked
		{
			add
			{
				_pickedDelegate += value;
			}
			remove
			{
				_pickedDelegate -= value;
			}
		}
		private ItemPickedDelegate _pickedDelegate;

		private MonoTouch.UIKit.UIScrollView _scrollView;
		private ItemView _selectedCoverView;
		private MonoTouch.CoreAnimation.CATransform3D _leftTransform, _rightTransform;

		private int _lowerVisibleCover, _upperVisibleCover, _numberOfImages, _beginningCover;
		private float _halfScreenHeight, _halfScreenWidth;

		private bool _isSingleTap, _isDoubleTap, _isDraggingACover;
		private float _startPosition;

		private CoverImage _defaultImage;												//!< カバー用デフォルト画像
		private System.Collections.Generic.Dictionary<int, CoverImage> _coverImages;	//!< カバー用画像リスト
		private System.Collections.Generic.Dictionary<int, ItemView> _onscreenCovers;	//!< 表示されているカバー
		private System.Collections.Generic.Stack<ItemView> _offscreenCovers;			//!< 表示外のカバー

		#region Properties
		/// <summary>
		/// データソース。
		/// </summary>
		public IOpenFlowDataSource DataSource
		{
			get
			{
				return _dataSource;
			}
			set
			{
				_dataSource = value;
				NumberOfImages = value.NumberOfImages(this);
				Layout();
			}
		}
		private IOpenFlowDataSource _dataSource;

		public int NumberOfImages
		{
			get
			{
				return _numberOfImages;
			}
			set
			{
				_numberOfImages = value;
				_scrollView.ContentSize = new System.Drawing.SizeF((float)(value * coverSpacing + Bounds.Size.Width), Bounds.Size.Height);
				if (_selectedCoverView == null)
				{
					SetSelectedCover(0);
				}
				CallChangedDelegate(0);
				Layout();
			}
		}
		#endregion

		#region Configuration settings
		int coverSpacing = 40;
		int centerCoverOffset = 70;
		float sideCoverAngle = 0.79f;
		int sideCoverPosition = -80;

		public int CoverSpacing
		{
			get
			{
				return coverSpacing;
			}
			set
			{
				coverSpacing = value;
				Layout();
			}
		}

		void Layout()
		{
			if (_selectedCoverView == null)
				return;

			int lowerBound = System.Math.Max(-1, _selectedCoverView.Number - kCoverBuffer);
			int upperBound = System.Math.Min(NumberOfImages - 1, _selectedCoverView.Number + kCoverBuffer);

			LayoutCovers(_selectedCoverView.Number, lowerBound, upperBound);
			CenterOnSelectedCover(false);

		}
		#endregion

#if false
		public override void MovedToWindow()
		{
			SetupInitialState();
			if (null != _dataSource)
			{
				NumberOfImages = _dataSource.NumberOfImages(this);
			}

			int selected = Selected;
			if (0 <= selected)
			{
				CallChangedDelegate(selected);
			}
		}
		void SetupInitialState()
		{
			//if (_dataSource != null)
			//    _defaultImage = _dataSource.GetDefaultImage();

			_scrollView = new MonoTouch.UIKit.UIScrollView(Frame)
			{
				UserInteractionEnabled = false,
				MultipleTouchEnabled = false,
				AutoresizingMask = MonoTouch.UIKit.UIViewAutoresizing.FlexibleHeight | MonoTouch.UIKit.UIViewAutoresizing.FlexibleWidth
			};
			AddSubview(_scrollView);

			MultipleTouchEnabled = false;
			UserInteractionEnabled = true;
			AutosizesSubviews = true;

			Layer.Position = new System.Drawing.PointF((float)(Frame.Size.Width / 2), (float)(Frame.Height / 2));

			// Initialize the visible and selected cover range.
			_lowerVisibleCover = _upperVisibleCover = -1;
			_selectedCoverView = null;

			// Set up transforms
			UpdateTransforms();

			// Set some perspective
			var sublayerTransform = MonoTouch.CoreAnimation.CATransform3D.Identity;
			sublayerTransform.m34 = -0.01f;
			_scrollView.Layer.SublayerTransform = sublayerTransform;

			Bounds = Frame;
		}
#endif

		void UpdateTransforms()
		{
			_leftTransform = MonoTouch.CoreAnimation.CATransform3D.Identity;
			_leftTransform = _leftTransform.Rotate(sideCoverAngle, 0, 1, 0);
			_rightTransform = MonoTouch.CoreAnimation.CATransform3D.Identity;
			_rightTransform = _rightTransform.Rotate(sideCoverAngle, 0, -1, 0);
		}

		ItemView CoverForIndex(int index)
		{
			var coverView = DequeueReusableCover();
			if (coverView == null)
				coverView = new ItemView(this, System.Drawing.RectangleF.Empty);
			coverView.Number = index;

			return coverView;
		}

		/// <summary>
		/// カバー用画像を更新する。
		/// </summary>
		/// <param name="aCover"></param>
		void UpdateCoverImage(ItemView aCover)
		{
			if (_coverImages.ContainsKey(aCover.Number))
			{
				// リスト内から割り当て
				aCover.SetImage(_coverImages[aCover.Number].Image, _coverImages[aCover.Number].Height, kReflectionFraction);
			}
			else
			{
				// データソースに要求
				if (null != _dataSource)
				{
					MonoTouch.UIKit.UIImage image = _dataSource.RequestImage(this, aCover.Number);
					if (null != image)
					{
						_coverImages.Add(aCover.Number, new CoverImage(image));
						aCover.SetImage(_coverImages[aCover.Number].Image, _coverImages[aCover.Number].Height, kReflectionFraction);
					}
					else
					{
						if (null == _defaultImage)
						{
							_defaultImage = new CoverImage(_dataSource.RequestDefaultImage(this));
						}
						aCover.SetImage(_defaultImage.Image, _defaultImage.Height, kReflectionFraction);
					}
				}
			}
		}

		ItemView DequeueReusableCover()
		{
			if (_offscreenCovers.Count > 0)
				return _offscreenCovers.Pop();
			return null;
		}

		void LayoutCover(ItemView aCover, int selectedIndex, bool animated)
		{
			int coverNumber = aCover.Number;
			MonoTouch.CoreAnimation.CATransform3D newTransform;
			float newZPosition = sideCoverPosition;
			System.Drawing.PointF newPosition = new System.Drawing.PointF(_halfScreenWidth + aCover.HorizontalPosition, _halfScreenHeight + aCover.VerticalPosition);

			if (coverNumber < selectedIndex)
			{
				newPosition.X -= centerCoverOffset;
				newTransform = _leftTransform;
			}
			else if (coverNumber > selectedIndex)
			{
				newPosition.X += centerCoverOffset;
				newTransform = _rightTransform;
			}
			else
			{
				newZPosition = 0;
				newTransform = MonoTouch.CoreAnimation.CATransform3D.Identity;
			}

			if (animated)
			{
				BeginAnimations(null);
				SetAnimationCurve(MonoTouch.UIKit.UIViewAnimationCurve.EaseOut);
				SetAnimationBeginsFromCurrentState(true);
			}
			aCover.Layer.Transform = newTransform;
			aCover.Layer.ZPosition = newZPosition;
			aCover.Layer.Position = newPosition;

			if (animated)
				CommitAnimations();
		}

		void LayoutCovers(int selected, int from, int to)
		{
			for (int i = from; i <= to; i++)
			{
				ItemView cover;

				if (_onscreenCovers.TryGetValue(i, out cover))
					LayoutCover(cover, selected, true);
			}
		}

		ItemView FindCoversOnScreen(MonoTouch.CoreAnimation.CALayer targetLayer)
		{
			foreach (ItemView cover in _onscreenCovers.Values)
				if (cover.ImageView.Layer == targetLayer)
					return cover;
			return null;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_defaultImage.Dispose();
				_scrollView.Dispose();
				foreach (var k in _offscreenCovers)
					k.Dispose();
				foreach (var k in _onscreenCovers.Values)
					k.Dispose();
			}

			base.Dispose(disposing);
		}

		public override System.Drawing.RectangleF Bounds
		{
			get
			{
				return base.Bounds;
			}
			set
			{
				base.Bounds = value;
				_halfScreenHeight = value.Size.Height / 2;
				_halfScreenWidth = value.Size.Width / 2;

				Layout();
			}
		}

		public void SetSelectedCover(int newSelectedCover)
		{
			if (_selectedCoverView != null && (newSelectedCover == _selectedCoverView.Number))
				return;

			int newLowerBound = System.Math.Max(0, newSelectedCover - kCoverBuffer);
			int newUpperBound = System.Math.Min(_numberOfImages - 1, newSelectedCover + kCoverBuffer);
			if (_selectedCoverView == null)
			{
				// Allocate and display covers from newLower to newUYpper bounds.
				for (int i = newLowerBound; i <= newUpperBound; i++)
				{
					var cover = CoverForIndex(i);
					_onscreenCovers[i] = cover;
					UpdateCoverImage(cover);
					_scrollView.Layer.AddSublayer(cover.Layer);
					LayoutCover(cover, newSelectedCover, false);
				}

				_lowerVisibleCover = newLowerBound;
				_upperVisibleCover = newUpperBound;
				_selectedCoverView = _onscreenCovers[newSelectedCover];
				return;
			}

			// Check to see if the new and current ranges overlap
			if ((newLowerBound > _upperVisibleCover) || (newUpperBound < _lowerVisibleCover))
			{
				// They do not overlap at all
				// This does not animate -- assuming it's programmatically set from view controller.
				// Recycle all onscreen covers.
				for (int i = _lowerVisibleCover; i <= _upperVisibleCover; i++)
				{
					var cover = _onscreenCovers[i];
					_offscreenCovers.Push(cover);
					cover.RemoveFromSuperview();
					_onscreenCovers.Remove(i);
				}

				// Move all available covers to new location
				for (int i = newLowerBound; i <= newUpperBound; i++)
				{
					var cover = CoverForIndex(i);
					_onscreenCovers[i] = cover;
					UpdateCoverImage(cover);
					_scrollView.Layer.AddSublayer(cover.Layer);
				}
				_lowerVisibleCover = newLowerBound;
				_upperVisibleCover = newUpperBound;
				_selectedCoverView = _onscreenCovers[newSelectedCover];
				LayoutCovers(newSelectedCover, newLowerBound, newUpperBound);
				return;
			}
			else if (newSelectedCover > _selectedCoverView.Number)
			{
				for (int i = _lowerVisibleCover; i < newLowerBound; i++)
				{
					var cover = _onscreenCovers[i];
					if (_upperVisibleCover < newUpperBound)
					{
						// Tack it on right side
						_upperVisibleCover++;
						cover.Number = _upperVisibleCover;
						UpdateCoverImage(cover);
						_onscreenCovers[cover.Number] = cover;
						LayoutCover(cover, newSelectedCover, false);
					}
					else
					{
						// Recycle this cover
						_offscreenCovers.Push(cover);
						cover.RemoveFromSuperview();
					}
					_onscreenCovers.Remove(i);
				}
				_lowerVisibleCover = newLowerBound;

				// Add in any missing covers on the right up to the newUpperBound.
				for (int i = _upperVisibleCover + 1; i <= newUpperBound; i++)
				{
					var cover = CoverForIndex(i);
					_onscreenCovers[i] = cover;
					UpdateCoverImage(cover);
					_scrollView.Layer.AddSublayer(cover.Layer);
					LayoutCover(cover, newSelectedCover, false);
				}
				_upperVisibleCover = newUpperBound;
			}
			else
			{
				// Move covers that are now out of range on the right to the left side.
				// but only if appropriate (within the range set by newLoweBound).
				for (int i = _upperVisibleCover; i > newUpperBound; i--)
				{
					var cover = _onscreenCovers[i];
					if (_lowerVisibleCover > newLowerBound)
					{
						// Tack it on the left
						_lowerVisibleCover--;
						cover.Number = _lowerVisibleCover;
						UpdateCoverImage(cover);
						_onscreenCovers[_lowerVisibleCover] = cover;
						LayoutCover(cover, newSelectedCover, false);
					}
					else
					{
						// Recycle this cover
						_offscreenCovers.Push(cover);
						cover.RemoveFromSuperview();
					}
				}
				_upperVisibleCover = newUpperBound;

				// Add in any missing covers on the left down to the newLowerBound
				for (int i = _lowerVisibleCover - 1; i >= newLowerBound; i--)
				{
					var cover = CoverForIndex(i);
					_onscreenCovers[i] = cover;
					UpdateCoverImage(cover);
					_scrollView.Layer.AddSublayer(cover.Layer);
					LayoutCover(cover, newSelectedCover, false);
				}
				_lowerVisibleCover = newLowerBound;
			}

			if (_selectedCoverView.Number > newSelectedCover)
				LayoutCovers(newSelectedCover, newSelectedCover, _selectedCoverView.Number);
			else if (newSelectedCover > _selectedCoverView.Number)
				LayoutCovers(newSelectedCover, _selectedCoverView.Number, newSelectedCover);

			_selectedCoverView = _onscreenCovers[newSelectedCover];
		}

		public void CenterOnSelectedCover(bool animated)
		{
			var selectedOffset = new System.Drawing.PointF((float)coverSpacing * _selectedCoverView.Number, 0);
			_scrollView.SetContentOffset(selectedOffset, animated);
		}

#if false
		public int Selected
		{
			get
			{
				if (_selectedCoverView == null)
					return -1;

				return _selectedCoverView.Number;
			}
		}
#endif

		public override void TouchesBegan(MonoTouch.Foundation.NSSet touches, MonoTouch.UIKit.UIEvent evt)
		{
			if (_selectedCoverView == null)
				return;

			var startPoint = ((MonoTouch.UIKit.UITouch)touches.AnyObject).LocationInView(this);
			_isDraggingACover = false;

			// Which cover did the user tap?
			var targetLayer = _scrollView.Layer.HitTest(startPoint);
			var targetCover = FindCoversOnScreen(targetLayer);
			_isDraggingACover = targetCover != null;

			_beginningCover = _selectedCoverView.Number;
			// Make sure the user is tapping on a cover.
			_startPosition = (float)((startPoint.X / 1.5) + _scrollView.ContentOffset.X);

			if (_isSingleTap)
				_isDoubleTap = true;

			_isSingleTap = touches.Count == 1;
		}

		public override void TouchesMoved(MonoTouch.Foundation.NSSet touches, MonoTouch.UIKit.UIEvent evt)
		{
			if (_selectedCoverView == null)
				return;

			_isSingleTap = false;
			_isDoubleTap = false;

			// Only scroll if the user started on a cover
			if (!_isDraggingACover)
				return;

			var movedPoint = ((MonoTouch.UIKit.UITouch)touches.AnyObject).LocationInView(this);
			var offset = _startPosition - (movedPoint.X / 1.5);
			var newPoint = new System.Drawing.PointF((float)offset, 0);
			_scrollView.ContentOffset = newPoint;
			int newCover = (int)(offset / coverSpacing);
			if (newCover != _selectedCoverView.Number)
			{
				if (newCover < 0)
					SetSelectedCover(0);
				else if (newCover >= _numberOfImages)
					SetSelectedCover(_numberOfImages - 1);
				else
					SetSelectedCover(newCover);

				if (0 <= newCover)
				{
					CallChangedDelegate(newCover);
				}
			}
		}

		public override void TouchesEnded(MonoTouch.Foundation.NSSet touches, MonoTouch.UIKit.UIEvent evt)
		{
			if (_selectedCoverView == null)
				return;

			if (_isSingleTap)
			{
				var targetPoint = ((MonoTouch.UIKit.UITouch)touches.AnyObject).LocationInView(this);
				var targetLayer = _scrollView.Layer.HitTest(targetPoint);
				var targetCover = FindCoversOnScreen(targetLayer);

				if (targetCover != null && (targetCover.Number != _selectedCoverView.Number))
					SetSelectedCover(targetCover.Number);
			}
			CenterOnSelectedCover(true);

			if (_beginningCover == _selectedCoverView.Number)
			{
				// アイテム選択
				if (null != _pickedDelegate)
				{
					_pickedDelegate(this, _selectedCoverView.Number);
				}
			}
		}

		private void CallChangedDelegate(int index)
		{
			if (null != _changedDelegate)
			{
				_changedDelegate(this, index);
			}
		}

		private void CallPickedDelegate(int index)
		{
			if (null != _pickedDelegate)
			{
				_pickedDelegate(this, index);
			}
		}

		/// <summary>
		/// カバー用画像クラス。
		/// </summary>
		class CoverImage : System.IDisposable
		{
			private MonoTouch.UIKit.UIImage _image;
			private float _height;

			public CoverImage(MonoTouch.UIKit.UIImage image)
			{
				_height = image.Size.Height;
				_image = ImageUtils.AddImageReflection(image, kReflectionFraction);
			}

			public MonoTouch.UIKit.UIImage Image
			{
				get { return _image; }
			}
			public float Height
			{
				get { return _height; }
			}

			#region IDisposable メンバ

			public void Dispose()
			{
				if (null != _image)
				{
					_image.Dispose();
				}
			}

			#endregion
		}
	}
}
