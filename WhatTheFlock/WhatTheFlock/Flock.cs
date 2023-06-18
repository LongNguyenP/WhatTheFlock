using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Autodesk.DesignScript.Runtime;
using SharpDX;

namespace WhatTheFlock
{
    [IsVisibleInDynamoLibrary(false)]
    public class Flock
    {
        public List<FlockAgent> Agents = new List<FlockAgent>();

        public float BoundingBoxSize;
        public float Timestep;
        public float NeighborhoodRadius;
        public float AlignmentStrength;
        public float CohesionStrength;
        public float SeparationStrength;
        public float SeparationDistance;
        public bool UseParallel = true;
        public bool UseKdTree = true;

        public List<Triple> RepellerCenters = new List<Triple>();
        public List<float> RepellerRadii = new List<float>();

        internal int Iterations = 0;
        private Stopwatch stopwatch = new Stopwatch();
        private KdTree kdTree;

        internal static Random random = new Random();


        public Flock()
        {
        }


        public Flock(int agentCount, bool is3D, float boundingBoxSize)
        {
            Agents = new List<FlockAgent>();

            if (is3D)
                for (int i = 0; i < agentCount; i++)
                {
                    FlockAgent agent = new FlockAgent(
                        GetRandomPoint(
                            -0.5f * boundingBoxSize, 0.5f * boundingBoxSize,
                            -0.5f * boundingBoxSize, 0.5f * boundingBoxSize,
                            0, boundingBoxSize),
                        GetRandomUnitVector() * 4f);
                    agent.Flock = this;
                    Agents.Add(agent);
                }
            else
                for (int i = 0; i < agentCount; i++)
                {
                    FlockAgent agent = new FlockAgent(
                        GetRandomPoint(-0.5f * boundingBoxSize, 0.5f * boundingBoxSize, -0.5f * boundingBoxSize, 0.5f * boundingBoxSize, 0f, 0f),
                        GetRandomUnitVectorXZ() * 4f);
                    agent.Flock = this;
                    Agents.Add(agent);
                }
        }


        private List<FlockAgent> FindNeighbors(FlockAgent agent)
        {
            List<FlockAgent> neighbors = new List<FlockAgent>();

            foreach (FlockAgent neighbor in Agents)
                if (neighbor != agent &&
                    (neighbor.Position - agent.Position).LengthSquared < NeighborhoodRadius * NeighborhoodRadius)
                {
                    neighbors.Add(neighbor);
                }

            return neighbors;
        }


        private List<FlockAgent> FindNeighborsUsingKdTree(FlockAgent agent)
        {
            List<int> ids = kdTree.Search(agent.Position.X, agent.Position.Y, agent.Position.Z, NeighborhoodRadius);
            List<FlockAgent> neighbors = new List<FlockAgent>();
            foreach (int id in ids)
                if (Agents[id] != agent)
                    neighbors.Add(Agents[id]);
            return neighbors;
        }


        public void Update()
        {
            Iterations++;

            if (UseKdTree)
            {
                float[] xs = new float[Agents.Count];
                float[] ys = new float[Agents.Count];
                float[] zs = new float[Agents.Count];
                int[] ids = new int[Agents.Count];

                for (int i = 0; i < Agents.Count; i++)
                {
                    xs[i] = Agents[i].Position.X;
                    ys[i] = Agents[i].Position.Y;
                    zs[i] = Agents[i].Position.Z;
                    ids[i] = i;
                }

                kdTree = new KdTree(xs, ys, zs, ids);

                if (UseParallel)
                    Parallel.ForEach(Agents, agent => { agent.ComputeDesiredVelocity(FindNeighborsUsingKdTree(agent)); });
                else
                    foreach (FlockAgent agent in Agents)
                        agent.ComputeDesiredVelocity(FindNeighborsUsingKdTree(agent));
            }
            else
            {
                if (UseParallel)
                    Parallel.ForEach(Agents, agent => { agent.ComputeDesiredVelocity(FindNeighbors(agent)); });
                else
                    foreach (FlockAgent agent in Agents)
                        agent.ComputeDesiredVelocity(FindNeighbors(agent));
            }

            // Once the desired velocity for each agent has been computed, we update each position and velocity

            if (UseParallel) Parallel.ForEach(Agents, agent => agent.UpdateVelocityAndPosition());
            else
                foreach (FlockAgent agent in Agents)
                    agent.UpdateVelocityAndPosition();
        }


        private static Triple GetRandomPoint(float minX, float maxX, float minY, float maxY, float minZ, float maxZ)
        {
            return new Triple(
                random.NextFloat(minX, maxX),
                random.NextFloat(minY, maxY),
                random.NextFloat(minZ, maxZ));
        }


        private static Triple GetRandomUnitVector()
        {
            double phi = random.NextFloat(0f, 2f * (float)Math.PI);
            double theta = Math.Acos(random.NextFloat(-1f, 1f));
            return new Triple(
                (float)(Math.Sin(theta) * Math.Cos(phi)),
                (float)(Math.Sin(theta) * Math.Sin(phi)),
                (float)(Math.Cos(theta)));
        }


        public static Triple GetRandomUnitVectorXZ(float y = 0f)
        {
            double angle = random.NextFloat(0f, 2f * (float)Math.PI);
            return new Triple((float)Math.Cos(angle), 0f, (float)Math.Sin(angle));
        }
    }
}