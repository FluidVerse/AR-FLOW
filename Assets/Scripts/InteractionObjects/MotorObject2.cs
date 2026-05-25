using Audio;
using InteractionMenus.MenuElements;
using Models;
using Shaders;
using UnityEngine;

namespace InteractionObjects {
    public class MotorObject2 : InteractionObject {

        private const string pumpButtonTextOn = "Motor an";
        private const string pumpButtonTextOff = "Motor aus";
        private const string transparencyButtonTextOn = "Transparenz an";
        private const string transparencyButtonTextOff = "Transparenz aus";

        [SerializeField] private ModelAnimator modelAnimator;
        [SerializeField] private TransparencyController transparencyController;
        [SerializeField] private ContinuousAudioPlayer pumpRunningAudio;

        private Button pumpOnOffButton, transparencyOnOffButton;
        private Slider transparencySlider;

        private void Start() {
            AddMenuElement(new Button("Detailansicht", CustomDetailView));
            pumpOnOffButton = AddMenuElement(new Button(pumpButtonTextOn, ToggleAnimation));
            AddMenuElement(new Button("Pumpe zurücksetzen", ResetAnimation));
            AddMenuElement(new Slider("Geschwindigkeit", SetAnimationSpeed, (int)modelAnimator.AnimationSpeed));
            transparencyOnOffButton = AddMenuElement(new Button(transparencyButtonTextOn, ToggleTransparency));
            // basic slider with no special settings
            // transparencySlider =
            //     AddMenuElement(new Slider("Transparenz", SetAlpha, (int)(transparencyController.Alpha * 100f)));
            // slider with custom step = 10 and value formatter to show it as percentage
            transparencySlider = AddMenuElement(new Slider("Transparenz", SetAlpha,
                (int)(transparencyController.Alpha * 100f), step: 10, valueFormatter: value => $"{value}%"));
            transparencySlider.IsActive.Set(false);

            SetAudioPitch();
        }

        private void CustomDetailView() {
            Debug.Log("Etanorm pump detail view");
            cameraManager.ChangeToDetailView(gameObject);
        }

        private void ToggleAnimation() {
            modelAnimator.AnimationEnabled = !modelAnimator.AnimationEnabled;
            pumpOnOffButton.Text.Set(modelAnimator.AnimationEnabled ? pumpButtonTextOff : pumpButtonTextOn);
            pumpRunningAudio.Toggle();
            Debug.Log($"Etanorm pump animation active: {modelAnimator.AnimationEnabled}");
        }

        private void ResetAnimation() {
            modelAnimator.ResetAnimation();
            pumpRunningAudio.Reset();
            Debug.Log("Etanorm pump animation reset");
        }

        private void SetAnimationSpeed(int sliderValue) {
            modelAnimator.AnimationSpeed = sliderValue;
            SetAudioPitch();
            Debug.Log($"Etanorm pump animation speed: {modelAnimator.AnimationSpeed}");
        }

        private void ToggleTransparency() {
            transparencyController.ToggleMode();
            bool isOpaque = transparencyController.CurrentMode == TransparencyMode.Opaque;
            transparencyOnOffButton.Text.Set(isOpaque ? transparencyButtonTextOn : transparencyButtonTextOff);
            transparencySlider.IsActive.Set(!isOpaque);
            Debug.Log($"Etanorm pump transparency mode: {transparencyController.CurrentMode}");
        }

        private void SetAlpha(int sliderValue) {
            transparencyController.Alpha = sliderValue / 100f;
            Debug.Log($"Etanorm pump transparency alpha: {transparencyController.Alpha}");
        }

        private void SetAudioPitch() {
            pumpRunningAudio.Pitch = 1 + 0.15f * (modelAnimator.AnimationSpeed / 100f);
        }
    }
}