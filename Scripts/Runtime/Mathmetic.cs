using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using theArkitectPackage.Enum;
using UnityEngine;
using Random = UnityEngine.Random;

namespace theArkitectPackage.Mathmetic
{
    public class Point
    {
        public float x;
        public float y;

        public static Vector2 ToVector2(Point p) => new Vector2(p.x, p.y);

        public Point(float _x, float _y)
        {
            x = _x;
            y = _y;
        }

        public Point(Vector2 v)
        {
            x = v.x;
            y = v.y;
        }

        public override string ToString()
        {
            return "(" + x.ToString("F3") + "," + y.ToString("F3") + ")";
        }
    }

    public class Circle //(x-a)^2+(y-b)^2-R^2=0;
    {
        public float a;
        public float b;
        public float r;

        public Point Center()
        {
            return new Point(a, b);
        }

        public Circle(Vector2Int center, float _r) : this(new Vector2(center.x, center.y), _r)
        {
        }

        public Circle(Vector2 center, float _r)
        {
            if (_r == 0)
            {
                throw new ArgumentException("Could not create circle for zero as radius");
            }

            a = center.x;
            b = center.y;
            r = _r;
        }

        public Circle(float _a, float _b, float _r)
        {
            if (_r == 0)
            {
                throw new ArgumentException("Could not create circle for zero as radius");
            }

            a = _a;
            b = _b;
            r = _r;
        }
    }

    public class Line //y=kx+b
    {
        public float k;
        public float b;
        public bool? VorH; //null=generic,true=vertical,false=horizontal

        public float SideIndex(Point A)
        {
            return SideIndex(new Vector2(A.x, A.y));
        }

        public float SideIndex(Vector2 A)
        {
            if (VorH.HasValue)
            {
                if (VorH.Value)
                {
                    return A.x - b;
                }

                return A.y - b;
            }

            return A.y - k * A.x - b;
        }

        public Line(float _k, float _b, bool? _vORh = null)
        {
            k = _k;
            b = _b;
            if (!_vORh.HasValue)
            {
                if (k == 0)
                {
                    VorH = false;
                }

                if (float.IsPositiveInfinity(k))
                {
                    VorH = true;
                }
            }
            else
            {
                VorH = _vORh;
            }
        }

        public Vector2 Intersect(Line other)
        {
            if (k == other.k)
            {
                throw new ArgumentException("Two line are parallel, no valid intersection");
            }

            if (VorH.HasValue && other.VorH.HasValue && VorH.Value == other.VorH.Value)
            {
                throw new ArgumentException("Two line are parallel, no valid intersection");
            }

            if (VorH.HasValue)
            {
                if (VorH.Value)
                {
                    if (other.VorH.HasValue)
                    {
                        return new Vector2(b, other.b);
                    }

                    return new Vector2(b, other.k * b + other.b);
                }

                if (other.VorH.HasValue)
                {
                    return new Vector2(other.b, b);
                }

                return new Vector2((b - other.b) / other.k, b);
            }

            //n=km+b
            //n=k1m+b1

            //km+b=k1m+b1

            var m = (other.b - b) / (k - other.k);
            var n = k * m + b;
            return new Vector2(m, n);
        }

        public Line(Vector2 A, Vector2 B) : this(new Point(A), new Point(B))
        {
        }

        public Line(Point A, Point B)
        {
            if (B.x == A.x && B.y == A.y)
            {
                throw new ArgumentException("Could not create Line for a single point");
            }

            if (B.x == A.x)
            {
                k = float.PositiveInfinity;
                b = B.x;
                VorH = true;
                return;
            }

            if (B.y == A.y)
            {
                VorH = false;
                k = 0;
                b = B.y;
                return;
            }

            k = (B.y - A.y) / (B.x - A.x);
            b = B.y - k * B.x;
        }

        public override string ToString()
        {
            return "y=" + k.ToString("F2") + "x+" + b.ToString("F2");
        }
    }

    public struct Matrix2x2
    {
        internal float A00;
        internal float A01;
        internal float A10;
        internal float A11;

