using UnityEngine;
using UnityEngine.UIElements;

namespace InteractionMenus {
    /// <summary>
    /// A slider with an <b>extra</b> wide dragger hitbox for easier dragging.
    /// </summary>
    public class WideHitboxSliderInt : SliderInt {

        public WideHitboxSliderInt() {
            VisualElement hitbox = this.Q<VisualElement>("unity-dragger");
            hitbox.AddToClassList("MenuSliderDraggerHitbox");
            hitbox.AddManipulator(new SensitivityManipulator(this, 1.25f));

            VisualElement visibleDragger = new();
            visibleDragger.AddToClassList("unity-base-slider_dragger");
            visibleDragger.AddToClassList("MenuSliderDragger");
            hitbox.Add(visibleDragger);
        }

        private class SensitivityManipulator : PointerManipulator {

            private readonly SliderInt slider;
            private readonly float sensitivity;

            private bool active;
            private Vector2 startPointer;
            private int startValue;
            private int activePointerId = -1;

            public SensitivityManipulator(SliderInt slider, float sensitivity) {
                this.slider = slider;
                this.sensitivity = sensitivity;
                activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
            }

            protected override void RegisterCallbacksOnTarget() {
                target.RegisterCallback<PointerDownEvent>(OnPointerDown);
                target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
                target.RegisterCallback<PointerUpEvent>(OnPointerUp);
            }

            protected override void UnregisterCallbacksFromTarget() {
                target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
                target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
                target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
            }

            private void OnPointerDown(PointerDownEvent evt) {
                if (!CanStartManipulation(evt)) return;
                active = true;
                activePointerId = evt.pointerId;
                startPointer = evt.position;
                startValue = slider.value;
                target.CapturePointer(evt.pointerId);
                evt.StopPropagation();
            }

            private void OnPointerMove(PointerMoveEvent evt) {
                if (!active || evt.pointerId != activePointerId || !target.HasPointerCapture(evt.pointerId)) return;
                float delta = (evt.position.x - startPointer.x) * sensitivity;
                int newValue = Mathf.RoundToInt(startValue +
                                                delta * (slider.highValue - slider.lowValue) /
                                                slider.resolvedStyle.width);
                slider.value = Mathf.Clamp(newValue, slider.lowValue, slider.highValue);
                evt.StopPropagation();
            }

            private void OnPointerUp(PointerUpEvent evt) {
                if (!active || evt.pointerId != activePointerId || !target.HasPointerCapture(evt.pointerId)) return;
                active = false;
                target.ReleasePointer(evt.pointerId);
                activePointerId = -1;
                evt.StopPropagation();
            }
        }
    }
}