using System;
using System.Windows.Threading;
using Autodesk.DesignScript.Runtime;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;


namespace WhatTheFlock
{
    [IsVisibleInDynamoLibrary(false)]
    public class WhatTheFlockDisplay : IDisposable
    {
        public static readonly Color4 HandleColor = new Color4(1.0f, 0.5f, 0.0f, 1.0f);

        private readonly WhatTheFlockSystem whatTheFlockSystem;

        internal MeshGeometryModel3D FlockMeshModel;

        public WhatTheFlockDisplay(WhatTheFlockSystem whatTheFlockSystem)
        {
            this.whatTheFlockSystem = whatTheFlockSystem;

            WhatTheFlockViewExtension.DynamoWindow.Dispatcher.Invoke(
                () =>
                {
                    MeshGeometry3D flockMesh = new MeshGeometry3D()
                    {
                        Positions = new Vector3Collection(),
                        Indices = new IntCollection(),
                        Normals = new Vector3Collection(),
                    };

                    FlockMeshModel = new MeshGeometryModel3D()
                    {
                        Geometry = flockMesh,
                        Material = new PhongMaterial()
                        {
                            DiffuseColor = new Color4(0.9f, 0.4f, 0.2f, 1f)
                        }
                    };
                },
                DispatcherPriority.Send);

            WhatTheFlockViewExtension.ViewModel.RequestViewRefresh += RequestViewRefreshHandler;
            WhatTheFlockViewExtension.DynamoWindow.Closed += (sender, args) => Dispose();
        }


        internal void Render()
        {
            WhatTheFlockViewExtension.DynamoWindow.Dispatcher.Invoke(RenderAction, DispatcherPriority.Send);
        }


        internal void ClearRender()
        {
            WhatTheFlockViewExtension.ViewModel.RequestViewRefresh -= RequestViewRefreshHandler;
            WhatTheFlockViewExtension.DynamoWindow.Dispatcher.Invoke(() =>
            {
                ObservableElement3DCollection sceneItems = WhatTheFlockViewExtension.GetSceneItems();

                if (sceneItems.Contains(FlockMeshModel)) sceneItems.Remove(FlockMeshModel);
                FlockMeshModel.Dispose();
            });
        }


        private void RenderAction()
        {
            ObservableElement3DCollection sceneItems = WhatTheFlockViewExtension.GetSceneItems();



            //============================================================================
            // Render Stuff
            //============================================================================

            MeshGeometry3D flockMesh = (MeshGeometry3D)FlockMeshModel.Geometry;

            flockMesh.Positions.Clear();
            flockMesh.Indices.Clear();
            flockMesh.Normals.Clear();

            float cos30 = 0.86602540378f;
            float l = whatTheFlockSystem.AgentDisplayedLength;
            float w = whatTheFlockSystem.AgentDisplayedWidth * 0.5f;
            Triple M_ = new Triple(0f, 0f, l * 0.6666f);
            Triple A_ = new Triple(w * cos30, w * 0.5f, -l * 0.3333f);
            Triple B_ = new Triple(-w * cos30, w * 0.5f, -l * 0.3333f);
            Triple C_ = new Triple(0, -w, -l * 0.3333f);

            for (int i = 0; i < whatTheFlockSystem.Flock.Agents.Count; i++)
            {
                FlockAgent agent = whatTheFlockSystem.Flock.Agents[i];

                Triple p = agent.Position;

                Triple z = agent.Velocity.Normalise();
                Triple x = z.GeneratePerpendicular();
                Triple y = z.Cross(x);

                Vector3 A = (p + A_.X * x + A_.Y * y + A_.Z * z).ToVector3();
                Vector3 B = (p + B_.X * x + B_.Y * y + B_.Z * z).ToVector3();
                Vector3 C = (p + C_.X * x + C_.Y * y + C_.Z * z).ToVector3();
                Vector3 M = (p + M_.X * x + M_.Y * y + M_.Z * z).ToVector3();

                flockMesh.Positions.Add(C);
                flockMesh.Positions.Add(B);
                flockMesh.Positions.Add(A);

                flockMesh.Positions.Add(M);
                flockMesh.Positions.Add(A);
                flockMesh.Positions.Add(B);

                flockMesh.Positions.Add(M);
                flockMesh.Positions.Add(B);
                flockMesh.Positions.Add(C);

                flockMesh.Positions.Add(M);
                flockMesh.Positions.Add(C);
                flockMesh.Positions.Add(A);

                int indexOffset = 12 * i;

                flockMesh.Indices.Add(indexOffset);
                flockMesh.Indices.Add(indexOffset + 1);
                flockMesh.Indices.Add(indexOffset + 2);
                flockMesh.Indices.Add(indexOffset + 3);
                flockMesh.Indices.Add(indexOffset + 4);
                flockMesh.Indices.Add(indexOffset + 5);
                flockMesh.Indices.Add(indexOffset + 6);
                flockMesh.Indices.Add(indexOffset + 7);
                flockMesh.Indices.Add(indexOffset + 8);
                flockMesh.Indices.Add(indexOffset + 9);
                flockMesh.Indices.Add(indexOffset + 10);
                flockMesh.Indices.Add(indexOffset + 11);

                Vector3 n = Vector3.Cross(C - A, B - A);
                flockMesh.Normals.Add(n);
                flockMesh.Normals.Add(n);
                flockMesh.Normals.Add(n);

                n = Vector3.Cross(A - M, B - M);
                flockMesh.Normals.Add(n);
                flockMesh.Normals.Add(n);
                flockMesh.Normals.Add(n);

                n = Vector3.Cross(B - M, C - M);
                flockMesh.Normals.Add(n);
                flockMesh.Normals.Add(n);
                flockMesh.Normals.Add(n);

                n = Vector3.Cross(C - M, A - M);
                flockMesh.Normals.Add(n);
                flockMesh.Normals.Add(n);
                flockMesh.Normals.Add(n);
            }

            flockMesh.UpdateVertices();
            flockMesh.UpdateTriangles();

            if (!sceneItems.Contains(FlockMeshModel))
                sceneItems.Add(FlockMeshModel);
        }


        public void Dispose()
        {
            ClearRender();
        }


        // This handler prevents flickering when geometries other than DynaShape ones exist in the viewport
        private void RequestViewRefreshHandler()
        {
            ObservableElement3DCollection sceneItems = WhatTheFlockViewExtension.GetSceneItems();
            if (!sceneItems.Contains(FlockMeshModel))
                sceneItems.Add(FlockMeshModel);
        }
    }
}