        public Matrix2x2(float _A00, float _A01, float _A10, float _A11)
        {
            A00 = _A00;
            A01 = _A01;
            A10 = _A10;
            A11 = _A11;
        }

        public Matrix2x2(float[] content = null)
        {
            if (content == null)
            {
                A00 = 1;
                A01 = 0;
                A10 = 0;
                A11 = 1;
            }
            else
            {
                A00 = content[0];
                A01 = content[1];
                A10 = content[2];
                A11 = content[3];
            }
        }

        public static Matrix2x2 operator +(Matrix2x2 a, Matrix2x2 b)
        {
            var content =
                new[]
                {
                    b.A00 + a.A00, b.A01 + a.A01, b.A10 + a.A10, b.A11 + a.A11
                };
            return new Matrix2x2(content);
        }

        public static Matrix2x2 operator -(Matrix2x2 a, Matrix2x2 b)
        {
            var content =
                new[]
                {
                    a.A00 - b.A00, a.A01 - b.A01, a.A10 - b.A10, a.A11 - b.A11
                };
            return new Matrix2x2(content);
        }

        public static Matrix2x2 operator *(Matrix2x2 a, Matrix2x2 b)
        {
            Vector2 rowA0 = new Vector2(a.A00, a.A01);
            Vector2 rowA1 = new Vector2(a.A10, a.A11);
            Vector2 rowB0 = new Vector2(b.A00, b.A01);
            Vector2 rowB1 = new Vector2(b.A10, b.A11);

            var content =
                new[]
                {
                    Vector2.Dot(rowA0, rowB0),
                    Vector2.Dot(rowA0, rowB1),
                    Vector2.Dot(rowA1, rowB0),
                    Vector2.Dot(rowA1, rowB1),
                };
            return new Matrix2x2(content);
        }

        public static Vector2 operator *(Matrix2x2 a, Vector2 b)
        {
            Vector2 rowA0 = new Vector2(a.A00, a.A01);
            Vector2 rowA1 = new Vector2(a.A10, a.A11);

            return new Vector2(
                Vector2.Dot(rowA0, b),
                Vector2.Dot(rowA1, b));
        }

        public static Vector2 operator *(Vector2 a, Matrix2x2 b)
        {
            return b * a;
        }
    }

    public static class Arki_Math
    {
        /*/// TODO Digong
        /// <summary>
        /// 目的：提供一个棋盘上的坐标作为center以及半径，返回由坐标构成，像素化的圆。
        /// 并且随机选择一个圆上的位置。这个随机过程符合二维正态分布。
        /// 如果圆被棋盘边界阻挡，那么可能需要返回半圆或四分之一圆等残圆。
        /// </summary>
        /// <param name="center">输入的棋盘位置。</param>
        /// <param name="radius">像素圆的半径。</param>
        /// <param name="s_div">随机选择过程的标准差。</param>
        /// <param name="boardLength">棋盘宽度。</param>
        /// <param name="selected">所选结果在return中的index。</param>
        /// <returns>构成像素圆全部像素的坐标的Array。</returns>
        /// 生成圆形的pattern可以参考网页：https://donatstudios.com/PixelCircleGenerator
        ///     里面输入的Height/Width是直径，因为是像素化的圆，那里的直径是函数中的：radius*2+1.
        public static List<Vector2Int> PositionRandomization_NormalDistro(
            in Vector2Int center, in int radius,
            in float s_div, in int boardLength,
            out int selected)
        {
            //考虑想辙把possibility也传出来？
            if (radius == 0)
            {
                var res0 = new List<Vector2Int> {center};
                selected = 0;
                return res0;
            }

            var mask = PixelCircleMask.GenerateMask(radius);
            var len = 2 * radius + 1;
            var possibility = new Dictionary<int, float>();
            var res = new List<Vector2Int>();
            var sum = 0f;
            for (int i = 0; i < len; ++i)
            {
                for (int j = 0; j < len; ++j)
                {
                    if (mask[i][j] == 1)
                    {
                        int x = i - radius, y = j - radius;
                        var now = new Vector2Int(x + center.x, y + center.y);
                        if (IsInBoard(now, boardLength))
                        {
                            sum += (possibility[res.Count] = (float)TwoDimensionalGaussianDistribution(x, y, s_div));
                            res.Add(now);
                        }
                    }
                }
            }
            //normalize
            for (var i = 0; i < res.Count; ++i)
            {
                possibility[i] *= 1 / sum;
            }
            selected = GenerateWeightedRandom(possibility);
            return res;
        }*/

