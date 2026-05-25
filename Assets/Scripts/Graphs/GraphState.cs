using UnityEngine;

namespace Graphs {
    /// <summary>
    /// Represents the state of the graph background and content.
    /// </summary>
    [System.Serializable]
    public struct GraphState {
        
        /// <summary>
        /// Coordinate of the graph at its anchor point, i.e. in the top left corner of the graph background.
        /// </summary>
        public Vector2 anchor;

        /// <summary>
        /// Zoom level of the graph in the x/y direction, i.e. the number of px per graph unit in the x/y direction.
        /// </summary>
        public Vector2 zoomLevel;
    }
}