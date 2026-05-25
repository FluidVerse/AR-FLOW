using InteractionMenus.MenuElements;

namespace InteractionObjects {
    public class SensorPressureObject : InteractionObject {

        public float pressure = 100000f;

        private ParameterLabel<float> label;

        void Start() {
            AddMenuElement(new Button("Detail View", OpenDetailView));
            label = AddMenuElement(new ParameterLabel<float>("Static Pressure [Pa]", pressure));
        }

        public void SetPressure(float newValue) => label.Value.Set(newValue);
    }
}