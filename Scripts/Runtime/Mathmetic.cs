    using System;
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;
    using UnityEngine;
    using Random = UnityEngine.Random;

    namespace theArkitectPackage.Mathmetic
    {
        public static class Arki_Random
        {
            /// <summary>
            /// 连续投n次“硬币”
            /// </summary>
            /// <param name="n">投硬币的次数</param>
            /// <param name="chance">硬币出Head的概率</param>
            /// <returns>Head=true,Tail=false</returns>
            public static bool[] ConsecutiveCoinToss(int n,float chance,out int headCount)
            {
                var res = new bool[n];
                for (var i = 0; i < n; i++)
                {
                    if (Random.value < chance) res[i] = true;
                }
                headCount = res.Count(b => b);
                return res;
            }

            public static IList<T> Shuffle<T>(this IList<T> array)
            {
                int n = array.Count();
                while (n > 1)
                {
                    int k = (int)(UnityEngine.Random.value * (n--));
                    (array[n], array[k]) = (array[k], array[n]);
                }

                return array;
            }

            /// <summary>
            /// 这个函数的Shuffle只满足Index-wise的Derangement，其内容的相等性不会被验证。
            /// </summary>
            /// <param name="array"></param>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            public static IList<T> DerangementShuffle<T>(this IList<T> array)
            {
                var indexArray = Enumerable.Range(0, array.Count);
                var shuffledArray = indexArray.ToList().Derangement().ToList();
                var res = array.ToList();
                for (var i = 0; i < array.Count; i++)
                {
                    res[i] = array[shuffledArray[i]];
                }

                return res;
            }

            private const int DerangementIterMax = 6;

            public static IList<T> Derangement<T>(this IList<T> array)
            {
                if (array.Distinct().Count() != array.Count)
                {
                    throw new ArgumentException("This derangement only works on distinct array!!");
                }

                bool CheckDerangement(IList<T> listA, IList<T> listB)
                {
                    if (listA.Count != listB.Count)
                    {
                        throw new ArgumentException("Length A should equal to B");
                    }

                    return !listA.Where((t, i) => t.Equals(listB[i])).Any();
                }

                var res = array.ToList();
                var iterCounter = -1;
                var foundDerangement = false;
                do
                {
                    res = res.Shuffle().ToList();
                    iterCounter++;
                    foundDerangement = CheckDerangement(array, res);
                } while (foundDerangement && iterCounter < DerangementIterMax);

                if (!foundDerangement)
                {
                    var bDerIndex = GenerateBernoulliDerangement(array.Count);
                    //Index add method.
                    for (var i = 0; i < array.Count; i++)
                    {
                        res[i] = array[bDerIndex[i]];
                    }
                }

                return res;
            }

            public static T GetRandomEnum<T>()
            {
                var values = Enum.GetValues(typeof(T));
                var random = new System.Random();
                var randomBar = (T)values.GetValue(random.Next(values.Length));
                return randomBar;
            }

            public static float LastRandom = 0.0f;

            public static IList<int> GenerateBernoulliDerangement(int n)
            {
                var res = Enumerable.Range(0, n).ToList();

                void SwapValByIndex(int indexA, int indexB)
                {
                    (res[indexA], res[indexB]) = (res[indexB], res[indexA]);
                }

                for (var i = 0; i < res.Count; i++)
                {
                    if (i != res.Count - 1)
                    {
                        SwapValByIndex(i, UnityEngine.Random.Range(i + 1, res.Count));
                    }
                    else
                    {
                        //Last one
                        if (i == res[i])
                        {
                            SwapValByIndex(i, UnityEngine.Random.Range(0, i));
                        }
                    }
                }

                return res;
            }

            public static IEnumerable<T> GenerateRandomItems<T>(IEnumerable<T> pool, IEnumerable<T> src,
                bool allowSame = false, bool forceDerangement = false)
            {
                if (!forceDerangement)
                {
                    return GenerateRandomItems(pool, src.Count(), allowSame);
                }

                var res = new List<T>();
                foreach (var x1 in src)
                {
                    var rawIndicesPool = pool.ToList(); //现在是每次抽取都把poolReset为初始值。
                    rawIndicesPool.Remove(x1); //因为已经forceDerangement了，所以把当前值移除。
                    if (!allowSame && res.Count > 0)
                    {
                        //如果不允许重复，那么每次都还要把已有的结果的内容从池子里去掉。
                        rawIndicesPool = rawIndicesPool.Except(res).ToList();
                    }

                    //然后再随机抽取。
                    var currentIndicesIndex = UnityEngine.Random.Range(0, rawIndicesPool.Count);
                    res.Add(rawIndicesPool[currentIndicesIndex]);
                    // if (!allowSame)
                    // {
                    //     rawIndicesPool.RemoveAt(currentIndicesIndex);
                    // }
                    //不要再往回加，而是每次随机抽选的时候，都重新挑出来。
                    // rawIndicesPool.Add(x1);//在第0步，把A元素选出来后，因为AllowSame被移除后，如果x1恰好处于src里面，有可能会因为下面的add(x1)把A元素又加回去。
                }

                return res;
            }

            /// <summary>
            /// 生成N个不同的随机元素。
            /// </summary>
            /// <param name="pool"></param>
            /// <param name="n"></param>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            public static IEnumerable<T> GenerateRandomItems<T>(IEnumerable<T> pool, int n, bool allowSame = false)
            {
                var rawIndicesPool = pool.ToList();
                var res = new List<T>();

                if (pool.Count() < n)
                {
                    throw new ArgumentException("n count should not large than pool counts");
                }

                for (var i = 0; i < n; i++)
                {
                    var currentIndicesIndex = UnityEngine.Random.Range(0, rawIndicesPool.Count);
                    res.Add(rawIndicesPool[currentIndicesIndex]);
                    if (!allowSame)
                    {
                        rawIndicesPool.RemoveAt(currentIndicesIndex);
                    }
                }

                return res;
            }

            public static T GenerateRandomItem<T>(IEnumerable<T> lib)
            {
                Dictionary<T, float> _lib = new Dictionary<T, float>();
                Debug.Assert(lib.Any());
                foreach (var type in lib)
                {
                    _lib.Add(type, 1.00f / lib.Count());
                }

                return GenerateWeightedRandom(_lib);
            }

            [CanBeNull]
            public static T GenerateWeightedRandom<T>(Dictionary<T, float> lib)
            {
                //有这个东西啊，不要小看他，这个很牛逼的；各种分布都可以有的。
                float totalWeight = 0;
                foreach (var weight in lib.Values)
                {
                    totalWeight += weight;
                }

                Debug.Assert((Mathf.Abs(totalWeight - 1) < 1e-3) && (lib.Count > 0),
                    "totalWeight=" + totalWeight + "||lib.Count=" + lib.Count);
                var val = UnityEngine.Random.value;
                LastRandom = val;
                totalWeight = 0;
                foreach (var keyValuePair in lib)
                {
                    totalWeight += keyValuePair.Value;
                    if (val <= totalWeight)
                    {
                        return keyValuePair.Key;
                    }
                }

                return default;
            }

        }
    }