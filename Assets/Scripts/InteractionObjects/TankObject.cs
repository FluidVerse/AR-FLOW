using InteractionMenus.MenuElements;

namespace InteractionObjects {
    public class TankObject : InteractionObject {

        public float diameter = 0.08f;

        public float volume = 1000.0f;

        void Start() {
            AddMenuElement(new Button("Detailansicht", OpenDetailView));
            AddMenuElement(new ParameterLabel<float>("Volumen [l]", volume));
        }
    }
}