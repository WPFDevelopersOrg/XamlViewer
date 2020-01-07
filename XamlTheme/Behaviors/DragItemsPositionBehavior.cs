using System;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Documents;
using XamlTheme.Adorners;
using XamlTheme.Utils;
using XamlUtil.Common;
using Microsoft.Xaml.Behaviors;

namespace XamlTheme.Behaviors
{
    public class DragItemsPositionBehavior : Behavior<Panel>
    {
        private Point _cacheMouseDownToChildPos;
        private Point _cacheChildToPanelPos;
        private UIElement _dragedChild = null;

        private MousePanelAdorner _panelAdorner = null;
        private MousePanelAdorner GetPanelAdorner(UIElement panel, UIElement draggedChild)
        {
            if (_panelAdorner == null)
                _panelAdorner = ConstructMousePanelAdorner(panel, draggedChild);

            return _panelAdorner;
        }

        private AdornerLayer _rootAdornerLayer = null;
        private AdornerLayer RootAdornerLayer
        {
            get
            {
                if (_rootAdornerLayer == null)
                    _rootAdornerLayer = AdornerLayer.GetAdornerLayer(AdornerLayerProvider);

                if (_rootAdornerLayer == null)
                    throw new Exception("There is no AdornerLayer in RootElement.");

                return _rootAdornerLayer;
            }
        }

        private bool? _isFromItemsPanelTemplate = null;
        private bool IsFromItemsPanelTemplate
        {
            get
            {
                if (!_isFromItemsPanelTemplate.HasValue)
                    _isFromItemsPanelTemplate = VisualTreeHelper.GetParent(AssociatedObject) is ItemsPresenter;

                return _isFromItemsPanelTemplate.Value;
            }
        }

        private ItemsControl _itemsContainer = null;
        private ItemsControl ItemsContainer
        {
            get
            {
                if (_itemsContainer == null)
                {
                    DependencyObject associatedObject = AssociatedObject;
                    for (DependencyObject i = associatedObject; i != null; i = VisualTreeHelper.GetParent(associatedObject))
                    {
                        if (i is ItemsControl)
                        {
                            _itemsContainer = i as ItemsControl;
                            break;
                        }

                        associatedObject = i;
                    }
                }

                return _itemsContainer;
            }
        }

        private UIElement _adornerLayerProvider = null;
        private UIElement AdornerLayerProvider
        {
            get
            {
                if (_adornerLayerProvider == null)
                {
                    DependencyObject topObjectWithAdornerLayer = null;
                    DependencyObject associatedObject = AssociatedObject;

                    for (DependencyObject i = associatedObject; i != null; i = VisualTreeHelper.GetParent(associatedObject))
                    {
                        if (AdornerLayer.GetAdornerLayer((Visual)i) != null)
                            topObjectWithAdornerLayer = i;

                        associatedObject = i;
                    }

                    _adornerLayerProvider = topObjectWithAdornerLayer as UIElement;
                }

                return _adornerLayerProvider;
            }
        }

        public static readonly DependencyProperty MoveItemFromItemsSourceProperty =
            DependencyProperty.Register("MoveItemFromItemsSource", typeof(Action<int, int>), typeof(DragItemsPositionBehavior));
        public Action<int, int> MoveItemFromItemsSource
        {
            get { return (Action<int, int>)GetValue(MoveItemFromItemsSourceProperty); }
            set { SetValue(MoveItemFromItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty DisabledYPositionProperty =
            DependencyProperty.Register("DisabledYPosition", typeof(bool), typeof(DragItemsPositionBehavior), new PropertyMetadata(false));
        public bool DisabledYPosition
        {
            get { return (bool)GetValue(DisabledYPositionProperty); }
            set { SetValue(DisabledYPositionProperty, value); }
        }

        #region Override

        protected override void OnAttached()
        {
            AssociatedObject.AddHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(OnMouseLeftButtonDown), true);
        }

        protected override void OnDetaching()
        {
            AssociatedObject.RemoveHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(OnMouseLeftButtonDown));
        }

        #endregion

        #region Event

        private void OnQueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            _panelAdorner.Update();
            MoveChild(_dragedChild);
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            StartDrag();
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (AssociatedObject.Children == null || AssociatedObject.Children.Count == 0)
                return;

            foreach (UIElement child in AssociatedObject.Children)
            {
                _cacheMouseDownToChildPos = e.GetPosition(child);

                var hitResult = VisualTreeHelper.HitTest(child, _cacheMouseDownToChildPos);
                if (hitResult != null)
                {
                    _cacheChildToPanelPos = child.TranslatePoint(new Point(), AssociatedObject);
                    _dragedChild = child;
					
                    AssociatedObject.PreviewMouseMove += OnMouseMove;

                    break;
                }
            }
        }