        public const double Epsilon = 1e-4;

        public const float Sqrt_5 = 2.23606797749f;
        public const float Inv_Sqrt_5 = 0.4472135955f;
        public const float Phi = 1.618033988749f;
        public const float Inv_Sqrt2Pi = 0.3989422804f;

        public static float GaussianDistribution(float x, float varianceSqrt)
        {
            var a = Inv_Sqrt2Pi / varianceSqrt;
            var b = Mathf.Pow(x / varianceSqrt, 2.0f) * (-0.5f);
            return a * Mathf.Exp(b);
        }

        /// <summary>
        /// using two dimensional gaussian distribution at point(x,y) as possibility of chunk (x,y)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="s_div"></param>
        /// <returns></returns>
        public static double TwoDimensionalGaussianDistribution(in int x, in int y, in float s_div)
        {
            double s_div2 = 1.0 * s_div * s_div;
            return (2 * Math.PI * s_div2) * Math.Exp(-0.5 * (x * x / s_div2 + y * y / s_div2));
        }

        public static float EaseInOutCubic(float x)
        {
            return x < 0.5f ? 4.0f * x * x * x : 1.0f - Mathf.Pow(-2.0f * x + 2.0f, 3.0f) / 2.0f;
        }

        public static int UnrollVector2Int(Vector2Int pos, int width)
        {
            return width * pos.x + pos.y;
        }

        /// <summary>
        /// 将拆分为min和max范围内的数字和。函数会将返回中的数值尽可能接近min和max的中间值。并且此函数的min、max均为包含。
        /// </summary>
        /// <param name="n">总数</param>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        /// <returns></returns>
        public static int[] DivideWithinRange(int n, int min, int max)
        {
            //21/4/8
            if (n <= min)
            {
                return new[] { n };
            }

            var maxDivider = n / (float)min; //5.25
            var minDivider = n / (float)max; //2.625
            var minDividerInt = Mathf.CeilToInt(minDivider); //3
            var maxDividerInt = Mathf.FloorToInt(maxDivider); //5
            var selectedDivider = Mathf.FloorToInt((minDividerInt + maxDividerInt) * 0.5f); //4
            return SpreadOutLaying(selectedDivider, n, out _);
        }

        /// <summary>
        /// 根据指定波动，生成特定随机数列，但是保证整个数组和为0。
        /// 这个函数的分布不是完全均匀的。±1的概率会高一些，±variation的概率会略低。
        /// 数组生成量较低的时候，这个概率变化是原始值的25%，但是会随着长度的增长而减低。
        /// </summary>
        /// <param name="length">长度</param>
        /// <param name="variation">指定波动</param>
        /// <returns>随机数列</returns>
        public static int[] SumZeroRandomArray(int length, int variation)
        {
            var res = new int[length];
            for (int i = 0; i < length; i++)
            {
                res[i] = Random.Range(-variation, variation + 1);
            }

            // return res;
            var sum = res.Sum();
            if (sum != 0)
            {
                var sumSign = Mathf.Sign(sum) > 0;
                var absSum = Mathf.Abs(sum);

                var spreadLength = res.Count(n => sumSign ? n >= 0 : n <= 0);
                var offset = SpreadOutLaying(spreadLength, absSum, out _);

                var offsetCount = 0;
                for (var i = 0; i < length; i++)
                {
                    if (sumSign ? res[i] >= 0 : res[i] <= 0)
                    {
                        res[i] += (sumSign ? -1 : 1) * offset[offsetCount];
                        offsetCount++;
                    }
                }

                // if (Mathf.Abs(sum) <= length)
                // {
                //     for (var i = 0; i < absSum; i++)
                //     {
                //         //如果sum数值小于等于总数，就将前sum个数量的值中都敲掉1就行了。
                //         res[i] -= sign ? 1 : -1;
                //     }
                // }
                // else
                // {
                //     var offset = SpreadOutLaying(length, absSum, out var sum1);
                //     for (var i = 0; i < length; i++)
                //     {
                //         //是减法，是因为需要和去掉sum的数量。
                //         res[i] -= (sign ? 1 : -1) * offset[i];
                //     }
                // }
                //
                // Debug.Assert(res.Sum() == 0, "sum not Zero!");
            }

            return res;
        }

