using System;
using System.Collections.Generic;
using System.Linq;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Serialization;

//这个是可以转到Arkitect的那个插件集里面的。
namespace theArkitectPackage.Mathmetic.Pathfinding_Generic
{
    public interface IPathfindHelper<T> where T : struct, IEquatable<T>
    {
        public T[] GetNeighbors(T current);
        public int GetStepCost(T current, T next);
        public int GetHeuristic(T current, T end);
    }

    public enum PathfindStatus
    {
        UNKNOWN = -1,
        FOUND = 0,
        NOPATH = 1,
    }

    public struct PathfinderNode<T> where T : struct, IEquatable<T>
    {
        //[position,cost]这里是Working位置包含终点距离的权重，这里是需要排序后读取的。等效于A*中常说的F(G+H)。
        // 等效于A*中常说的G。
        public int Cost;

        // parent
        public T Parent;
        public int StepCount; //在当前邻居限制的情况下，需要若干步才能到达的数量。

        public PathfinderNode(T parent = default, int cost = -1)
        {
            Parent = parent;
            Cost = cost;
            StepCount = 0;
        }
    }

    public class PathfinderMapData<T> : Dictionary<T, PathfinderNode<T>> where T : struct, IEquatable<T>
    {
        public bool CompletedIsland;
        public T Start { private set; get; }
        private const int MaxIterCount = 100000;

        public PathfindStatus TryTraceBack(T end, out Queue<T> res)
        {
            res = new Queue<T>();
            if (!ContainsKey(end))
                return CompletedIsland ? PathfindStatus.NOPATH : PathfindStatus.UNKNOWN;

            var loopCounter = 0;
            var currentTemp = end;
            do
            {
                res.Enqueue(currentTemp);
                currentTemp = this[currentTemp].Parent;
                if (loopCounter > MaxIterCount)
                {
                    Debug.Log("LOOP POPED");
                    return PathfindStatus.NOPATH;
                }

                loopCounter++;
            } while (!Start.Equals(currentTemp));

            res.Enqueue(Start);
            return PathfindStatus.FOUND;
        }

        public PathfinderMapData(T start)
        {
            CompletedIsland = false;
            Start = start;
            Add(start, new PathfinderNode<T>());
        }

        public List<(T, int)> GetFrontier(Func<T, T, int> heuristicFunc, T end)
        {
            // var frontierQueue = PriorityQueue<>();
            var frontier = this.Select(k => (k.Key, this[k.Key].Cost + heuristicFunc(k.Key, end))).ToList();
            frontier.Add((Start, 0)); //以防万一，把初始点再加回去，有些臃肿，但是先这样。
            return frontier.Distinct().ToList();
        }
    }

    public class Pathfinder<T,P> where T : struct, IEquatable<T> where P : IPathfindHelper<T>
    {
        private IPathfindHelper<T> _helper;
        private Pathfinder_Raw<T> _raw;
        
        public Pathfinder(P helper)
        {
            _helper = helper;
            _raw = new Pathfinder_Raw<T>();
        }
        
        public PathfindStatus Pathfinder_Core(T start, T end,out Queue<T> foundPath)
        {
            var refMP = new PathfinderMapData<T>(start);
            return _raw.Pathfinder_Core(start, end, out foundPath, ref refMP, _helper.GetNeighbors, _helper.GetStepCost, _helper.GetHeuristic);
        }
    }

    public class Pathfinder_Raw<T> where T : struct, IEquatable<T>
    {
        private const int MaxIterCount = 100000;
        private const int NoPassableThreshold = 1000;

        public PathfindStatus Pathfinder_Core(T start, T end,out Queue<T> foundPath
            , Func<T, T[]> neighborsFunc, Func<T, T, int> stepCostFunc, Func<T, T, int> heuristicFunc)
        {
            var refMP = new PathfinderMapData<T>(start);
            return Pathfinder_Core(start, end, out foundPath, ref refMP, neighborsFunc, stepCostFunc,heuristicFunc);
        }
        
        public PathfindStatus Pathfinder_Core(T start, T end, out Queue<T> foundPath,
            ref PathfinderMapData<T> cachedMapData, Func<T, T[]> neighborsFunc,
            Func<T, T, int> stepCostFunc, Func<T, T, int> heuristicFunc)
        {
            if (start.Equals(end))
            {
                //平凡解，没啥可说的。
                foundPath = new Queue<T>();
                foundPath.Enqueue(start);
                return PathfindStatus.FOUND;
            }

            cachedMapData ??= new PathfinderMapData<T>(start); //同时也是Visited的标记。
            var oldCacheMapDataCount = cachedMapData.Count;
            //先无所谓，就楞找一下。目前是不是Completed都可以了，因为新版存储的都必然是最短的。
            var cachedPathStatus = cachedMapData.TryTraceBack(end, out foundPath);

            if (cachedPathStatus != PathfindStatus.UNKNOWN)
                //目前这里的结果是，能找着就有，找不着就必然没有。//如果是UNKnown才继续向下寻找。
                return cachedPathStatus;

            //目前的利用Cache内容的方案是，所有的Cache值，加上H后，直接变成初始的frontier。
            var frontier = cachedMapData.GetFrontier(heuristicFunc, end);

            var loopCounter = 0;
            foundPath = new Queue<T>();
            do
            {
                loopCounter++;
                var currentPack = frontier.OrderBy(t => t.Item2).First(); //获取cost最低的前线点。
                frontier.Remove(currentPack);
                var current = currentPack.Item1;
                if (current.Equals(end)) break;

                bool localWalkableCheck(int currentCost, T currentVec, ref PathfinderMapData<T> cachedMapData2)
                {
                    //这个判断是没有visited过或者有更新（更小）的cost
                    return currentCost < NoPassableThreshold && (!cachedMapData2.ContainsKey(currentVec) ||
                                                                 currentCost < cachedMapData2[currentVec].Cost);
                }

                foreach (var currentNeib in neighborsFunc(current))
                {
                    var currentCost = cachedMapData[current].Cost + stepCostFunc(current, currentNeib);
                    if (!localWalkableCheck(currentCost, currentNeib, ref cachedMapData)) continue;

                    var newNode = new PathfinderNode<T>
                    {
                        Cost = currentCost,
                        Parent = current,
                        StepCount = cachedMapData[current].StepCount + 1,
                    };
                    cachedMapData[currentNeib] = newNode;
                    frontier.Add((currentNeib, currentCost + heuristicFunc(currentNeib, end)));
                }

                if (frontier.Count == 0)
                {
                    // Debug.Log("No Path found");
                    cachedMapData.CompletedIsland = true;
                    return PathfindStatus.NOPATH;
                }

                if (loopCounter > MaxIterCount)
                {
                    foreach (var vector2Int in frontier)
                        Debug.Log(vector2Int.Item1);

                    Debug.Log("LOOP POPED@loopCounter=" + loopCounter);
                    return PathfindStatus.UNKNOWN;
                }
            } while (frontier.Count != 0);

            // Debug.Log("LOOPED@loopCounter=" + loopCounter);
            cachedMapData.CompletedIsland |= (oldCacheMapDataCount == cachedMapData.Count);
            return cachedMapData.TryTraceBack(end, out foundPath); //这里应该是不会有问题，一定能找到路线。
        }
    }
}
