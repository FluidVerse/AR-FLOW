using Graphs;
using Graphs.Content;
using Toolbar;
using UnityEngine;

namespace LevelScripts {
    public class TestLevel : MonoBehaviour {

        private ToolbarManager toolbarManager;
        private GraphApi graphApi;

        private void Awake() {
            toolbarManager = FindAnyObjectByType<ToolbarManager>();
            if (toolbarManager == null) {
                Debug.LogError("ToolbarManager not found", this);
            }
            graphApi = FindAnyObjectByType<GraphApi>();
            if (graphApi == null) {
                Debug.LogError("GraphApi not found", this);
            }
        }

        /// <summary>
        /// Test setup: show the graph button in the toolbar and add some test points to the graph.
        /// </summary>
        private void Start() {
            toolbarManager.ShowGraphButton();
            graphApi.Resize(-10, 110, -10, 110, true);
            graphApi.AddPoint(new PointData("Test1", new Vector2(0, 0), Color.red));
            graphApi.AddPoint(new PointData("Test2", new Vector2(50, 50), Color.green));
            graphApi.AddPoint(new PointData("Test3", new Vector2(100, 100), Color.blue));
            graphApi.AddPoint(new PointData("Test4", new Vector2(100, -10), Color.yellow));
        }
    }
}