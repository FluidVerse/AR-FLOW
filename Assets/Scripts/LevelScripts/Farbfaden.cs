using System.Collections.Generic;
using BCI;
using Graphs;
using Quests;
using Toolbar;
using UnityEngine;
using static Quests.QuestInteractionTypes;
using static Quests.QuestObject;

namespace LevelScripts {
    public class Farbfaden : MonoBehaviour {

        // Kritischer Reynolds-Wert für den Übergang laminar → turbulent (Re_krit ≈ 2300)
        // Kleiner Toleranzbereich erlaubt
        private const float RE_MIN = 2200f;
        private const float RE_MAX = 2400f;

        // Erwartete Antworten für Eingabefeld-Quests
        private const string QUESTION_ANSWER = "TODO_ANSWER";  // TODO: Korrekte Antwort für Quest 7
        private const string DIAMETER_ANSWER = "TODO_DIAMETER"; // TODO: Korrekte Antwort für Quest 8
        // =========================================

        private QuestManager questManager;
        private ToolbarManager toolbarManager;
        private QuestUIController questUIController;

        private GraphApi graphApi;
        private GraphInput graphInput;
        private MoodyDiagramController moodyDiagram;
        private ReynoldsFlowController flowController;

        // 4 Punkte für k/D-Werte auf dem Moody-Diagramm
        // graphPosition: kalibrierte Position im Graph-Space (0-1), geräteunabhängig
        // diagramPosition: (Re, f) für Anzeige
        private readonly List<(Vector2 graphPosition, Vector2 diagramPosition, string label)> diagramPoints = new() {
            (new Vector2(0.3594f, 0.7008f), new Vector2(2.48e5f, 0.0718f), "k/D = 0.05"),
            (new Vector2(0.3132f, 0.7989f), new Vector2(7.0e5f, 0.11f), "k/D = 0.1"),
            (new Vector2(0.3940f, 0.6402f), new Vector2(7.0e4f, 0.0568f), "k/D = 0.03"),
            (new Vector2(0.3304f, 0.7618f), new Vector2(4.52e5f, 0.0863f), "k/D = 0.08"),
        };

        private int currentDiagramPointIndex;
        private bool reynoldsQuestCompleted;
        private int currentInputQuestIndex; // 0 = Frage, 1 = Durchmesser

        private void Awake() {
            questManager = FindAnyObjectByType<QuestManager>();
            if (questManager == null) {
                Debug.LogError("QuestManager not found", this);
            }
            toolbarManager = FindAnyObjectByType<ToolbarManager>();
            if (toolbarManager == null) {
                Debug.LogError("ToolbarManager not found", this);
            }
            questUIController = FindAnyObjectByType<QuestUIController>();
            if (questUIController == null) {
                Debug.LogError("QuestUIController not found", this);
            }
            graphApi = FindAnyObjectByType<GraphApi>();
            if (graphApi == null) {
                Debug.LogError("GraphApi not found", this);
            }
            moodyDiagram = FindAnyObjectByType<MoodyDiagramController>();
            if (moodyDiagram == null) {
                Debug.LogError("MoodyDiagramController not found", this);
            }
            flowController = FindAnyObjectByType<ReynoldsFlowController>();
            if (flowController == null) {
                Debug.LogError("ReynoldsFlowController not found", this);
            }
            graphInput = FindAnyObjectByType<GraphInput>();
            if (graphInput == null) {
                Debug.LogError("GraphInput not found", this);
            } else {
                Debug.Log($"[Farbfaden] GraphInput found, current IsCheckingForInput={graphInput.IsCheckingForInput}");
            }
        }

        private void Start() {
            questManager.SetQuestLine(QuestLines.FarbfadenLevel);
            toolbarManager.ShowGraphButton();

            graphApi.Resize(0, 1, 0, 1, false);

            // Bestehenden Reynolds-Slider mit Quest verbinden
            if (flowController != null && flowController.reynoldsSlider != null) {
                flowController.reynoldsSlider.onValueChanged.AddListener(OnReynoldsSliderChanged);
            }

            // Auf Antwort-Eingaben aus dem Quest-Menü lauschen
            if (questUIController != null) {
                questUIController.onAnswerSubmitted.AddListener(OnAnswerSubmitted);
            }

            // Quest-Stage-Änderungen beobachten
            questManager.onQuestStageCompleted.AddListener(OnQuestStageCompleted);

            // Explizit die Graph-Input-Verbindung herstellen (robuster als Unity Events)
            if (graphApi != null) {
                graphApi.onEnableGraph.AddListener(OnGraphEnabled);
                Debug.Log("[Farbfaden] Added OnGraphEnabled listener to graphApi.onEnableGraph");
            } else {
                Debug.LogError("[Farbfaden] graphApi is null, cannot add OnGraphEnabled listener!");
            }
        }