        /// <summary>
        /// 将一个整数以特定数量切分，并且每个数值可以会有特定的随机波动。
        /// 并且可以设定一定的结果最大值，而且长度允许一定的随机性。
        /// </summary>
        /// <param name="length">目标计数，大于等于1</param>
        /// <param name="lengthVariation">目标计数可变化的绝对值</param>
        /// <param name="cap">切分后数据的最大值（含）</param>
        /// <param name="sum">总数，大于等于目标计数</param>
        /// <param name="variationRatio">随机波动的强度：0~1</param>
        /// <returns>将总数按照需求切分的结果</returns>
        public static int[] SpreadOutLayingWRandomizationWCapWLengthRandomization(int length, int lengthVariation,
            int cap, int sum, float variationRatio)
        {
            if ((length + lengthVariation) * cap < sum)
            {
                throw new ArgumentException("Division is not possible even maxed-out cap and length");
            }

            var minPossibleLengthA = Mathf.CeilToInt(sum / (float)cap);
            var minPossibleLength = Mathf.Max(minPossibleLengthA, length - lengthVariation);
            var maxPossibleLength = length + lengthVariation;

            var targetLength = Random.Range(minPossibleLength, maxPossibleLength + 1);
            return SpreadOutLayingWRandomizationWCap(targetLength, cap, sum, variationRatio);
        }

        /// <summary>
        /// 将一个整数以特定数量切分，并且每个数值可以会有特定的随机波动。
        /// 并且可以设定一定的结果最大值。
        /// </summary>
        /// <param name="length">目标计数，大于等于1</param>
        /// <param name="cap">切分后数据的最大值（含）</param>
        /// <param name="sum">总数，大于等于目标计数</param>
        /// <param name="variationRatio">随机波动的强度：0~1</param>
        /// <returns>将总数按照需求切分的结果</returns>
        public static int[] SpreadOutLayingWRandomizationWCap(int length, int cap, int sum, float variationRatio)
        {
            if (length * cap < sum)
            {
                throw new ArgumentException("Division is not possible even maxed-out cap");
            }

            var averagedList = SpreadOutLaying(length, sum, out _);
            var maxElement = averagedList.Select(Mathf.Abs).Max();
            var variation = Mathf.RoundToInt(maxElement * variationRatio);
            variation = Mathf.Min(variation, cap - averagedList[0]);
            if (averagedList[0] == cap || variation == 0)
            {
                //这里相当于12拆四个，并且最大值为三。就是不能允许任何随机变化了。
                return averagedList;
            }

            var offset = SumZeroRandomArray(length, variation);
            var res = new int[length];
            for (int i = 0; i < length; i++)
            {
                res[i] = averagedList[i] + offset[i];
            }

            Debug.Assert(res.Sum() == sum);
            return res;
        }

        /// <summary>
        /// 将一个整数以特定数量切分，并且每个数值可以会有特定的随机波动。
        /// </summary>
        /// <param name="length">目标计数，大于等于1</param>
        /// <param name="sum">总数，大于等于目标计数</param>
        /// <param name="variationRatio">随机波动的强度：0~1</param>
        /// <returns>将总数按照计数随机切分的结果</returns>
        public static int[] SpreadOutLayingWRandomization(int length, int sum, float variationRatio)
        {
            var averagedList = SpreadOutLaying(length, sum, out _);
            var minElement = averagedList.Select(Mathf.Abs).Min();
            var variation = Mathf.FloorToInt(minElement * variationRatio);
            var offset = SumZeroRandomArray(length, variation);
            var res = new int[length];
            for (int i = 0; i < length; i++)
            {
                res[i] = averagedList[i] + offset[i];
            }

            Debug.Assert(res.Sum() == sum);
            return res;
        }