        #endregion

        #region Func

        private MousePanelAdorner ConstructMousePanelAdorner(UIElement panel, UIElement draggedChild)
        {
            if (panel == null || draggedChild == null)
                return null;

            return new MousePanelAdorner(panel, draggedChild as FrameworkElement, _cacheMouseDownToChildPos, _cacheChildToPanelPos, DisabledYPosition);
        } 

        private void StartDrag()
        {
            if (_panelAdorner != null || _dragedChild == null)
                return;

            RootAdornerLayer.Add(GetPanelAdorner(AssociatedObject, _dragedChild));
            _dragedChild.Opacity = 0; 

            DragDrop.AddQueryContinueDragHandler(AssociatedObject, OnQueryContinueDrag);
            DragDrop.DoDragDrop(AssociatedObject, _dragedChild, DragDropEffects.Move);
            DragDrop.RemoveQueryContinueDragHandler(AssociatedObject, OnQueryContinueDrag);

            EndDrag();
        }

        private void EndDrag()
        { 
            AssociatedObject.PreviewMouseMove -= OnMouseMove;

            RootAdornerLayer.Remove(_panelAdorner);
            _panelAdorner = null;

            _dragedChild.Opacity = 1;
            _dragedChild = null;
        }

        private void MoveChild(UIElement dragedChild)
        { 
            var screenPos = new Win32.POINT();
            if (!Win32.GetCursorPos(ref screenPos))
            	return;
            
            var posToPanel = AssociatedObject.PointFromScreen(new Point(screenPos.X, screenPos.Y)); 
            var dragedElement = dragedChild as FrameworkElement;
			
            var childRect = new Rect(posToPanel.X - _cacheMouseDownToChildPos.X, DisabledYPosition ? _cacheChildToPanelPos.Y : (posToPanel.Y - _cacheMouseDownToChildPos.Y), dragedElement.ActualWidth, dragedElement.ActualHeight);

            //find the child which has max overlapping area with dragedChild
            Size? maxOverlapSize = null;
            FrameworkElement maxOverlapChild = null;
            foreach (FrameworkElement fe in AssociatedObject.Children)
            {
                if (fe == dragedElement)
                    continue;

                var sp = fe.TranslatePoint(new Point(), AssociatedObject);
                var overlapSize = GetOverlapSize(new Rect(sp, new Point(sp.X + fe.ActualWidth, sp.Y + fe.ActualHeight)), childRect);

                if (overlapSize.IsEmpty)
                    continue;

                if (maxOverlapSize == null || DoubleUtil.GreaterThan(overlapSize.Width * overlapSize.Height, maxOverlapSize.Value.Width * maxOverlapSize.Value.Height))
                {
                    maxOverlapSize = overlapSize;
                    maxOverlapChild = fe;
                }
            }

            //check the overlapping area whether match the exchanging child condition
            if (!maxOverlapSize.HasValue || maxOverlapSize.Value.IsEmpty)
                return;

            if (DoubleUtil.GreaterThanOrClose(maxOverlapSize.Value.Width, maxOverlapChild.ActualWidth / 2) && DoubleUtil.GreaterThanOrClose(maxOverlapSize.Value.Height, maxOverlapChild.ActualHeight / 2))
            {
                var targetIndex = AssociatedObject.Children.IndexOf(maxOverlapChild);

                if (IsFromItemsPanelTemplate)
                {
                    var sourceIndex = AssociatedObject.Children.IndexOf(dragedChild);

                    if (ItemsContainer.ItemsSource != null)
                    {
                        if (MoveItemFromItemsSource != null)
                        {
                            MoveItemFromItemsSource(sourceIndex, targetIndex);

                            //if use ObservableCollection.Move(...) to exchange position, follow code is unnecessary.
                            //else use ObservableCollection.RemoveAt(...) and ObservableCollection.Insert(...) to exchange position, follow code is necessary.
                            _dragedChild = AssociatedObject.Children[targetIndex];
                            _dragedChild.Opacity = 0;
                        }
                    }
                    else
                    {
                        var sourceItem = ItemsContainer.Items[sourceIndex];

                        ItemsContainer.Items.RemoveAt(sourceIndex);
                        ItemsContainer.Items.Insert(targetIndex, sourceItem);

                        _dragedChild = AssociatedObject.Children[targetIndex];
                        _dragedChild.Opacity = 0;
                    }
                }
                else
                {
                    AssociatedObject.Children.Remove(dragedChild);
                    AssociatedObject.Children.Insert(targetIndex, dragedChild);
                }
            }
        }

        private Size GetOverlapSize(Rect rect1, Rect rect2)
        {
            return Rect.Intersect(rect1, rect2).Size;
        }

        #endregion
    }
}
