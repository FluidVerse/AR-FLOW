using Graphs;
using Graphs.Content;
using UnityEditor;
using UnityEngine;

namespace Editor.Scripts {
    /// <summary>
    /// Custom inspector for the <see cref="Graphs.GraphApi"/> script.
    /// </summary>
    [CustomEditor(typeof(GraphApi))]
    public class GraphApiEditor : UnityEditor.Editor {

        private float xMin = -10f;
        private float xMax = 10f;
        private float yMin = -10f;
        private float yMax = 10f;
        private bool fitToEqualZoomLevel = true;

        private float valueRangeMin = GraphController.DefaultValueRange.x;
        private float valueRangeMax = GraphController.DefaultValueRange.y;

        private string xAxisUnit = "m²";
        private string yAxisUnit = "m³";

        private string pointName = "TestPoint";
        private float pointX;
        private float pointY;
        private Color pointColor = Color.red;

        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            GraphApi api = (GraphApi)target;
            if (!Application.isPlaying) {
                EditorGUILayout.HelpBox("This script provides additional inspector controls in play mode.",
                    MessageType.Info);
                return;
            }

            GUILayout.Label("Test controls", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            xMin = EditorGUILayout.FloatField("xMin", xMin);
            xMax = EditorGUILayout.FloatField("xMax", xMax);
            yMin = EditorGUILayout.FloatField("yMin", yMin);
            yMax = EditorGUILayout.FloatField("yMax", yMax);
            fitToEqualZoomLevel = EditorGUILayout.Toggle("Fit to equal zoom level", fitToEqualZoomLevel);
            if (GUILayout.Button("Resize")) {
                api.Resize(xMin, xMax, yMin, yMax, fitToEqualZoomLevel, false);
            }
            if (GUILayout.Button("Set default size")) {
                api.SetDefaultSize(xMin, xMax, yMin, yMax, fitToEqualZoomLevel);
            }
            if (GUILayout.Button("Resize to default")) {
                api.ResizeToDefault();
            }
            EditorGUILayout.Space();

            valueRangeMin = EditorGUILayout.FloatField("valueRangeMin", valueRangeMin);
            valueRangeMax = EditorGUILayout.FloatField("valueRangeMax", valueRangeMax);
            if (GUILayout.Button("Set value range")) {
                api.SetValueRange(new Vector2(valueRangeMin, valueRangeMax));
            }
            if (GUILayout.Button("Reset value range to default")) {
                api.SetValueRange(null);
            }
            EditorGUILayout.Space();

            xAxisUnit = EditorGUILayout.TextField("X axis unit", xAxisUnit);
            yAxisUnit = EditorGUILayout.TextField("Y axis unit", yAxisUnit);
            if (GUILayout.Button("Set axis unit labels")) {
                api.SetAxisUnitLabels(xAxisUnit, yAxisUnit);
            }
            if (GUILayout.Button("Hide axis unit labels")) {
                api.HideAxisUnitLabels();
            }
            EditorGUILayout.Space();

            pointName = EditorGUILayout.TextField("Point name", pointName);
            pointX = EditorGUILayout.FloatField("Point X", pointX);
            pointY = EditorGUILayout.FloatField("Point Y", pointY);
            pointColor = EditorGUILayout.ColorField("Point color", pointColor);
            if (GUILayout.Button("Add point")) {
                api.AddPoint(new PointData(pointName, new Vector2(pointX, pointY), pointColor));
            }
            if (GUILayout.Button("Clear points")) {
                api.ClearPoints();
            }
            EditorGUILayout.Space();

            if (GUILayout.Button("Enable graph")) {
                api.EnableGraph();
            }
            if (GUILayout.Button("Disable graph")) {
                api.DisableGraph();
            }
        }
    }
}