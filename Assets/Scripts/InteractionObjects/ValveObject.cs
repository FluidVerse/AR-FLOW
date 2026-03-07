using InteractionMenus.MenuElements;
using Models;
using Quests;
using UnityEngine;
using static ActionLog.LogMessages;
using static Quests.QuestInteractionTypes;
using static Quests.QuestObject;

namespace InteractionObjects {
    public class ValveObject : InteractionObject {

        [SerializeField] private AudioSource setValveAudio;
        [SerializeField] private SingleMotionAnimator setValveAnimator;

        public int valvePosition = 0;

        public int minValue = 0;
        public int maxValue = 100;
        public int step = 1;

        private SliderWithButton valvePositionSlider;

        void Start() {
            AddMenuElement(new Button("Detail View", OpenDetailView));
            valvePositionSlider = AddMenuElement(new SliderWithButton(
                new Slider("Valve Position [%]", null, valvePosition, minValue, maxValue, step,
                    labelDigits: LabelDigits.Three),
                new Button("Set"), SetPosition));

            // demonstration of reacting to interactions returned from quests
            questManager.onInteractionFromQuest.AddListener(interaction => {
                if (interaction.IsObjectAndType(Kugelhahn1, SetValvePosition, out int value)) {
                    Debug.Log($"Interaction \"{interaction}\" forces setting valve position to {value}");
                    valvePosition = value;
                    valvePositionSlider.Slider.DefaultValue.Set(value); // set and force menu recreation
                }
            });
        }

        void SetPosition(int pValvePosition) {
            valvePosition = pValvePosition;
            log.Write(ValvePositionSet(valvePosition));
            questManager.SendInteraction(new QuestInteraction<int>(questObject, SetValvePosition, valvePosition));
            setValveAudio.Play();
            setValveAnimator.AnimateOnce();
            AddAction("SetPosition");
            Debug.Log("Ventilstellung = " + valvePosition);
        }
    }
}