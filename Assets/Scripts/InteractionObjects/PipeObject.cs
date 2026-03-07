using InteractionMenus.MenuElements;

namespace InteractionObjects {
    public class PipeObject : InteractionObject {

        public float diameter = 0.08f;

        public float length = 2.0f;

        void Start() {
            AddMenuElement(new Button("Detail View", OpenDetailView));
        }

    }
}