        /// <summary>
        /// 将一个整数尽可能以目标计数以整数平均分解
        /// </summary>
        /// <param name="length">目标计数，大于等于1</param>
        /// <param name="sum">总数，大于等于目标计数</param>
        /// <param name="sumArray">将切分结果求和积分结果，最后一个数值就是总数</param>
        /// <returns>将总数按照计数切分的结果</returns>
        public static int[] SpreadOutLaying(int length, int sum, out int[] sumArray)
        {
            if (length == 0)
            {
                throw new DivideByZeroException("切分数不能为0!");
            }

            if (sum < length)
            {
                var res = new int[length];
                sumArray = new int[length];
                for (int i = 0; i < length; i++)
                {
                    if (i < sum)
                    {
                        res[i] = 1;
                        sumArray[i] = i + 1;
                    }
                    else
                    {
                        res[i] = 0;
                        sumArray[i] = sum;
                    }
                }

                return res;
            }

            sumArray = new int[length];
            var resDiv = new int[length];
            var baseInterval = sum / length;
            var residue = sum - (baseInterval * length);
            for (int i = 0; i < length; i++)
            {
                int interval = baseInterval;
                if (length - i <= residue)
                {
                    interval++;
                }

                resDiv[i] = interval;
            }

            for (var i = 0; i < sumArray.Length; i++)
            {
                for (var j = 0; j <= i; j++)
                {
                    sumArray[i] += resDiv[j];
                }
            }

            return resDiv;
        }

        public static int Fibonacci(int n) // find nth value in the fibonacci sequence
        {
            return Mathf.RoundToInt(Mathf.Pow(Phi, n) * Inv_Sqrt_5);
        }

        public static Vector2Int V2toV2Int(Vector2 a)
        {
            return new Vector2Int(Mathf.RoundToInt(a.x), Mathf.RoundToInt(a.y));
        }

        public static Vector2Int PermutateV2I(Vector2Int inVec, int maxEdgeIndex, PatternPermutation permutation)
        {
            Debug.Assert(maxEdgeIndex >= 1);
            if (maxEdgeIndex == 1)
            {
                return inVec;
            }

            Vector2 center = new Vector2(maxEdgeIndex / 2.0f, maxEdgeIndex / 2.0f);
            Vector2 normalizedIn = new Vector2(inVec.x, inVec.y) - center;
            Matrix2x2 rhs = new Matrix2x2();
            switch (permutation)
            {
                case PatternPermutation.None:
                    return inVec;
                case PatternPermutation.RotateR:
                    rhs = new Matrix2x2(0, 1, -1, 0);
                    break;
                case PatternPermutation.RotateL:
                    rhs = new Matrix2x2(0, -1, 1, 0);
                    break;
                case PatternPermutation.RotateH:
                    rhs = new Matrix2x2(-1, 0, 0, -1);
                    break;
                case PatternPermutation.FlipX:
                    rhs = new Matrix2x2(1, 0, 0, -1);
                    break;
                case PatternPermutation.FlipY:
                    rhs = new Matrix2x2(-1, 0, 0, 1);
                    break;
                case PatternPermutation.FlipXY:
                    rhs = new Matrix2x2(0, 1, 1, 0);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(permutation), permutation, null);
            }

            return V2toV2Int(normalizedIn * rhs + center);
        }

        public static Vector2Int FindNearestPosToRequiredPos(Vector2Int pos, IEnumerable<Vector2Int> pool,
            Func<Vector2Int, Vector2Int, float> tieBreaker = null)
        {
            if (!pool.Any())
            {
                throw new ArgumentException("pool should have element!!!");
            }

            if (pool.Contains(pos))
            {
                Debug.Log("pool contains pos, return pos directly");
                return pos;
            }

            if (pool.Count() == 1)
            {
                Debug.Log("pool contains only one pos,return this.");
                return pool.First();
            }

            if (tieBreaker == null)
            {
                tieBreaker = (a, b) => a.GetHashCode() - b.GetHashCode();
            }

            return pool.OrderBy(v => Vector2Int.Distance(v, pos)).ThenBy(v => tieBreaker(v, pos)).First();
        }

