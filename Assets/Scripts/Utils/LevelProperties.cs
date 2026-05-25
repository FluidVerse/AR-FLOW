using UnityEngine;

namespace Utils {
    /// <summary>
    /// Represents the properties of a level in the game.
    ///
    /// Each level in this game needs to have an instance of this class in the <c>Resources/Levels</c> directory.
    /// </summary>
    [CreateAssetMenu(fileName = "LevelProperties", menuName = "AR Flow/LevelProperties")]
    public class LevelProperties : ScriptableObject {
    
        /// <summary>
        /// Name of the AR scene.
        /// </summary>
        private const string arSceneName = "ARScene";
    
        /// <summary>
        /// Whether this level is the AR level.
        /// </summary>
        public bool IsArLevel => sceneName == arSceneName;

        /// <summary>
        /// Unique ID of this level.
        ///
        /// Additionally, it represents the ascending sort order of this level in the level list. A level with a lower
        /// number shows up higher in the list.
        /// </summary>
        public int id = -1;
    
        /// <summary>
        /// Whether this level should be shown in the main menu.
        ///
        /// If <c>false</c>, this level cannot be played inside the game.
        /// </summary>
        public bool showInMenu = true;

        /// <summary>
        /// Display name of the level.
        /// </summary>
        public string displayName = "Testlevel";

        /// <summary>
        /// Hint text for the level, displayed in the top left corner.
        /// </summary>
        public string displayHint = "Level 1";

        /// <summary>
        /// Name of the scene to load when this level is played.
        /// </summary>
        public string sceneName = "";

        /// <summary>
        /// Key of the AR model to use in the AR scene for this level.
        ///
        /// Not used for non-AR levels, can be left empty in that case.
        /// </summary>
        public string modelKey = "";

        /// <summary>
        /// Preview image for this level.
        /// </summary>
        public Texture2D levelImage;
    }
}