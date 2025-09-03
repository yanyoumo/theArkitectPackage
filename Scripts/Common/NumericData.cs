using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace theArkitectPackage
{
    public static class NumericData
    {
        public static readonly int[] V2Int4DirLib_dist = { 1, 2, 3, 2 };
        public static readonly int[] V2Int8DirLib_dist = { 1, 2, 3, 4, 5, 4, 3, 2 };

        //N/S/W/E
        public static readonly Vector2Int[] V2Int4DirLib =
            { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        //N/S/W/E/NE/NW/SE/SW
        public static readonly Vector2Int[] V2Int8DirLib =
        {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
            Vector2Int.up + Vector2Int.right, Vector2Int.up + Vector2Int.left,
            Vector2Int.down + Vector2Int.right, Vector2Int.down + Vector2Int.left
        };

        //N/W/S/E
        public static readonly Vector2Int[] V2Int4DirLib_cw =
            { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };

        //N/NW/W/WS/S/SE/E/EN
        public static readonly Vector2Int[] V2Int8DirLib_cw =
        {
            Vector2Int.up, Vector2Int.up + Vector2Int.right,
            Vector2Int.right, Vector2Int.right + Vector2Int.down,
            Vector2Int.down, Vector2Int.down + Vector2Int.left,
            Vector2Int.left, Vector2Int.left + Vector2Int.up,
        };
    }
}