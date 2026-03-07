using UnityEngine;

namespace Graphs {
    /// <summary>
    /// Stores temporary data used during recalculation of the graph background and content.
    /// </summary>
    public struct RecalculationData {

        /// <summary>
        /// Exponents of the current zoom levels in the x and y direction, i.e. the power of 10 that is closest to the
        /// current zoom level.
        /// </summary>
        public Vector2Int exps;

        /// <summary>
        /// Units per axis line in the x and y direction, i.e. the distance between two axis lines in graph units.
        /// </summary>
        public Vector2 axisUnits;

        /// <summary>
        /// Distance between two axis lines in the x and y direction, i.e. the distance between two axis lines in
        /// pixels.
        /// </summary>
        public Vector2 lineDistancePx;

        /// <summary>
        /// Coordinates of the first grid line that is visible in the graph background (with a top left anchor).
        /// </summary>
        public Vector2 firstOnGrid;

        /// <summary>
        /// Offsets of the graph background in the x and y direction in pixels (with a top left anchor).
        /// </summary>
        public Vector2 offsets;

        /// <summary>
        /// Positions of the axes.
        ///
        /// <c>x</c> is the y-position of the x-axis, <c>y</c> is the x-position of the y-axis.
        /// </summary>
        public Vector2 axisPositions;
    }
}