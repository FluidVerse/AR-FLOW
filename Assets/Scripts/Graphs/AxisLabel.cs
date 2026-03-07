using System.Globalization;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace Graphs {
    /// <summary>
    /// Represents a label with a number value on an axis of a graph.
    /// </summary>
    public class AxisLabel : MonoBehaviour {

        private static readonly CultureInfo cultureGerman = new("de-DE");
        private static readonly CultureInfo cultureEnglish = new("en-US");

        /// <summary>
        /// The text component of the label.
        /// </summary>
        [SerializeField] private TMP_Text text;

        /// <summary>
        /// The UI elements that make up the label (e.g. background, text).
        ///
        /// All elements in this array will be activated/deactivated together.
        /// </summary>
        [SerializeField] private GameObject[] uiElements;

        /// <summary>
        /// Orientation of the axis the label belongs to (X-axis = <c>Horizontal</c>, Y-axis = <c>Vertical</c>).
        /// </summary>
        [SerializeField] private Orientation orientation = Orientation.Horizontal;

        /// <summary>
        /// Estimated average size of a character in the label's font, used for layout calculations.
        /// </summary>
        [SerializeField] private Vector2 averageCharacterSize = new(20f, 30f);

        /// <summary>
        /// Whether the label is currently visible.
        /// </summary>
        public bool IsVisible { get; private set; }

        /// <summary>
        /// The real underlying value that the label represents.
        /// </summary>
        public double RealValue { get; private set; }

        /// <summary>
        /// The text currently displayed by the label.
        /// </summary>
        public string DisplayedText => text.text;

        /// <summary>
        /// Estimates the width of the current text inside the label with respect to its <see cref="orientation"/>.
        ///
        /// That means, its width for <see cref="Orientation.Horizontal"/> and its height for
        /// <see cref="Orientation.Vertical"/>. The returned value is in scaled canvas space, not the real pixel size.
        /// </summary>
        public float OrientedWidth => orientation == Orientation.Horizontal
            ? averageCharacterSize.x * DisplayedText.Length
            : averageCharacterSize.y;

        /// <summary>
        /// Shows/hides the label.
        /// </summary>
        /// <param name="value">Shows if <c>true</c>, otherwise hides</param>
        public void Show(bool value) {
            IsVisible = value;
            foreach (GameObject obj in uiElements) {
                obj.SetActive(value);
            }
        }

        /// <summary>
        /// Sets the position of the label on the canvas.
        /// </summary>
        /// <param name="newPosition">New position</param>
        public void SetPosition(Vector2 newPosition) {
            transform.position = newPosition;
        }

        /// <summary>
        /// Sets the value of the label and updates the displayed text accordingly.
        /// </summary>
        /// <param name="value">New value</param>
        /// <param name="useCommaSeparator">Whether to use a comma as decimal separator</param>
        /// <param name="useScientificNotation">Whether to display the value in scientific notation</param>
        public void SetValue(double value, bool useCommaSeparator, bool useScientificNotation) {
            RealValue = value;
            CultureInfo culture = useCommaSeparator ? cultureGerman : cultureEnglish;
            text.text = useScientificNotation
                ? ToScientificNotation(value)
                : value.ToString("0.####################", culture);
        }

        /// <summary>
        /// Formats a number in scientific notation, removing unnecessary trailing zeros in the decimal part and
        /// leading zeros in the exponent.
        /// </summary>
        /// <param name="number">Number to format</param>
        /// <returns>Formatted number in scientific notation, e.g. <c>1.0E+5</c></returns>
        private static string ToScientificNotation(double number) {
            // scary ai-generated regex to format scientific notation nicely
            string raw = number.ToString("E10", cultureGerman);
            // 1. remove trailing zeros in the decimal part (e.g. 1,1000000000E+05 → 1,1E+05)
            raw = Regex.Replace(raw, @"(,\d*?[1-9])0+(E[+-]?\d+)", "$1$2");
            // 2. remove ',0' if decimal is zero (e.g. 1,0000000000E+05 → 1E+05)
            raw = Regex.Replace(raw, @",0+(E[+-]?\d+)", "$1");
            // 3. remove leading zeros in the exponent (e.g. E+005 → E+5)
            return Regex.Replace(raw, @"E([+-])0*(\d+)", "E$1$2");
        }
    }
}