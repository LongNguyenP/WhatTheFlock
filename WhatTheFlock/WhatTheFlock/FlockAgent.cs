using System;
using System.Collections.Generic;
using Autodesk.DesignScript.Runtime;


namespace WhatTheFlock
{
    [IsVisibleInDynamoLibrary(false)]
    public class FlockAgent
    {
        public Triple Position;
        public Triple Velocity;

        public Flock Flock;

        private Triple desiredVelocity;


        public FlockAgent(Triple position, Triple velocity)
        {
            Position = position;
            Velocity = velocity;
        }


        public void UpdateVelocityAndPosition()
        {
            Velocity = 0.97f * Velocity + 0.03f * desiredVelocity;

            if (Velocity.Length > 8f) Velocity *= 8f / Velocity.Length;
            else if (Velocity.Length < 4f) Velocity *= 4f / Velocity.Length;

            Position += Velocity * Flock.Timestep;
        }



        public void ComputeDesiredVelocity(List<FlockAgent> neighbors)
        {
            // First, reset the desired velocity to 0
            desiredVelocity = Triple.Zero;


            // ===============================================================================
            // Pull the agent back if it gets out of the bounding box
            // ===============================================================================

            float bbSize = 0.5f * Flock.BoundingBoxSize;

            if (Position.X < -bbSize) desiredVelocity.X += -bbSize - Position.X;
            else if (Position.X > bbSize) desiredVelocity.X += bbSize - Position.X;

            if (Position.Y < -bbSize) desiredVelocity.Y += -bbSize - Position.Y;
            else if (Position.Y > bbSize) desiredVelocity.Y += bbSize - Position.Y;

            if (Position.Z < 0) desiredVelocity.Z += -Position.Z;
            else if (Position.Z > 2f * bbSize) desiredVelocity.Z += 2f * bbSize - Position.Z;

            desiredVelocity += Velocity;

            // ===============================================================================
            // If there are no neighbors nearby, the agent will maintain its current velocity,
            // else it will perform the "alignment", "cohesion" and "separation" behaviors
            // ===============================================================================

            if (neighbors.Count == 0) desiredVelocity += Velocity; // maintain the current velocity
            else
            {
                // -------------------------------------------------------------------------------
                // "Alignment" behavior
                // -------------------------------------------------------------------------------

                Triple alignment = Triple.Zero;

                foreach (FlockAgent neighbor in neighbors)
                    alignment += neighbor.Velocity;

                // We divide by the number of neighbors to actually get their average velocity
                alignment /= neighbors.Count;

                desiredVelocity += Flock.AlignmentStrength * alignment;


                // -------------------------------------------------------------------------------
                // "Cohesion" behavior
                // -------------------------------------------------------------------------------

                Triple centre = Triple.Zero;

                foreach (FlockAgent neighbour in neighbors)
                    centre += neighbour.Position;

                // We divide by the number of neighbors to actually get their centre of mass
                centre /= neighbors.Count;

                Triple cohesion = centre - Position;

                desiredVelocity += Flock.CohesionStrength * cohesion;


                // -------------------------------------------------------------------------------
                // "Separation" behavior
                // -------------------------------------------------------------------------------

                Triple separation = Triple.Zero;

                foreach (FlockAgent neighbor in neighbors)
                {
                    Triple getAway = Position - neighbor.Position;

                    if (getAway.LengthSquared < Flock.SeparationDistance * Flock.SeparationDistance)
                    {
                        /* We scale the getAway vector by inverse of the distance to the neighbor to make
                           the getAway vector bigger as the agent gets closer to its neighbor */
                        separation += getAway / (getAway.LengthSquared);
                    }
                }

                desiredVelocity += Flock.SeparationStrength * separation;

            }

            // -------------------------------------------------------------------------------
            // Avoiding the repellers
            // -------------------------------------------------------------------------------

            for (int i = 0; i < Flock.RepellerCenters.Count; i++)
            {
                Triple repel = Position - Flock.RepellerCenters[i];
                float d = repel.Length;
                repel /= d;
                float f = (d - Flock.RepellerRadii[i]);
                if (f > 20f) continue;
                if (f < 0.01f) f = 0.01f;
                repel *= 60f / (float)Math.Pow(f, 1);
                desiredVelocity += repel;
            }
        }
    }
}

