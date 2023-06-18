using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.DesignScript.Runtime;


namespace WhatTheFlock.ZeroTouch
{
    public static class WhatTheFlock
    {
        public static WhatTheFlockSystem Create()
        {
            return new WhatTheFlockSystem();
        }


        [MultiReturn("agentPositions", "agentDirections")]
        public static Dictionary<string, object> Execute(
            WhatTheFlockSystem whatTheFlock,
            [DefaultArgument("100")] int agentCount,
            [DefaultArgument("true")] bool is3D,
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
            [DefaultArgument("0.55")] float agentDisplayedLength,
            [DefaultArgument("0.35")] float agentDisplayedWidth
        )
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            bool enableFastDisplay = true;

            if (reset)
            {
                whatTheFlock.StopBackgroundExecution();
                whatTheFlock.Clear();
                whatTheFlock.InitizializeFlockAgents(agentCount, is3D, boundingBoxSize);
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

                if (execute) whatTheFlock.StartBackgroundExecution();
                else
                {
                    whatTheFlock.StopBackgroundExecution();
                    if (!enableFastDisplay)
                    {
                        whatTheFlock.ClearRender();
                        whatTheFlock.Iterate();
                    }
                }
            }

            return new Dictionary<string, object>
                {
                    { "agentPositions", null },
                    { "agentDirections", null },
                };
        }
    }
}