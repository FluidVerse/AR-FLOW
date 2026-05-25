using Audio;
using InteractionMenus.MenuElements;
using Quests;
using UnityEngine;
using static ActionLog.LogMessages;

namespace InteractionObjects {
    public class WashingMachineObject : InteractionObject {

        public int program = 0;

        [SerializeField] private ContinuousAudioPlayer machineRunningAudio;

        private const string labelMachineOff = "Machine off";
        private const string labelMachineOn = "Machine on";

        private SliderWithButton rotationalSpeedSlider;
        private Button motorOnOffButton;
        private Label onOffLabel;
        private ParameterLabel<int> programLabel;

        void Start() {
            AddMenuElement(new Button("Detail View", OpenDetailView));
            onOffLabel = AddMenuElement(new Label(labelMachineOff));
            programLabel = AddMenuElement(new ParameterLabel<int>("Program", 0, p => p == 0 ? "none" : p.ToString()));
            AddMenuElement(new Button("Start Prog. 1", StartProgram1));
            AddMenuElement(new Button("Start Prog. 2", StartProgram2));
            AddMenuElement(new Button("Stop Machine", StopMachine));
        }


        private void StartProgram1() {
            SetProgram(1);
            Debug.Log("Programm 1 wird gestartet!");
            machineRunningAudio.Play();
            log.Write(WashingMachineProgram1Started);
            questManager.SendInteraction(new QuestInteraction<object>(questObject,
                QuestInteractionTypes.StartProgram1));
            AddAction("StartProgram1");
        }

        private void StartProgram2() {
            SetProgram(2);
            Debug.Log("Programm 2 wird gestartet!");
            machineRunningAudio.Play();
            log.Write(WashingMachineProgram2Started);
            questManager.SendInteraction(new QuestInteraction<object>(questObject,
                QuestInteractionTypes.StartProgram2));
            AddAction("StartProgram2");
        }

        private void StopMachine() {
            SetProgram(0);
            Debug.Log("Waschmaschine wird gestoppt!");
            machineRunningAudio.Stop();
            AddAction("StopMachine");
            log.Write(WashingMachineStopped);
        }

        private void SetProgram(int newProgram) {
            program = newProgram;
            onOffLabel.Text.SetSilently(program == 0 ? labelMachineOff : labelMachineOn);
            programLabel.Value.Set(program);
        }
    }
}