        public static Vector2Int Find8DirCardinalOfVector2(Vector2 dir)
        {
            var offsetDir = dir.normalized;
            var normalizedCardinalData =
                NumericData.V2Int8DirLib.Select(v => ((Vector2)v).normalized).ToList();
            var dotedCardinalProduct = normalizedCardinalData.Select(c => Vector2.Dot(c, offsetDir)).ToList();
            var offsetNearestIndex = dotedCardinalProduct.IndexOf(dotedCardinalProduct.Max());
            return NumericData.V2Int8DirLib.ElementAt(offsetNearestIndex);
        }

        public static List<Vector2Int> FindSingleIsland_Iter(Vector2Int crt,
            ref Dictionary<Vector2Int, bool> unitIslandPoseMap, bool _4or8)
        {
            //TODO 问题还真不小，主要是这里计算的时候，没有Wall相关的数据结构。如果需要考虑的话，数据注入是个问题。
            var res = new List<Vector2Int>();
            Debug.Assert(unitIslandPoseMap.ContainsKey(crt));
            unitIslandPoseMap[crt] = true;
            res.Add(crt);
            var dirLib = _4or8 ? NumericData.V2Int4DirLib : NumericData.V2Int8DirLib;
            foreach (var vector2Int in dirLib)
            {
                var ccrt = crt + vector2Int;
                if (unitIslandPoseMap.ContainsKey(ccrt) && !unitIslandPoseMap[ccrt])
                {
                    var localRes = FindSingleIsland_Iter(ccrt, ref unitIslandPoseMap, _4or8);
                    if (localRes.Count > 0)
                    {
                        res.AddRange(localRes.Select(p => p).ToList());
                    }
                }
            }

            return res;
        }

        public static Dictionary<Vector2Int, int> TotalSurroundingGridWConnectivity(IEnumerable<Vector2Int> src,
            bool _4or8, bool dist = false)
        {
            // var pendingExtraGrid = RootMath.TotalSurroundingGrid(src, _4or8);//获得IsLand为起始列表的全部直接周边单元格。
            // var surroundingCount = RootMath.GetSurroundingCount(pendingExtraGrid, src, _4or8);//获得每个周边单元格的链接数。
            var setForSignalZone = new HashSet<Vector2Int>();
            var res = new Dictionary<Vector2Int, int>();
            var dirLib = _4or8 ? NumericData.V2Int4DirLib : NumericData.V2Int8DirLib;
            foreach (var vector2Int in src)
            {
                foreach (var dir in dirLib)
                {
                    var offsetV2 = vector2Int + dir;
                    if (setForSignalZone.Contains(offsetV2) || src.Contains(offsetV2)) continue;
                    setForSignalZone.Add(offsetV2);
                    res.Add(offsetV2, GridTotalSurroundingCount(offsetV2, src, _4or8, dist));
                }
            }

            return res;
        }

        public static IEnumerable<Vector2Int> TotalSurroundingGrid(IEnumerable<Vector2Int> src, bool _4or8)
        {
            return TotalSurroundingGridWConnectivity(src, _4or8).Keys;
        }

        public static int GridTotalSurroundingCount(Vector2Int v, IEnumerable<Vector2Int> pool, bool _4or8,
            bool dist = false)
        {
            var dirLib = _4or8 ? NumericData.V2Int4DirLib : NumericData.V2Int8DirLib;
            var dirLib_cw = _4or8 ? NumericData.V2Int4DirLib_cw : NumericData.V2Int8DirLib_cw;
            var dirLib_dist = _4or8 ? NumericData.V2Int4DirLib_dist : NumericData.V2Int8DirLib_dist;

            if (!dist)
            {
                return dirLib.Count(o => pool.Contains(v + o));
            }

            var res = 0;
            var distIndex = 0;
            var firstConnection = false;
            foreach (var t in dirLib_cw)
            {
                if (pool.Contains(v + t))
                {
                    if (!firstConnection)
                    {
                        firstConnection = true;
                        distIndex = 0;
                    }

                    res += dirLib_dist[distIndex];
                }

                distIndex++;
            }

            return res;
        }

