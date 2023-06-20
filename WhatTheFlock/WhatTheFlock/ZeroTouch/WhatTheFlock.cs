using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;


namespace WhatTheFlock.ZeroTouch
{
    public static class WhatTheFlock
    {
        public static WhatTheFlockSystem Create()
        {
            return new WhatTheFlockSystem();
        }


        /// <summary>
        /// Run the flocking simulation
        /// </summary>
        /// <param name="whatTheFlock">The flocking simulation engine</param>
        /// <param name="agentCount">The number of agents to be created for simulation, with random initial positions and directions (this input will be ignored if agentInitialPositions input is not-null)</param>
        /// <param name="agentInitialPositions">The initial positions of the agents. If null. The positions will be created randomly</param>
        /// <param name="agentInitialDirections">The initial directions of the agents. If null. The directions will be created randomly</param>
        /// <param name="is3D"></param>
        /// <param name="iterations"></param>
        /// <param name="reset"></param>
        /// <param name="execute"></param>
        /// <param name="boundingBoxSize"></param>
        /// <param name="timeStep"></param>
        /// <param name="neighborhoodRadius"></param>
        /// <param name="alignmentStrength"></param>
        /// <param name="cohesionStrength"></param>
        /// <param name="separationStrength"></param>
        /// <param name="separationDistance"></param>
        /// <param name="sphereRepellers"></param>
        /// <param name="agentDisplayedLength"></param>
        /// <param name="agentDisplayedWidth"></param>
        /// <param name="outputDynamoGeometries">If true, output agent positions and directions as Dynamo points and vectors when the simulation is paused (i.e. when the "execute" input is False)</param>
        /// <returns></returns>
        [MultiReturn("agentPositions", "agentDirections")]
        public static Dictionary<string, object> Execute(
            WhatTheFlockSystem whatTheFlock,
            [DefaultArgument("100")] int agentCount,
            [DefaultArgument("true")] bool is3D,
            [DefaultArgument("null")] List<Point> agentInitialPositions,
            [DefaultArgument("null")] List<Vector> agentInitialDirections,
            [DefaultArgument("1")] int iterations,
            [DefaultArgument("true")] bool reset,
            [DefaultArgument("true")] bool execute,
            [DefaultArgument("40.0")] float boundingBoxSize,
            [DefaultArgument("0.015")] float timeStep,
            [DefaultArgument("1.0")] float neighborhoodRadius,
            [DefaultArgument("10.0")] float alignmentStrength,
            [DefaultArgument("5.0")] float cohesionStrength,
            [DefaultArgument("5.0")] float separationStrength,
            [DefaultArgument("0.5")] float separationDistance,
            [DefaultArgument("null")] List<Sphere> sphereRepellers,
            [DefaultArgument("0.55")] float agentDisplayedLength,
            [DefaultArgument("0.35")] float agentDisplayedWidth,
            [DefaultArgument("true")] bool outputDynamoGeometries
        )
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            bool enableFastDisplay = true;

            if (reset)
            {
                whatTheFlock.StopBackgroundExecution();

                if (agentInitialPositions == null)
                {
                    whatTheFlock.InitizializeFlockAgents(agentCount, is3D, boundingBoxSize);
                }
                else
                {
                    whatTheFlock.Flock = new Flock()
                    {
                        Agents = new List<FlockAgent>(agentInitialPositions.Count)
                    };

                    if (agentInitialDirections != null)
                    {
                        for (int i = 0; i < agentInitialPositions.Count; i++)
                        {
                            Triple v = agentInitialDirections[i].ToTriple();
                            v *= 4.0f / v.Length;
                            whatTheFlock.Flock.Agents.Add(new FlockAgent(agentInitialPositions[i].ToTriple(), v) { Flock = whatTheFlock.Flock });
                        }
                    }
                    else
                    {
                        for (int i = 0; i < agentInitialPositions.Count; i++)
                        {
                            whatTheFlock.Flock.Agents.Add(new FlockAgent(agentInitialPositions[i].ToTriple(), Util.GetRandomUnitVector() * 4f) { Flock = whatTheFlock.Flock });
                        }
                    }
                }

                whatTheFlock.AgentDisplayedLength = agentDisplayedLength;
                whatTheFlock.AgentDisplayedWidth = agentDisplayedWidth;
                whatTheFlock.Render();
            }
            else
            {
                whatTheFlock.Flock.BoundingBoxSize = boundingBoxSize;
                whatTheFlock.Flock.Timestep = timeStep;
                whatTheFlock.Flock.NeighborhoodRadius = neighborhoodRadius;
                whatTheFlock.Flock.AlignmentStrength = alignmentStrength;
                whatTheFlock.Flock.CohesionStrength = cohesionStrength;
                whatTheFlock.Flock.SeparationStrength = separationStrength;
                whatTheFlock.Flock.SeparationDistance = separationDistance;
                whatTheFlock.AgentDisplayedLength = agentDisplayedLength;
                whatTheFlock.AgentDisplayedWidth = agentDisplayedWidth;

                int n = sphereRepellers?.Count ?? 0;
                whatTheFlock.Flock.RepellerCenters = new List<Triple>(n);
                whatTheFlock.Flock.RepellerRadii = new List<float>(n);

                if (sphereRepellers != null)
                {
                    for (int i = 0; i < n; i++)
                    {
                        whatTheFlock.Flock.RepellerCenters.Add(sphereRepellers[i].CenterPoint.ToTriple());
                        whatTheFlock.Flock.RepellerRadii.Add((float)(sphereRepellers[i].Radius));
                    }
                }

                if (execute) whatTheFlock.StartBackgroundExecution();
                else
                {
                    whatTheFlock.StopBackgroundExecution();
                }
            }

            if (outputDynamoGeometries && !execute)
            {
                List<Point> agentPositions = new List<Point>();
                List<Vector> agentDirections = new List<Vector>();

                foreach (FlockAgent agent in whatTheFlock.Flock.Agents)
                {
                    agentPositions.Add(agent.Position.ToPoint());
                    agentDirections.Add(agent.Position.ToVector());
                }

                return new Dictionary<string, object>
                {
                    { "agentPositions", agentPositions },
                    { "agentDirections", agentDirections },
                };
            }

            else
            {
                return new Dictionary<string, object>
                {
                    { "agentPositions", null },
                    { "agentDirections", null },
                };
            }
        }
    }
}