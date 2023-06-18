using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Autodesk.DesignScript.Runtime;
using Dynamo.Wpf.Extensions;
using Dynamo.Wpf.ViewModels.Watch3D;
using System;
using Dynamo.Controls;
using HelixToolkit.Wpf.SharpDX;


namespace WhatTheFlock
{
    [IsVisibleInDynamoLibrary(false)]
    internal class WhatTheFlockViewExtension : IViewExtension
    {
        public static ViewLoadedParams Parameters;
        public static Window DynamoWindow;
        public static HelixWatch3DViewModel ViewModel;
        public static CameraData CameraData;
        public static Triple MouseRayOrigin;
        public static Triple MouseRayDirection;


        public void Dispose() { }


        public void Startup(ViewStartupParams parameters) { }


        public void Loaded(ViewLoadedParams parameters)
        {
            Parameters = parameters;
            DynamoWindow = parameters.DynamoWindow;
            ViewModel = parameters.BackgroundPreviewViewModel as HelixWatch3DViewModel;
            if (ViewModel == null) throw new Exception("Could not obtain HelixWatch3DViewModel. Sad!");

            ViewModel.ViewCameraChanged += ViewModelViewCameraChangedHandler;
            ViewModel.ViewMouseDown += ViewModelViewMouseDownHandler;
            ViewModel.ViewMouseMove += ViewModelViewMouseMoveHandler;
            ViewModel.RequestViewRefresh += ViewModelRequestViewRefreshHandler;
        }


        private void ViewModelRequestViewRefreshHandler()
        {
        }


        private void ViewModelViewMouseDownHandler(object sender, MouseButtonEventArgs e)
        {
        }


        internal static Viewport3DX GetViewport()
        {
            return
                ((Watch3DView)
                    ((Grid)
                        ((Grid)
                            DynamoWindow.Content)
                        .Children[2])
                    .Children[1])
                .View;
        }


        internal static ObservableElement3DCollection GetSceneItems()
            => (ObservableElement3DCollection)ViewModel.SceneItems;


        public void Shutdown()
        {
            ViewModel.ViewCameraChanged -= ViewModelViewCameraChangedHandler;
            ViewModel.ViewMouseDown -= ViewModelViewMouseDownHandler;
            ViewModel.ViewMouseMove -= ViewModelViewMouseMoveHandler;
            ViewModel.RequestViewRefresh -= ViewModelRequestViewRefreshHandler;
        }


        private void ViewModelViewCameraChangedHandler(object sender, RoutedEventArgs e)
        {
            CameraData = ViewModel.GetCameraInformation();
        }


        private void ViewModelViewMouseMoveHandler(object sender, MouseEventArgs e)
        {
            IRay clickRay = ViewModel.GetClickRay(e);
            MouseRayOrigin = new Triple(clickRay.Origin.X, clickRay.Origin.Y, clickRay.Origin.Z);
            MouseRayDirection = new Triple(clickRay.Direction.X, clickRay.Direction.Y, clickRay.Direction.Z);
        }


        public string UniqueId => "{E2765389-E4A7-4D0A-8CBC-EC3D111FB0D4}";
        public string Name => "DynamoPlaygroundViewExtension";
    }
}