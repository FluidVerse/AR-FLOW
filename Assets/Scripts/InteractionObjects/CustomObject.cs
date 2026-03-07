using InteractionMenus.MenuElements;

namespace InteractionObjects {
    public class CustomObject : InteractionObject {
        void Start() {
            AddMenuElement(new Button("Detail View", OpenDetailView));
        }
    }
}