        public static Vector2Int PickClosestToCenter(IEnumerable<Vector2Int> poses, Vector2 _cp,
            Func<Vector2Int, int> TieBreaker)
        {
            if (!poses.Any())
            {
                Debug.LogError("its empty Collection");
                return Vector2Int.zero;
            }

            if (poses.Count() == 1)
            {
                return poses.First();
            }

            var orderedPoses = poses.OrderBy(v => D3Distance(v, _cp));
            var shortestD3Dist = D3Distance(orderedPoses.First(), _cp);
            var secondDist = D3Distance(orderedPoses.ElementAt(1), _cp) - shortestD3Dist;
            if (secondDist * 1e-3 >= Epsilon)
            {
                return orderedPoses.First();
            }

            var minGridDistList = orderedPoses.Where(v => (D3Distance(v, _cp) - shortestD3Dist) * 1e-3 < Epsilon);
            return minGridDistList.OrderBy(TieBreaker).First();
        }

        public static int D3Distance(Vector2Int v, Vector2 _cp)
        {
            var dist_f = Vector2.Distance(v, _cp);
            return Mathf.RoundToInt(dist_f * 1000); //保留小数点后三位，量化所较数据。
        }

        public static Vector2 ComputeCenterPosWeight(IEnumerable<(Vector2Int, float)> posWweight)
        {
            var WeightedSum = Vector2.zero;
            var sumWeight = 0.0f;
            foreach (var (vector2Int, item2) in posWweight)
            {
                WeightedSum += (Vector2)vector2Int * item2;
                sumWeight += item2;
            }

            return WeightedSum / sumWeight;
        }

        public static Line GetPerpendicularLine(Line l, Point A)
        {
            if (l.VorH.HasValue)
            {
                if (l.VorH.Value)
                {
                    return new Line(0, A.y, false);
                }

                return new Line(0, A.x, true);
            }

            Debug.Assert(l.k != 0, "source line should be marked as horizontal!");
            var new_k = -1 / l.k;
            Debug.Assert(!float.IsPositiveInfinity(new_k) && !float.IsNegativeInfinity(new_k),
                "source line should be marked as vertical!");
            var new_b = A.y - A.x * new_k;
            return new Line(new_k, new_b);
        }

        public static int SolveQuadraticEquation_RealOnly(float a, float b, float c, out float[] res)
        {
            if (a == 0)
            {
                if (b == 0)
                {
                    res = new[] { -c };
                    return 1;
                }

                res = new[] { -c / b };
                return 1;
            }

            var del = b * b - 4 * a * c;
            if (del < 0)
            {
                res = new float[0];
                return 0;
            }

            if (del == 0)
            {
                res = new[] { -b / (2 * a) };
                return 1;
            }

            res = new[] { (-b + Mathf.Sqrt(del)) / (2 * a), (-b - Mathf.Sqrt(del)) / (2 * a) };
            return 2;
        }

