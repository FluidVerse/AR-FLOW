using Audio;
using InteractionMenus.MenuElements;
using Quests;
using UnityEngine;
using static ActionLog.LogMessages;
using static Quests.QuestInteractionTypes;

namespace InteractionObjects {
    public class MotorObject : InteractionObject {

        public int speed = 0;
        public bool isRunning = false; //

        [SerializeField] private ContinuousAudioPlayer pumpRunningAudio;

        private const string motorButtonTextOn = "Stop Motor";
        private const string motorButtonTextOff = "Start Motor";

        private SliderWithButton rotationalSpeedSlider;
        private Button motorOnOffButton;

        void Start() {
            AddMenuElement(new Button("Detail View", OpenDetailView));
            //AddMenuElement(new Button("Motor starten", () => StartMotor()));
            //AddMenuElement(new Button("Motor stoppen", () => StopMotor()));
            motorOnOffButton = AddMenuElement(new Button(motorButtonTextOff, ToggleMotor));
            //AddMenuElement(new Slider("Drehzahl [1/min]", SetSpeedMotor, speed,0,3000,100, labelDigits:LabelDigits.Four));
            rotationalSpeedSlider = AddMenuElement(new SliderWithButton(
                new Slider("Speed [1/min]", null, speed, 0, 3000, 250, false, labelDigits: LabelDigits.Four),
                new Button("Set"), SetSpeedMotor));

            // demonstration of reacting to interactions returned from quests
            questManager.onInteractionFromQuest.AddListener(interaction => {
                if (interaction.IsObjectAndType(questObject, SetMotorSpeed, out int value)) {
                    Debug.Log($"Interaction \"{interaction}\" forces setting motor speed to {value}");
                    speed = value;
                    rotationalSpeedSlider.Slider.DefaultValue.Set(value); // set and force menu recreation
                }
                if (interaction.IsObjectAndType(questObject, BlockMotorSpeed)) {
                    rotationalSpeedSlider.IsClickable.Set(false);
                }
                if (interaction.IsObjectAndType(questObject, UnblockMotorSpeed)) {
                    rotationalSpeedSlider.IsClickable.Set(true);
                }
            });
        }

        private void ToggleMotor() {
            if (!isRunning) {
                motorOnOffButton.Text.Set(motorButtonTextOn);
                pumpRunningAudio.Toggle();
                Debug.Log($"Motor an!");
                isRunning = true;
                log.Write(MotorStarted);
            } else {
                motorOnOffButton.Text.Set(motorButtonTextOff);
                pumpRunningAudio.Toggle();
                Debug.Log($"Motor aus!");
                isRunning = false;
                log.Write(MotorStopped);
            }
            questManager.SendInteraction(new QuestInteraction<object>(questObject,
                QuestInteractionTypes.StartMotor));
            AddAction("ToggleMotor");
            //modelAnimator.AnimationEnabled = !modelAnimator.AnimationEnabled;
        }

        public void StartMotor() {
            isRunning = true;
            Debug.Log("Motor wird gestartet!");
            SetAudioPitch();
            pumpRunningAudio.Play();
            AddAction("StartMotor");
            motorOnOffButton.Text.Set(motorButtonTextOn);
        }

        public void StopMotor() {
            isRunning = false;
            Debug.Log("Motor wird gestoppt!");
            pumpRunningAudio.Stop();
            motorOnOffButton.Text.Set(motorButtonTextOff);
        }

        public void SetSpeedMotor(int pSpeed) {
            speed = pSpeed;
            Debug.Log("Drehzahl = " + speed);
            SetAudioPitch();
            log.Write(MotorSpeedSet(speed));
            questManager.SendInteraction(new QuestInteraction<int>(questObject, SetMotorSpeed, speed));
            rotationalSpeedSlider.Slider.DefaultValue.Set(speed);
            AddAction("SetMotorSpeed");
        }

        private void SetAudioPitch() {
            pumpRunningAudio.Pitch = 1 + 0.5f * (speed / 3000f);
        }
    }
}