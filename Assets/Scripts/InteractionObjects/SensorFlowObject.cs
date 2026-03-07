using InteractionMenus.MenuElements;

namespace InteractionObjects {
    public class SensorFlowObject : InteractionObject {

        public float volumeFlow = 0f;

        private ParameterLabel<float> label;

        void Start() {
            AddMenuElement(new Button("Detail View", OpenDetailView));
            label = AddMenuElement(new ParameterLabel<float>("Volume Flow [m³/h]", volumeFlow, v => $"{v:N1}"));
        }

        public void SetVolumeFlow(float newValue) => label.Value.Set(newValue);
    }
}