        public static int SolveLineIntersectCircle(Circle c, Line l, out Point[] res)
        {
            var A = c.a;
            var B = c.b;
            var R = c.r;

            //(x-a)^2+(y-b)^2-R^2=0;
            //(x-a)^2+(y-b)^2=R^2;
            if (l.VorH.HasValue)
            {
                if (l.VorH.Value)
                {
                    //(y-b)^2=R^2-(x-a)^2;
                    //x=x0;
                    var A0 = R * R - (l.b - A) * (l.b - A);
                    if (A0 < 0)
                    {
                        res = new Point[0];
                        return 0;
                    }

                    if (A0 == 0)
                    {
                        res = new[] { new Point(l.b, B) };
                        return 1;
                    }

                    var resy0 = Mathf.Sqrt(A0) + c.b;
                    var resy1 = -Mathf.Sqrt(A0) + c.b;

                    res = new[]
                    {
                        new Point(l.b, resy0),
                        new Point(l.b, resy1)
                    };
                    return 2;
                }
                else
                {
                    //(x-a)^2=R^2-(y-b)^2;
                    //y=y0;
                    var A0 = R * R - (l.b - B) * (l.b - B);
                    if (A0 < 0)
                    {
                        res = new Point[0];
                        return 0;
                    }

                    if (A0 == 0)
                    {
                        res = new[] { new Point(A, l.b) };
                        return 1;
                    }

                    var resx0 = Mathf.Sqrt(A0) + c.a;
                    var resx1 = -Mathf.Sqrt(A0) + c.a;

                    res = new[]
                    {
                        new Point(resx0, l.b),
                        new Point(resx1, l.b)
                    };
                    return 2;
                }
            }


            //(x-c_a)^2+(y-c_b)^2-c_R^2=0;
            //y=l_k*x+l_b;

            //(x-c_a)^2+(l_k*x+l_b-c_b)^2-c_R^2=0;

            //c_a=A,c_b=B,c_R=R,l_k=K,l_b=M;

            //[(x-A)^2]+[(K*x+M-B)^2]-R^2=0;

            //[x^2-2Ax+A^2]+[K^2*x^2+2*k*(M-B)*x+(M-B)^2]-R^2=0;

            //(K^2+1)*x^2+(-2A+2*k*(M-B))*x+(A^2+(M-B)^2-R^2)=0;

            var qa = l.k * l.k + 1;
            var qb = (-2 * c.a + 2 * l.k * (l.b - c.b));
            var qc = c.a * c.a + (l.b - c.b) * (l.b - c.b) - c.r * c.r;

            var count = SolveQuadraticEquation_RealOnly(qa, qb, qc, out var qres);
            res = new Point[0];
            switch (count)
            {
                case 1:
                    res = new[] { new Point(qres[0], qres[0] * l.k + l.b) };
                    break;
                case 2:
                    res = new[]
                    {
                        new Point(qres[0], qres[0] * l.k + l.b),
                        new Point(qres[1], qres[1] * l.k + l.b)
                    };
                    break;
            }

            return count;
        }

        public static Line[] GetOuterCoTangentOf2Circle(Circle A, Circle B)
        {
            return GetOuterCoTangentOf2Circle(A, B, out var resP);
        }

        public static Line[] GetOuterCoTangentOf2Circle(Circle A, Circle B, out Point[] resP)
        {
            //RISK 用应用上不可能，还是想着判断一下A，B不全等。
            var cA = A.Center();
            var cB = B.Center();
            var cA_cB = new Line(cA, cB);
            var pCA = GetPerpendicularLine(cA_cB, cA);
            var pCB = GetPerpendicularLine(cA_cB, cB);
            var countCPA = SolveLineIntersectCircle(A, pCA, out var resAp);
            var countCPB = SolveLineIntersectCircle(B, pCB, out var resBp);
            Debug.Assert(countCPA == 2 && countCPB == 2);
            var sideIndexA0 = cA_cB.SideIndex(resAp[0]); //圆的半径大于零，这两个数就不可能等于0.
            var sideIndexB0 = cA_cB.SideIndex(resBp[0]);
            Debug.Assert(sideIndexA0 != 0 && sideIndexB0 != 0);
            if (sideIndexA0 * sideIndexB0 > 0)
            {
                //resAp[0]和resBp[0]同侧
                resP = new[] { resAp[0], resBp[0], resAp[1], resBp[1] };
                return new[]
                {
                    new Line(resAp[0], resBp[0]),
                    new Line(resAp[1], resBp[1]),
                };
            }

            //resAp[0]和resBp[0]异侧
            resP = new[] { resAp[0], resBp[1], resAp[1], resBp[0] };
            return new[]
            {
                new Line(resAp[0], resBp[1]),
                new Line(resAp[1], resBp[0]),
            };
        }
    }

    public static class Arki_Random
    {
        /// <summary>
        /// 连续投n次“硬币”
        /// </summary>
        /// <param name="n">投硬币的次数</param>
        /// <param name="chance">硬币出Head的概率,(0.0~1.0)</param>
        /// <returns>Head=true,Tail=false</returns>
        public static bool[] ConsecutiveCoinToss(int n, float chance, out int headCount)
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
            var values = System.Enum.GetValues(typeof(T));
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