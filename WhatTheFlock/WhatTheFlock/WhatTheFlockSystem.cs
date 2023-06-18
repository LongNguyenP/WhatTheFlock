using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Autodesk.DesignScript.Runtime;
using Dynamo.Graph.Workspaces;
using HelixToolkit.Wpf.SharpDX;


namespace WhatTheFlock
{
    [IsVisibleInDynamoLibrary(false)]
    public class WhatTheFlockSystem : IDisposable
    {
        public int IterationCount = 1;
        public bool EnableFastDisplay = true;
        public float AgentDisplayedLength;
        public float AgentDisplayedWidth;

        internal WhatTheFlockDisplay WhatTheFlockDisplay;
        internal MeshGeometryModel3D MeshModel;

        internal Flock Flock = new Flock();

        private Task backgroundExecutionTask;
        private CancellationTokenSource ctSource;


        public WhatTheFlockSystem()
        {
            WhatTheFlockDisplay = new WhatTheFlockDisplay(this);

            WhatTheFlockViewExtension.ViewModel.ViewMouseDown += ViewportMouseDownHandler;
            WhatTheFlockViewExtension.ViewModel.ViewMouseUp += ViewportMouseUpHandler;
            WhatTheFlockViewExtension.ViewModel.ViewMouseMove += ViewportMouseMoveHandler;
            WhatTheFlockViewExtension.ViewModel.ViewCameraChanged += ViewportCameraChangedHandler;
            WhatTheFlockViewExtension.ViewModel.CanNavigateBackgroundPropertyChanged += ViewportCanNavigateBackgroundPropertyChangedHandler;
            WhatTheFlockViewExtension.Parameters.CurrentWorkspaceCleared += ParametersOnCurrentWorkspaceCleared;
        }


        public void InitizializeFlockAgents(int count = 100, bool is3D = true, float boundingBoxSize = 40f)
        {
            Flock = new Flock(count, is3D, boundingBoxSize);
        }


        public void Reset()
        {
        }


        public void Iterate(int iterationCount = 1)
        {
            for (int i = 0; i < iterationCount; i++)
                Flock.Update();
        }


        public void Clear()
        {
        }


        public void ClearRender()
        {
            WhatTheFlockDisplay?.ClearRender();
        }


        public void Render()
        {
            WhatTheFlockDisplay.Render();
        }

        private void BackgroundExecutionAction()
        {
            while (!ctSource.Token.IsCancellationRequested)
            {
                Iterate(IterationCount);
                if (EnableFastDisplay) WhatTheFlockDisplay.Render();
            }
        }


        public void StartBackgroundExecution()
        {
            if (backgroundExecutionTask != null && backgroundExecutionTask.Status == TaskStatus.Running) return;
            ctSource = new CancellationTokenSource();
            backgroundExecutionTask = Task.Factory.StartNew(BackgroundExecutionAction, ctSource.Token);
        }


        public void StopBackgroundExecution()
        {
            if (backgroundExecutionTask == null) return;
            ctSource?.Cancel();
            backgroundExecutionTask?.Wait(300);
            WhatTheFlockDisplay.DispatcherOperation?.Task.Wait(300);
        }


        private void ViewportCameraChangedHandler(object sender, RoutedEventArgs args)
        {
        }


        private void ViewportMouseDownHandler(object sender, MouseButtonEventArgs args)
        {
        }


        private void ViewportMouseUpHandler(object sender, MouseButtonEventArgs args)
        {
        }


        private void ViewportMouseMoveHandler(object sender, MouseEventArgs args)
        {
        }


        private void ViewportCanNavigateBackgroundPropertyChangedHandler(bool canNavigate)
        {
        }


        private void ParametersOnCurrentWorkspaceCleared(IWorkspaceModel obj)
        {
        }


        public void Dispose()
        {
            StopBackgroundExecution();
            WhatTheFlockViewExtension.ViewModel.ViewMouseDown -= ViewportMouseDownHandler;
            WhatTheFlockViewExtension.ViewModel.ViewMouseUp -= ViewportMouseUpHandler;
            WhatTheFlockViewExtension.ViewModel.ViewMouseMove -= ViewportMouseMoveHandler;
            WhatTheFlockViewExtension.ViewModel.ViewCameraChanged -= ViewportCameraChangedHandler;
            WhatTheFlockViewExtension.ViewModel.CanNavigateBackgroundPropertyChanged -= ViewportCanNavigateBackgroundPropertyChangedHandler;
            WhatTheFlockDisplay.Dispose();
            WhatTheFlockViewExtension.Parameters.CurrentWorkspaceCleared -= ParametersOnCurrentWorkspaceCleared;
        }
    }
}