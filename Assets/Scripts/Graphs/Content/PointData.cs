using UnityEngine;

namespace Graphs.Content {
    /// <summary>
    /// Data structure that describes a point on the graph.
    /// </summary>
    public struct PointData {

        /// <summary>
        /// Name of the point, displayed in the point info box when selected.
        /// </summary>
        public readonly string name;

        /// <summary>
        /// Position of the point in graph space (not screen space).
        /// </summary>
        public readonly Vector2 position;

        /// <summary>
        /// Color of the point.
        /// </summary>
        public readonly Color color;

        /// <summary>
        /// Position that is displayed in the point info box.
        ///
        /// Is equal to the actual graph space <see cref="position"/> by default, but can be overridden.
        /// </summary>
        public readonly Vector2 displayedPosition;

        /// <param name="name">See <see cref="name"/></param>
        /// <param name="position">See <see cref="position"/></param>
        /// <param name="color">See <see cref="color"/></param>
        /// <param name="displayedPosition">See <see cref="displayedPosition"/></param>
        public PointData(string name, Vector2 position, Color color, Vector2? displayedPosition = null) {
            this.name = name;
            this.position = position;
            this.color = color;
            this.displayedPosition = displayedPosition ?? position;
        }
    }
}