        /// <summary>
        /// Called when the graph is enabled or disabled.
        /// Ensures GraphInput.IsCheckingForInput is set correctly.
        /// </summary>
        private void OnGraphEnabled(bool enabled) {
            Debug.Log($"[Farbfaden] OnGraphEnabled called with enabled={enabled}, graphInput={graphInput != null}");
            if (graphInput != null) {
                graphInput.IsCheckingForInput = enabled;
                Debug.Log($"[Farbfaden] Set GraphInput.IsCheckingForInput to {enabled}");
            }
        }

        private void OnDestroy() {
            // Event-Listener entfernen
            if (flowController != null && flowController.reynoldsSlider != null) {
                flowController.reynoldsSlider.onValueChanged.RemoveListener(OnReynoldsSliderChanged);
            }
            if (questUIController != null) {
                questUIController.onAnswerSubmitted.RemoveListener(OnAnswerSubmitted);
            }
            if (questManager != null) {
                questManager.onQuestStageCompleted.RemoveListener(OnQuestStageCompleted);
            }
            if (graphApi != null) {
                graphApi.onEnableGraph.RemoveListener(OnGraphEnabled);
            }
        }

        /// <summary>
        /// Called when the Reynolds slider value changes.
        /// Checks if Re is in the target range for Quest 1.
        /// </summary>
        private void OnReynoldsSliderChanged(float sliderValue) {
            if (reynoldsQuestCompleted) return;

            // Berechne die tatsächliche Re-Zahl aus dem Slider-Wert
            // (basierend auf ReynoldsFlowController.OnReynoldsChanged Logik)
            float u_vel = sliderValue * 20f;
            float rho = 50f;  // Default-Werte aus ReynoldsFlowController
            float eta = 0.01f;
            float Re = u_vel * rho * 0.036f / eta;

            if (Re >= RE_MIN && Re <= RE_MAX) {
                reynoldsQuestCompleted = true;
                questManager.SendInteraction(new QuestInteraction<float>(ReynoldsSlider, SetReynoldsNumber, Re));
            }
        }

        /// <summary>
        /// Called when a quest stage is completed.
        /// Sets up the next diagram point if we're in the diagram stages.
        /// </summary>
        private void OnQuestStageCompleted(QuestStage completedStage) {
            int currentStage = questManager.ActiveQuestLine.CurrentStageIndex;

            // Stages 1-4 sind die Diagramm-Punkt-Quests (Index 1-4, da Index 0 = Re einstellen)
            if (currentStage >= 1 && currentStage <= 4) {
                int pointIndex = currentStage - 1;
                if (pointIndex < diagramPoints.Count) {
                    var point = diagramPoints[pointIndex];
                    moodyDiagram.SetDesiredPoint(point.graphPosition, point.diagramPosition, point.label);
                }
            }
        }

        /// <summary>
        /// Callback for <see cref="MoodyDiagramController.onClickOnDiagram"/>
        /// </summary>
        /// <param name="clickedCorrectly"><c>true</c> if the user has clicked on the desired point</param>
        public void OnClickOnDiagram(bool clickedCorrectly) {
            if (!clickedCorrectly) {
                return;
            }

            questManager.SendInteraction(new QuestInteraction<object>(MoodyDiagram, ClickedCorrectly));
            currentDiagramPointIndex++;

            if (currentDiagramPointIndex < diagramPoints.Count) {
                var nextPoint = diagramPoints[currentDiagramPointIndex];
                moodyDiagram.SetDesiredPoint(nextPoint.graphPosition, nextPoint.diagramPosition, nextPoint.label);
            } else {
                moodyDiagram.SetDesiredPoint(null, null, "");
                moodyDiagram.ShowExplanationDiagram();
            }
        }

        /// <summary>
        /// Called when the user submits an answer in the quest menu input field.
        /// </summary>
        private void OnAnswerSubmitted(string answer) {
            int currentStage = questManager.ActiveQuestLine.CurrentStageIndex;

            // Stage 6 = Frage (Index 6)
            // Stage 7 = Durchmesser (Index 7)
            string expectedAnswer = currentStage == 6 ? QUESTION_ANSWER : DIAMETER_ANSWER;

            // Einfache String-Vergleich (kann später verfeinert werden)
            bool isCorrect = answer.Trim().ToLower() == expectedAnswer.Trim().ToLower();

            if (isCorrect) {
                questManager.SendInteraction(new QuestInteraction<string>(InputField, AnsweredCorrectly, answer));
            } else {
                Debug.Log($"Falsche Antwort: {answer} (erwartet: {expectedAnswer})");
            }
        }
    }
}
