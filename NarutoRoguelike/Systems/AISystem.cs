using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using NarutoRoguelike.Components;
using NarutoRoguelike.Core;
using NarutoRoguelike.Entities;
using NarutoRoguelike.UI;
using NarutoRoguelike.World;

namespace NarutoRoguelike.Systems
{
    public static class AISystem
    {
        /// <summary>
        /// Updates one enemy's AI for this turn.
        /// Returns the (dx, dy) move intent — call site should validate and apply it.
        /// Returns (0,0) if the enemy attacks or stays put.
        /// </summary>
        public static (int dx, int dy) Update(
            Enemy      enemy,
            Player     player,
            Map        map,
            MessageLog log,
            Random     rng)
        {
            if (!enemy.IsAlive) return (0, 0);

            var ai = enemy.AI;

            int distToPlayer = Map.ChebyshevDist(enemy.GridX, enemy.GridY, player.GridX, player.GridY);
            bool playerVisible = map.IsVisible(enemy.GridX, enemy.GridY) || distToPlayer <= 2;

            // ── State transitions ─────────────────────────────────────────────────
            switch (ai.State)
            {
                case AIState.Idle:
                    if (playerVisible && distToPlayer <= ai.SightRange)
                    {
                        ai.State   = AIState.Chasing;
                        ai.LastKnownPlayerPos = player.GridPosition;
                        log.Add($"{enemy.Name} spots you!", Constants.AkatsukiRed);
                    }
                    break;

                case AIState.Alert:
                    if (playerVisible && distToPlayer <= ai.SightRange)
                    {
                        ai.State   = AIState.Chasing;
                        ai.LastKnownPlayerPos = player.GridPosition;
                    }
                    else if (--ai.AlertTurns <= 0)
                    {
                        ai.State = AIState.Idle;
                    }
                    break;

                case AIState.Chasing:
                    if (playerVisible)
                        ai.LastKnownPlayerPos = player.GridPosition;
                    else if (Map.ChebyshevDist(enemy.GridX, enemy.GridY,
                                               ai.LastKnownPlayerPos.X, ai.LastKnownPlayerPos.Y) <= 1)
                    {
                        ai.State      = AIState.Alert;
                        ai.AlertTurns = 3;
                    }
                    break;

                case AIState.Attacking:
                    ai.State = distToPlayer <= 1 ? AIState.Attacking : AIState.Chasing;
                    break;
            }

            // ── Action selection ──────────────────────────────────────────────────
            if (ai.State == AIState.Idle) return (0, 0);

            // Adjacent → attack
            if (distToPlayer <= 1 && player.IsAlive)
            {
                ai.State = AIState.Attacking;
                CombatSystem.Attack(enemy, player, log, rng);
                return (0, 0);
            }

            // Move toward last-known player position using BFS step
            Point target = playerVisible ? player.GridPosition : ai.LastKnownPlayerPos;
            return BfsStep(enemy.GridX, enemy.GridY, target.X, target.Y, map);
        }

        // ── BFS pathfinding (single step) ─────────────────────────────────────────

        private static (int dx, int dy) BfsStep(int fromX, int fromY, int toX, int toY, Map map)
        {
            if (fromX == toX && fromY == toY) return (0, 0);

            // BFS with a small depth cap to stay cheap
            const int MaxDepth = 20;
            var visited = new HashSet<(int, int)>();
            var queue   = new Queue<(int x, int y, int firstDx, int firstDy)>();

            foreach (var (dx, dy) in Directions)
            {
                int nx = fromX + dx, ny = fromY + dy;
                if (!map.IsWalkable(nx, ny)) continue;
                queue.Enqueue((nx, ny, dx, dy));
                visited.Add((nx, ny));
            }

            int depth = 0;
            while (queue.Count > 0 && depth < MaxDepth)
            {
                int batchSize = queue.Count;
                for (int b = 0; b < batchSize; b++)
                {
                    var (x, y, fdx, fdy) = queue.Dequeue();
                    if (x == toX && y == toY) return (fdx, fdy);

                    foreach (var (dx, dy) in Directions)
                    {
                        int nx = x + dx, ny = y + dy;
                        if (!map.IsWalkable(nx, ny) || visited.Contains((nx, ny))) continue;
                        visited.Add((nx, ny));
                        queue.Enqueue((nx, ny, fdx, fdy));
                    }
                }
                depth++;
            }

            // Fallback: step directly toward target
            int sdx = Math.Sign(toX - fromX);
            int sdy = Math.Sign(toY - fromY);
            if (map.IsWalkable(fromX + sdx, fromY + sdy)) return (sdx, sdy);
            if (map.IsWalkable(fromX + sdx, fromY))       return (sdx, 0);
            if (map.IsWalkable(fromX,       fromY + sdy)) return (0, sdy);
            return (0, 0);
        }

        private static readonly (int dx, int dy)[] Directions =
        {
            ( 0, -1), ( 0, 1), (-1, 0), ( 1, 0),
            (-1, -1), ( 1, -1), (-1, 1), ( 1, 1)
        };
    }
}
