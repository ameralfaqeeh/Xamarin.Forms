﻿using System;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class StructuredItemsViewController<TItemsView> : ItemsViewController<TItemsView>
		where TItemsView : StructuredItemsView
	{
		bool _disposed;

		UIView _headerUIView;
		VisualElement _headerViewFormsElement;

		UIView _footerUIView;
		VisualElement _footerViewFormsElement;

		public StructuredItemsViewController(TItemsView structuredItemsView, ItemsViewLayout layout)
			: base(structuredItemsView, layout)
		{
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			_disposed = true;

			if (disposing)
			{
				_headerUIView = null;
				_headerViewFormsElement = null;
				_footerUIView = null;
				_footerViewFormsElement = null;
				
			}

			base.Dispose(disposing);
		}

		protected override bool IsHorizontal => (ItemsView?.ItemsLayout as ItemsLayout)?.Orientation == ItemsLayoutOrientation.Horizontal;

		public override void ViewWillLayoutSubviews()
		{
			base.ViewWillLayoutSubviews();

			// This update is only relevant if you have a footer view because it's used to place the footer view
			// based on the ContentSize so we just update the positions if the ContentSize has changed
			if (_footerUIView != null)
			{
				if (IsHorizontal)
				{
					if (_footerUIView.Frame.X != ItemsViewLayout.CollectionViewContentSize.Width)
						UpdateHeaderFooterPosition();
				}
				else
				{
					if (_footerUIView.Frame.Y != ItemsViewLayout.CollectionViewContentSize.Height)
						UpdateHeaderFooterPosition();
				}
			}
		}

		internal void UpdateFooterView()
		{
			UpdateSubview(ItemsView?.Footer, ItemsView?.FooterTemplate, 
				ref _footerUIView, ref _footerViewFormsElement);
			UpdateHeaderFooterPosition();
		}

		internal void UpdateHeaderView()
		{
			UpdateSubview(ItemsView?.Header, ItemsView?.HeaderTemplate, 
				ref _headerUIView, ref _headerViewFormsElement);
			UpdateHeaderFooterPosition();
		}

		internal void UpdateSubview(object view, DataTemplate viewTemplate, ref UIView uiView, ref VisualElement formsElement)
		{
			uiView?.RemoveFromSuperview();

			if (formsElement != null)
			{
				ItemsView.RemoveLogicalChild(formsElement);
				formsElement.MeasureInvalidated -= OnFormsElementMeasureInvalidated;
			}

			UpdateView(view, viewTemplate, ref uiView, ref formsElement);

			if (uiView != null)
			{
				CollectionView.AddSubview(uiView);
			}

			if (formsElement != null)
				ItemsView.AddLogicalChild(formsElement);

			if (formsElement != null)
			{
				RemeasureLayout(formsElement);
				formsElement.MeasureInvalidated += OnFormsElementMeasureInvalidated;
			}
			else if (uiView != null)
			{
				uiView.SizeToFit();
			}
		}

		void UpdateHeaderFooterPosition()
		{
			if (IsHorizontal)
			{
				var currentInset = CollectionView.ContentInset;

				nfloat headerWidth = _headerUIView?.Frame.Width ?? 0f;
				nfloat footerWidth = _footerUIView?.Frame.Width ?? 0f;

				nfloat headerSpacing = (_headerUIView?.Superview != null) ? headerWidth : 0f;
				UpdateCollectionViewHeaderSpacing(headerSpacing);

				if (_headerUIView != null && _headerUIView.Frame.X != headerWidth)
					_headerUIView.Frame = new CoreGraphics.CGRect(0, 0, headerWidth, CollectionView.Frame.Height);

				if (_footerUIView != null && (_footerUIView.Frame.X != ItemsViewLayout.CollectionViewContentSize.Width))
					_footerUIView.Frame = new CoreGraphics.CGRect(ItemsViewLayout.CollectionViewContentSize.Width, 0, footerWidth, CollectionView.Frame.Height);

				if (CollectionView.ContentInset.Left != headerWidth || CollectionView.ContentInset.Right != footerWidth)
				{
					var currentOffset = CollectionView.ContentOffset;
					CollectionView.ContentInset = new UIEdgeInsets(0, 0, 0, footerWidth);

					var xOffset = currentOffset.X + (currentInset.Left - CollectionView.ContentInset.Left);

					if (CollectionView.ContentSize.Width + headerWidth <= CollectionView.Bounds.Width)
						xOffset = -headerWidth;

					// if the header grows it will scroll off the screen because if you change the content inset iOS adjusts the content offset so the list doesn't move
					// this changes the offset of the list by however much the header size has changed
					CollectionView.ContentOffset = new CoreGraphics.CGPoint(xOffset, CollectionView.ContentOffset.Y);
				}
			}
			else
			{
				var currentInset = CollectionView.ContentInset;
				nfloat headerHeight = _headerUIView?.Frame.Height ?? 0f;
				nfloat footerHeight = _footerUIView?.Frame.Height ?? 0f;

				nfloat headerSpacing = (_headerUIView?.Superview != null) ? headerHeight : 0f;
				UpdateCollectionViewHeaderSpacing(headerSpacing);

				if (CollectionView.ContentInset.Top != headerHeight || CollectionView.ContentInset.Bottom != footerHeight)
				{
					var currentOffset = CollectionView.ContentOffset;
	 
					CollectionView.ContentInset = new UIEdgeInsets(0, 0, footerHeight, 0);

					// if the header grows it will scroll off the screen because if you change the content inset iOS adjusts the content offset so the list doesn't move
					// this changes the offset of the list by however much the header size has changed
					var yOffset = currentOffset.Y + (currentInset.Top - CollectionView.ContentInset.Top);

					if (CollectionView.ContentSize.Height + headerHeight <= CollectionView.Bounds.Height)
						yOffset = -headerHeight;

					CollectionView.ContentOffset = new CoreGraphics.CGPoint(CollectionView.ContentOffset.X, yOffset);
				}

				if (_headerUIView != null && _headerUIView.Frame.Y != headerHeight)
				{
					_headerUIView.Frame = new CoreGraphics.CGRect(0, 0, CollectionView.Frame.Width, headerHeight);
				}

				if (_footerUIView != null && (_footerUIView.Frame.Y != ItemsViewLayout.CollectionViewContentSize.Height))
				{
					_footerUIView.Frame = new CoreGraphics.CGRect(0, ItemsViewLayout.CollectionViewContentSize.Height, CollectionView.Frame.Width, footerHeight);
				}
			}
		}

  		void UpdateCollectionViewHeaderSpacing(nfloat headerSpacing)
		{
			if (CollectionView.CollectionViewLayout is ItemsViewLayout itemsViewLayout)
			{
				nfloat footerHeight = _footerUIView?.Frame.Height ?? 0f;
				CollectionView.ContentInset = new UIEdgeInsets(1, 0, footerHeight, 0);

				itemsViewLayout?.UpdateCollectionViewHeaderSpacing(headerSpacing);
			}
		}

		protected override void HandleFormsElementMeasureInvalidated(VisualElement formsElement)
		{
			base.HandleFormsElementMeasureInvalidated(formsElement);
			UpdateHeaderFooterPosition();
		}

		internal void UpdateLayoutMeasurements()
		{
			if (_headerViewFormsElement != null)
				HandleFormsElementMeasureInvalidated(_headerViewFormsElement);

			if (_footerViewFormsElement != null)
				HandleFormsElementMeasureInvalidated(_footerViewFormsElement);
		}
	}
}