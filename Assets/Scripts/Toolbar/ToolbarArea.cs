namespace Toolbar {
    /// <summary>
    /// Collection of frequently used areas in the toolbar, i.e. usually a VisualElement that is used as a container
    /// for other elements.
    ///
    /// Note that these areas do not have to be disjoint, e.g. <see cref="Position"/> is a part of
    /// <see cref="LevelObjects"/>.
    /// </summary>
    public enum ToolbarArea {
        LevelObjects,
        FieldProperties,
        Position,
        Property1,
        Property2,
        CameraButtonContainer,
        FunctionBox,
        LevelInfoBox
    }
}