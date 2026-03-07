using System;
using UnityEngine;
using UnityEngine.UI;

namespace Controls {
    /// <summary>
    /// Wrapper for the Joystick component from the asset store with some useful functions and properties for easy use.
    /// </summary>
    public class JoystickWrapper : MonoBehaviour {

        [SerializeField] private Joystick stick;
        [SerializeField] private Image outlineImage;
        [SerializeField] private Sprite horizontalOutline;
        [SerializeField] private Sprite verticalOutline;
        [SerializeField] private Sprite bothAxesOutline;

        /// <summary>
        /// Horizontal input value of the joystick.
        /// </summary>
        public float Horizontal => stick.Horizontal;

        /// <summary>
        /// Vertical input value of the joystick.
        /// </summary>
        public float Vertical => stick.Vertical;
        
        /// <summary>
        /// Whether the joystick is currently in the neutral position, i.e. whether both the horizontal and vertical
        /// input values are approximately zero.
        /// </summary>
        public bool IsNeutral => Mathf.Approximately(stick.Horizontal, 0f) && Mathf.Approximately(stick.Vertical, 0f);

        /// <summary>
        /// Current axis mode of the joystick, i.e. whether it is set to use only the horizontal or vertical axis or
        /// both axes.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public Axis CurrentAxisMode => stick.AxisOptions switch {
            AxisOptions.Both => Axis.Both,
            AxisOptions.Horizontal => Axis.Horizontal,
            AxisOptions.Vertical => Axis.Vertical,
            _ => throw new ArgumentOutOfRangeException()
        };

        /// <summary>
        /// Sets the joystick to only use the specified axis or both axes.
        /// </summary>
        public void SetAxisMode(Axis axis) {
            switch (axis) {
                case Axis.Horizontal:
                    outlineImage.sprite = horizontalOutline;
                    stick.AxisOptions = AxisOptions.Horizontal;
                    break;
                case Axis.Vertical:
                    outlineImage.sprite = verticalOutline;
                    stick.AxisOptions = AxisOptions.Vertical;
                    break;
                case Axis.Both:
                    outlineImage.sprite = bothAxesOutline;
                    stick.AxisOptions = AxisOptions.Both;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
            }
        }

        public enum Axis {
            Horizontal,
            Vertical,
            Both
        }
    }
}