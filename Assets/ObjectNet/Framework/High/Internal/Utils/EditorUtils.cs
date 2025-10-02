using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.onlineobject.objectnet {
#if UNITY_EDITOR
    /// <summary>
    /// Class BackgroundStyle.
    /// </summary>
    public static class BackgroundStyle {
        /// <summary>
        /// The style
        /// </summary>
        private static GUIStyle style = new GUIStyle();
        /// <summary>
        /// The texture
        /// </summary>
        private static Texture2D texture = new Texture2D(1, 1);
        /// <summary>
        /// Gets the specified color.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <returns>GUIStyle.</returns>
        public static GUIStyle Get(Color color) {
            if (texture == null) {
                texture = new Texture2D(1, 1);
            }
            texture.SetPixel(0, 0, color);
            texture.Apply();
            style.normal.background = texture;
            return style;
        }
    }

    /// <summary>
    /// Class EditorUtils.
    /// </summary>
    public static class EditorUtils {
        /// <summary>
        /// The default icon spacing
        /// </summary>
        public const float DEFAULT_ICON_SPACING = 10;

        /// <summary>
        /// The default explanation font size
        /// </summary>
        public const int DEFAULT_EXPLANATION_FONT_SIZE = 13;

        /// <summary>
        /// The default image button font size
        /// </summary>
        public const int DEFAULT_IMAGE_BUTTON_FONT_SIZE = 16;

        /// <summary>
        /// The default slider font size
        /// </summary>
        public const int DEFAULT_SLIDER_FONT_SIZE = 13;

        /// <summary>
        /// The image button color
        /// </summary>
        public static Color IMAGE_BUTTON_COLOR = new Color(51f / 255f, 51f / 255f, 51f / 255f);

        /// <summary>
        /// The image button font color
        /// </summary>
        public static Color IMAGE_BUTTON_FONT_COLOR = Color.white;

        /// <summary>
        /// The sub detail panel color
        /// </summary>
        public static Color SUB_DETAIL_PANEL_COLOR = new Color(77f / 255f, 166f / 255f, 255f / 255f);

        /// <summary>
        /// The sub detail background color
        /// </summary>
        public static Color SUB_DETAIL_BACKGROUND_COLOR = new Color(255f / 255f, 217f / 255f, 179f / 255f);

        /// <summary>
        /// The internal itens color
        /// </summary>
        public static Color INTERNAL_ITENS_COLOR = new Color(255f / 255f, 209f / 255f, 26f / 255f);

        /// <summary>
        /// The explanation font color
        /// </summary>
        public static Color EXPLANATION_FONT_COLOR = new Color(255f / 255f, 209f / 255f, 26f / 255f);

        /// <summary>
        /// The simple explanation font color
        /// </summary>
        public static Color SIMPLE_EXPLANATION_FONT_COLOR = new Color(204f / 255f, 255f / 255f, 153f / 255f);

        /// <summary>
        /// The simple danger font color
        /// </summary>
        public static Color SIMPLE_DANGER_FONT_COLOR = new Color(255f / 255f, 102f / 255f, 0f / 255f, 0.70f);

        /// <summary>
        /// The default script font color
        /// </summary>
        public static Color DEFAULT_SCRIPT_FONT_COLOR = new Color(0f / 255f, 153f / 255f, 0f / 255f, 1.00f);

        /// <summary>
        /// The sub detail font color
        /// </summary>
        public static Color SUB_DETAIL_FONT_COLOR = Color.white;

        /// <summary>
        /// The line divisor color
        /// </summary>
        public static Color LINE_DIVISOR_COLOR = new Color(255f / 255f, 209f / 255f, 26f / 255f, 0.25f);

        /// <summary>
        /// The line divisor color
        /// </summary>
        public static Color LINE_DIVISOR_COLOR_SUB = new Color(255f / 255f, 209f / 255f, 26f / 255f, 1.0f);

        /// <summary>
        /// The help button color
        /// </summary>
        public static Color HELP_BUTTON_COLOR = new Color(175f / 255f, 175f / 255f, 175f / 255f, 1.0f);

        /// <summary>
        /// The default header title color
        /// </summary>
        public static Color DEFAULT_HEADER_TITLE_COLOR = new Color(204f / 255f, 204f / 255f, 0f / 255f, 1.00f);

        /// <summary>
        /// The event background color
        /// </summary>
        public static Color EVENT_BACKGROUND_COLOR = new Color(255f / 255f, 102f / 255f, 0f / 255f);

        /// <summary>
        /// The event background clear color
        /// </summary>
        public static Color EVENT_BACKGROUND_CLEAR_COLOR = new Color(255f / 255f, 221f / 255f, 204f / 255f);

        /// <summary>
        /// The error background color
        /// </summary>
        public static Color ERROR_BACKGROUND_COLOR = new Color(128f / 255f, 0f / 255f, 0f / 255f);

        /// <summary>
        /// The error font color
        /// </summary>
        public static Color ERROR_FONT_COLOR = Color.white;

        /// <summary>
        /// The detail information color
        /// </summary>
        public static Color DETAIL_INFO_COLOR = new Color(255f / 255f, 153f / 255f, 0f / 255f);

        /// <summary>
        /// The subdetail item color
        /// </summary>
        public static Color SUBDETAIL_ITEM_COLOR = new Color(0f / 255f, 51f / 255f, 204f / 255f);


        /// <summary>
        /// Horizontals the line.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <param name="height">The height.</param>
        /// <param name="margin">The margin.</param>
        public static void HorizontalLine(Color color, float height, Vector2 margin)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(margin.x);
            EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, height), color);
            GUILayout.Space(margin.y);
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Prints the image.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="backGround">The back ground.</param>
        /// <param name="leftOffset">The left offset.</param>
        /// <param name="rightOffset">The right offset.</param>
        public static void PrintImage(string image, Color backGround, int leftOffset = 0, int rightOffset = 0) {
            GUILayout.BeginHorizontal();
            Color previousColor = GUI.contentColor;
            GUI.backgroundColor = backGround;
            GUILayout.Box(Resources.Load(image) as Texture, GUILayout.Width(EditorGUIUtility.currentViewWidth - rightOffset), GUILayout.Height(50));
            GUI.backgroundColor = previousColor;
            GUILayout.EndHorizontal();

        }

        /// <summary>
        /// Prints the header.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="backGround">The back ground.</param>
        /// <param name="fontColor">Color of the font.</param>
        /// <param name="fontSize">Size of the font.</param>
        /// <param name="icon">The icon.</param>
        public static void PrintHeader(string title, Color backGround, Color fontColor, int fontSize, string icon = null, bool adjustAlignment = false, Action extraCall = null) {
            Color previousColor = GUI.backgroundColor;
            Color previousFontColor = GUI.contentColor;
            GUI.backgroundColor = backGround;
            GUILayout.BeginHorizontal("box");
            GUILayout.Space(5.0f);
            if (adjustAlignment) {
                GUILayout.BeginVertical();
                GUILayout.Space(2.0f);
            }
            if (!string.IsNullOrEmpty(icon)) {
                if (GUILayout.Button(Resources.Load(icon) as Texture, GUIStyle.none, GUILayout.Width(18), GUILayout.Height(18))) {
                }
            }
            if (adjustAlignment) {
                GUILayout.EndVertical();
            }
            int previousSize = GUI.skin.label.fontSize;
            GUI.skin.label.fontSize = fontSize;
            GUI.contentColor = fontColor;
            GUILayout.Label(title);
            GUI.skin.label.fontSize = previousSize;
            if (extraCall != null) {
                extraCall.Invoke();
            }
            GUILayout.EndHorizontal();
            GUI.backgroundColor = previousColor;
            GUI.contentColor = previousFontColor;
        }

        /// <summary>
        /// Prints the vector3 boolean axis.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="title">The title.</param>
        /// <param name="icon">The icon.</param>
        public static void PrintVector3BooleanAxis(ref SerializedProperty value, string title, string icon) {
            GUILayout.BeginHorizontal("box");
            if (GUILayout.Button(Resources.Load(icon) as Texture, GUIStyle.none, GUILayout.Width(18), GUILayout.Height(18))) {
            }
            GUILayout.Space(DEFAULT_ICON_SPACING);
            if (GUILayout.Button(Resources.Load(value.GetArrayElementAtIndex(0).boolValue ? "checked_x" : "unchecked_x") as Texture, GUIStyle.none, GUILayout.Width(18), GUILayout.Height(18))) {
                value.GetArrayElementAtIndex(0).boolValue = !value.GetArrayElementAtIndex(0).boolValue;
            }
            GUILayout.Space(5.0f);
            if (GUILayout.Button(Resources.Load(value.GetArrayElementAtIndex(1).boolValue ? "checked_y" : "unchecked_y") as Texture, GUIStyle.none, GUILayout.Width(18), GUILayout.Height(18))) {
                value.GetArrayElementAtIndex(1).boolValue = !value.GetArrayElementAtIndex(1).boolValue;
            }
            GUILayout.Space(5.0f);
            if (GUILayout.Button(Resources.Load(value.GetArrayElementAtIndex(2).boolValue ? "checked_z" : "unchecked_z") as Texture, GUIStyle.none, GUILayout.Width(18), GUILayout.Height(18))) {
                value.GetArrayElementAtIndex(2).boolValue = !value.GetArrayElementAtIndex(2).boolValue;
            }
            int previousSize = GUI.skin.label.fontSize;
            GUI.skin.label.fontSize = 14;
            GUILayout.Label(title);
            GUI.skin.label.fontSize = previousSize;
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Prints the vector3 boolean axis.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="title">The title.</param>
        /// <param name="icon">The icon.</param>
        public static bool PrintVector3BooleanAxis(ref bool[] value, string title, string icon) {
            bool result = false;
            GUILayout.BeginHorizontal("box");
            if (GUILayout.Button(Resources.Load(icon) as Texture, GUIStyle.none, GUILayout.Width(18), GUILayout.Height(18))) {
            }
            GUILayout.Space(DEFAULT_ICON_SPACING);
            if (GUILayout.Button(Resources.Load(value[0] ? "checked_x" : "unchecked_x") as Texture, GUIStyle.none, GUILayout.Width(18), GUILayout.Height(18))) {
                value[0] = !value[0];
                result = true;
            }
            GUILayout.Space(5.0f);
            if (GUILayout.Button(Resources.Load(value[1] ? "checked_y" : "unchecked_y") as Texture, GUIStyle.none, GUILayout.Width(18), GUILayout.Height(18))) {
                value[1] = !value[1];
                result = true;
            }
            GUILayout.Space(5.0f);
            if (GUILayout.Button(Resources.Load(value[2] ? "checked_z" : "unchecked_z") as Texture, GUIStyle.none, GUILayout.Width(18), GUILayout.Height(18))) {
                value[2] = !value[2];
                result = true;
            }
            int previousSize = GUI.skin.label.fontSize;
            GUI.skin.label.fontSize = 14;
            GUILayout.Label(title);
            GUI.skin.label.fontSize = previousSize;
            GUILayout.EndHorizontal();
            return result;
        }

        /// <summary>
        /// Prints the boolean.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="title">The title.</param>
        /// <param name="icon">The icon.</param>
        /// <param name="iconSize">Size of the icon.</param>
        /// <param name="fontSize">Size of the font.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static bool PrintBoolean(ref SerializedProperty value, string title, string icon = null, int iconSize = 18, int fontSize = 14) {
            bool executed = false;
            GUILayout.BeginHorizontal("box");
            if (!String.IsNullOrEmpty(icon)) {
                if (GUILayout.Button(Resources.Load(icon) as Texture, GUIStyle.none, GUILayout.Width(iconSize), GUILayout.Height(iconSize))) {
                }
            }
            GUILayout.Space(DEFAULT_ICON_SPACING);
            if (GUILayout.Button(Resources.Load(value.boolValue ? "oo_checked" : "oo_unchecked") as Texture, GUIStyle.none, GUILayout.Width(iconSize), GUILayout.Height(iconSize))) {
                value.boolValue = !value.boolValue;
                executed = true;
            }
            int previousSize = GUI.skin.label.fontSize;
            GUI.skin.label.fontSize = fontSize;
            GUILayout.Label(title);
            GUI.skin.label.fontSize = previousSize;
            GUILayout.EndHorizontal();
            return executed;
        }

        /// <summary>
        /// Prints the boolean squared by reference.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="title">The title.</param>
        /// <param name="icon">The icon.</param>
        /// <param name="iconSize">Size of the icon.</param>
        /// <param name="fontSize">Size of the font.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static bool PrintBooleanSquaredByRef(ref SerializedProperty value, string title, string icon = null, int iconSize = 18, int fontSize = 14, bool groupInBox = true) {
            bool executed = false;
            if (groupInBox) {
                GUILayout.BeginHorizontal("box");
            } else {
                GUILayout.BeginHorizontal();
            }
            if (!String.IsNullOrEmpty(icon)) {
                if (GUILayout.Button(Resources.Load(icon) as Texture, GUIStyle.none, GUILayout.Width(iconSize), GUILayout.Height(iconSize))) {
                }
            }
            GUILayout.Space(DEFAULT_ICON_SPACING);
            if (GUILayout.Button(Resources.Load(value.boolValue ? "oo_checked_squared" : "oo_unchecked_squared") as Texture, GUIStyle.none, GUILayout.Width(iconSize), GUILayout.Height(iconSize))) {
                value.boolValue = !value.boolValue;
                executed = true;
            }
            int previousSize = GUI.skin.label.fontSize;
            GUI.skin.label.fontSize = fontSize;
            GUILayout.Label(title);
            GUI.skin.label.fontSize = previousSize;
            GUILayout.EndHorizontal();
            return executed;
        }


        /// <summary>
        /// Prints the boolean.
        /// </summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        /// <param name="title">The title.</param>
        /// <param name="icon">The icon.</param>
        /// <param name="iconSize">Size of the icon.</param>
        /// <param name="fontSize">Size of the font.</param>
        /// <param name="isolateHorizontal">if set to <c>true</c> [isolate horizontal].</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static bool PrintBoolean(ref bool value, string title, string icon = null, int iconSize = 18, int fontSize = 14, bool isolateHorizontal = true) {
            if (isolateHorizontal) {
                GUILayout.BeginHorizontal();
            }
            bool executed = false;
            if (!String.IsNullOrEmpty(icon)) {
                if (GUILayout.Button(Resources.Load(icon) as Texture, GUIStyle.none, GUILayout.Width(iconSize), GUILayout.Height(iconSize))) {
                }
            }
            GUILayout.Space(DEFAULT_ICON_SPACING);
            if (GUILayout.Button(Resources.Load(value ? "oo_checked" : "oo_unchecked") as Texture, GUIStyle.none, GUILayout.Width(iconSize), GUILayout.Height(iconSize))) {
                value = !value;
                executed = true;
            }
            int previousSize = GUI.skin.label.fontSize;
            GUI.skin.label.fontSize = fontSize;
            GUILayout.Label(title);
            GUI.skin.label.fontSize = previousSize;
            if (isolateHorizontal) {
                GUILayout.EndHorizontal();
            }
            return executed;
        }

        /// <summary>
        /// Prints the boolean squared.
        /// </summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        /// <param name="title">The title.</param>
        /// <param name="icon">The icon.</param>
        /// <param name="iconSize">Size of the icon.</param>
        /// <param name="fontSize">Size of the font.</param>
        /// <param name="isolateHorizontal">if set to <c>true</c> [isolate horizontal].</param>
        /// <param name="topOffset">The top offset.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static bool PrintBooleanSquared(ref bool value, string title, string icon = null, int iconSize = 18, int fontSize = 14, bool isolateHorizontal = true, float topOffset = 0f) {
            if (isolateHorizontal) {
                GUILayout.BeginHorizontal();
            }
            bool executed = false;
            if (!String.IsNullOrEmpty(icon)) {
                if (GUILayout.Button(Resources.Load(icon) as Texture, GUIStyle.none, GUILayout.Width(iconSize), GUILayout.Height(iconSize))) {
                }
            }
            GUILayout.Space(DEFAULT_ICON_SPACING);
            GUILayout.BeginVertical(GUILayout.Width(iconSize));
            GUILayout.Space(topOffset);
            if (GUILayout.Button(Resources.Load(value ? "oo_checked_squared" : "oo_unchecked_squared") as Texture, GUIStyle.none, GUILayout.Width(iconSize), GUILayout.Height(iconSize))) {
                value = !value;
                executed = true;
            }
            GUILayout.EndVertical();
            int previousSize = GUI.skin.label.fontSize;
            GUI.skin.label.fontSize = fontSize;
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            GUILayout.Label(title);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUI.skin.label.fontSize = previousSize;
            if (isolateHorizontal) {
                GUILayout.EndHorizontal();
            }
            return executed;
        }

        /// <summary>
        /// Prints the visibility boolean.
        /// </summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        /// <param name="title">The title.</param>
        /// <param name="icon">The icon.</param>
        /// <param name="iconSize">Size of the icon.</param>
        /// <param name="fontSize">Size of the font.</param>
        /// <param name="isolateHorizontal">if set to <c>true</c> [isolate horizontal].</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static bool PrintVisibilityBoolean(ref bool value, string title, string icon = null, int iconSize = 18, int fontSize = 14, bool isolateHorizontal = true) {
            if (isolateHorizontal) {
                GUILayout.BeginHorizontal("box");
            }
            bool executed = false;
            if (!String.IsNullOrEmpty(icon)) {
                if (GUILayout.Button(Resources.Load(icon) as Texture, GUIStyle.none, GUILayout.Width(iconSize), GUILayout.Height(iconSize))) {
                }
            }
            GUILayout.Space(DEFAULT_ICON_SPACING);
            if (GUILayout.Button(Resources.Load(value ? "oo_eye_on" : "oo_eye_off") as Texture, GUIStyle.none, GUILayout.Width(iconSize), GUILayout.Height(iconSize))) {
                value = !value;
                executed = true;
            }
            int previousSize = GUI.skin.label.fontSize;
            GUI.skin.label.fontSize = fontSize;
            GUILayout.Label(title);
            GUI.skin.label.fontSize = previousSize;
            if (isolateHorizontal) {
                GUILayout.EndHorizontal();
            }
            return executed;
        }

        /// <summary>
        /// Prints the visibility boolean.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="title">The title.</param>
        /// <param name="icon">The icon.</param>
        /// <param name="iconSize">Size of the icon.</param>
        /// <param name="fontSize">Size of the font.</param>
        /// <param name="isolateHorizontal">if set to <c>true</c> [isolate horizontal].</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static bool PrintVisibilityBoolean(ref SerializedProperty value, string title, string icon = null, int iconSize = 18, int fontSize = 14, bool isolateHorizontal = true) {
            if (isolateHorizontal) {
                GUILayout.BeginHorizontal("box");
            }
            bool executed = false;
            if (!String.IsNullOrEmpty(icon)) {
                if (GUILayout.Button(Resources.Load(icon) as Texture, GUIStyle.none, GUILayout.Width(iconSize), GUILayout.Height(iconSize))) {
                }
            }
            GUILayout.Space(DEFAULT_ICON_SPACING);
            if (GUILayout.Button(Resources.Load(value.boolValue ? "oo_eye_on" : "oo_eye_off") as Texture, GUIStyle.none, GUILayout.Width(iconSize), GUILayout.Height(iconSize))) {
                value.boolValue = !value.boolValue;
                executed = true;
            }
            int previousSize = GUI.skin.label.fontSize;
            GUI.skin.label.fontSize = fontSize;
            GUILayout.Label(title);
            GUI.skin.label.fontSize = previousSize;
            if (isolateHorizontal) {
                GUILayout.EndHorizontal();
            }
            return executed;
        }

        /// <summary>
        /// Prints the visibility boolean with icon.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="title">The title.</param>
        /// <param name="iconLeft">The icon left.</param>
        /// <param name="iconRight">The icon right.</param>
        /// <param name="iconSize">Size of the icon.</param>
        /// <param name="fontSize">Size of the font.</param>
        /// <param name="isolateHorizontal">if set to <c>true</c> [isolate horizontal].</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static bool PrintVisibilityBooleanWithIcon(ref SerializedProperty value, string title, string iconLeft = null, string iconRight = null, int iconSize = 18, int fontSize = 14, bool isolateHorizontal = true) {
            if (isolateHorizontal) {
                GUILayout.BeginHorizontal("box");
            }
            bool executed = false;
            if (!String.IsNullOrEmpty(iconLeft)) {
                if (GUILayout.Button(Resources.Load(iconLeft) as Texture, GUIStyle.none, GUILayout.Width(iconSize), GUILayout.Height(iconSize))) {
                }
            }
            GUILayout.Space(DEFAULT_ICON_SPACING);
            if (GUILayout.Button(Resources.Load(value.boolValue ? "oo_eye_on" : "oo_eye_off") as Texture, GUIStyle.none, GUILayout.Width(iconSize), GUILayout.Height(iconSize))) {
                value.boolValue = !value.boolValue;
                executed = true;
            }
            int previousSize = GUI.skin.label.fontSize;
            GUI.skin.label.fontSize = fontSize;
            GUILayout.Label(title);
            GUI.skin.label.fontSize = previousSize;

            if (iconRight != null) {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(Resources.Load(iconRight) as Texture, GUIStyle.none, GUILayout.Width(16), GUILayout.Height(16))) {                    
                }
            }

            if (isolateHorizontal) {
                GUILayout.EndHorizontal();
            }
            return executed;
        }

        /// <summary>
        /// Prints the visibility boolean with icon.
        /// </summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        /// <param name="title">The title.</param>
        /// <param name="iconLeft">The icon left.</param>
        /// <param name="iconRight">The icon right.</param>
        /// <param name="iconSize">Size of the icon.</param>
        /// <param name="fontSize">Size of the font.</param>
        /// <param name="isolateHorizontal">if set to <c>true</c> [isolate horizontal].</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static bool PrintVisibilityBooleanWithIcon(ref bool value, bool isFiled, string title, string iconLeft = null, string iconRight = null, int iconSize = 18, int fontSize = 14, bool isolateHorizontal = true) {
            if (isolateHorizontal) {
                GUILayout.BeginHorizontal("box");
            }
            bool executed = false;
            if (!String.IsNullOrEmpty(iconLeft)) {
                if (GUILayout.Button(Resources.Load(iconLeft) as Texture, GUIStyle.none, GUILayout.Width(iconSize), GUILayout.Height(iconSize))) {
                }
            }
            GUILayout.Space(DEFAULT_ICON_SPACING);
            if (GUILayout.Button(Resources.Load((value ? (isFiled) ? "oo_eye_on_filled" : "oo_eye_on_not_filled" : (isFiled) ? "oo_eye_off_filled" : "oo_eye_off_not_filled")) as Texture, GUIStyle.none, GUILayout.Width(iconSize), GUILayout.Height(iconSize))) {
                value = !value;
                executed = true;
            }
            int previousSize = GUI.skin.label.fontSize;
            GUI.skin.label.fontSize = fontSize;
            GUILayout.Label(title);
            GUI.skin.label.fontSize = previousSize;

            if (iconRight != null) {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(Resources.Load(iconRight) as Texture, GUIStyle.none, GUILayout.Width(16), GUILayout.Height(16))) {                    
                }
            }

            if (isolateHorizontal) {
                GUILayout.EndHorizontal();
            }
            return executed;
        }

        /// <summary>
        /// Prints the visibility boolean with icon.
        /// </summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        /// <param name="title">The title.</param>
        /// <param name="iconLeft">The icon left.</param>
        /// <param name="iconRight">The icon right.</param>
        /// <param name="iconSize">Size of the icon.</param>
        /// <param name="fontSize">Size of the font.</param>
        /// <param name="isolateHorizontal">if set to <c>true</c> [isolate horizontal].</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static bool PrintVisibilityBooleanWithIcon(ref bool value, string title, string iconLeft = null, string iconRight = null, int iconSize = 18, int fontSize = 14, bool isolateHorizontal = true) {
            if (isolateHorizontal) {
                GUILayout.BeginHorizontal("box");
            }
            bool executed = false;
            if (!String.IsNullOrEmpty(iconLeft)) {
                if (GUILayout.Button(Resources.Load(iconLeft) as Texture, GUIStyle.none, GUILayout.Width(iconSize), GUILayout.Height(iconSize))) {
                }
            }
            GUILayout.Space(DEFAULT_ICON_SPACING);
            if (GUILayout.Button(Resources.Load(value ? "oo_eye_on" : "oo_eye_off") as Texture, GUIStyle.none, GUILayout.Width(iconSize), GUILayout.Height(iconSize))) {
                value = !value;
                executed = true;
            }
            int previousSize = GUI.skin.label.fontSize;
            GUI.skin.label.fontSize = fontSize;
            GUILayout.Label(title);
            GUI.skin.label.fontSize = previousSize;

            if (iconRight != null) {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(Resources.Load(iconRight) as Texture, GUIStyle.none, GUILayout.Width(16), GUILayout.Height(16))) {
                }
            }

            if (isolateHorizontal) {
                GUILayout.EndHorizontal();
            }
            return executed;
        }

        /// <summary>
        /// Prints the deletable boolean.
        /// </summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        /// <param name="title">The title.</param>
        /// <param name="iconSize">Size of the icon.</param>
        /// <param name="fontSize">Size of the font.</param>
        /// <param name="isolateHorizontal">if set to <c>true</c> [isolate horizontal].</param>
        /// <param name="onDelete">The on delete.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static bool PrintDeletableBoolean(ref bool value, string title, int iconSize = 18, int fontSize = 14, bool isolateHorizontal = true, Action onDelete = null) {
            if (isolateHorizontal) {
                GUILayout.BeginHorizontal("box");
            }
            bool executed = false;
            GUILayout.Space(DEFAULT_ICON_SPACING);
            if (GUILayout.Button(Resources.Load(value ? "oo_eye_on" : "oo_eye_off") as Texture, GUIStyle.none, GUILayout.Width(iconSize), GUILayout.Height(iconSize))) {
                value = !value;
                executed = true;
            }
            int previousSize = GUI.skin.label.fontSize;
            GUI.skin.label.fontSize = fontSize;
            GUILayout.Label(title);
            GUI.skin.label.fontSize = previousSize;

            GUILayout.FlexibleSpace();
            if (GUILayout.Button(Resources.Load("oo_remove") as Texture, GUIStyle.none, GUILayout.Width(16), GUILayout.Height(16))) {
                if ( onDelete != null ) {
                    onDelete.Invoke();
                }
            }

            if (isolateHorizontal) {
                GUILayout.EndHorizontal();
            }
            return executed;
        }

        /// <summary>
        /// Draws the horizontal int bar.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="min">The minimum.</param>
        /// <param name="max">The maximum.</param>
        /// <param name="label">The label.</param>
        public static void DrawHorizontalIntBar(ref int value, int min, int max, String label = null, float width = 250f, bool allowUnlimited = false, string unlimitedText = "") {
            float current = value;
            EditorGUILayout.BeginVertical();
            if (label != null) {
                EditorUtils.PrintSizedLabel(((allowUnlimited == false) || ((allowUnlimited == true) && (value < max))) ? String.Format(label + " [{0}]", current) : String.Format(label + " [{0}]", unlimitedText), EditorUtils.DEFAULT_SLIDER_FONT_SIZE, EditorUtils.INTERNAL_ITENS_COLOR);
                GUILayout.Space(5);
            }
            value = Mathf.FloorToInt(GUILayout.HorizontalSlider(value, min, max, GUILayout.Width(width)));
            GUILayout.Space(15);
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draws the horizontal int bar.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="label">The label.</param>
        /// <param name="min">The minimum.</param>
        /// <param name="max">The maximum.</param>
        /// <param name="fontSize">Size of the font.</param>
        /// <param name="valueText">The value text.</param>
        public static void DrawHorizontalIntBar(ref SerializedProperty value, String label, int min, int max, int fontSize = 0, string valueText = null) {
            int current = value.intValue;
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10);
            EditorGUILayout.BeginVertical();
            EditorUtils.PrintSizedLabel(String.Format(label + " [{0}]", (valueText == null) ? current : valueText), (fontSize > 0) ? fontSize : EditorUtils.DEFAULT_SLIDER_FONT_SIZE, EditorUtils.INTERNAL_ITENS_COLOR);
            GUILayout.Space(5);
            Color previousColor = GUI.backgroundColor;
            GUI.backgroundColor = EditorUtils.INTERNAL_ITENS_COLOR;
            value.intValue = Mathf.FloorToInt(GUILayout.HorizontalSlider(value.intValue, min, max));
            GUI.backgroundColor = previousColor;
            GUILayout.Space(15);
            EditorGUILayout.EndVertical();
            GUILayout.Space(10);
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draws the horizontal float bar.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="label">The label.</param>
        /// <param name="min">The minimum.</param>
        /// <param name="max">The maximum.</param>
        /// <param name="increaseDecrease">The increase decrease.</param>
        /// <param name="format">The format.</param>
        public static void DrawHorizontalFloatBar(ref float value, String label, float min, float max, float increaseDecrease, String format) {
            float current = value;
            EditorGUILayout.BeginVertical();
            EditorUtils.PrintSizedLabel(String.Format(label + " [{0:" + format + "}]", current), EditorUtils.DEFAULT_SLIDER_FONT_SIZE, EditorUtils.INTERNAL_ITENS_COLOR);
            GUILayout.Space(5);
            value = GUILayout.HorizontalSlider(value, min, max, GUILayout.Width(250));
            GUILayout.Space(15);
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Draws the horizontal float bar.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="label">The label.</param>
        /// <param name="min">The minimum.</param>
        /// <param name="max">The maximum.</param>
        /// <param name="increaseDecrease">The increase decrease.</param>
        /// <param name="format">The format.</param>
        public static void DrawHorizontalFloatBar(ref SerializedProperty value, String label, float min, float max, float increaseDecrease, String format) {
            float current = value.floatValue;
            EditorGUILayout.BeginVertical();
            EditorUtils.PrintSizedLabel(String.Format(label + " [{0:" + format + "}]", current), EditorUtils.DEFAULT_SLIDER_FONT_SIZE, EditorUtils.INTERNAL_ITENS_COLOR);
            GUILayout.Space(15);
            value.floatValue = GUILayout.HorizontalSlider(value.floatValue, min, max);
            GUILayout.Space(15);
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Prints the simple explanation.
        /// </summary>
        /// <param name="text">The text.</param>
        public static void PrintSimpleExplanation(string text) {
            EditorGUILayout.BeginVertical();
            EditorUtils.PrintSizedLabelWrap(text, EditorUtils.DEFAULT_EXPLANATION_FONT_SIZE, EditorUtils.SIMPLE_EXPLANATION_FONT_COLOR);
            GUILayout.Space(5);
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Prints the simple explanation.
        /// </summary>
        /// <param name="text">The text.</param>
        public static void PrintSimpleExplanation(string text, int fontSize, Color textColor) {
            EditorGUILayout.BeginVertical();
            EditorUtils.PrintSizedLabelWrap(text, fontSize, textColor);
            GUILayout.Space(5);
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Prints the simple explanation.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="fontColor">Color of the font.</param>
        /// <param name="fontSize">Size of the font.</param>
        /// <param name="wrap">if set to <c>true</c> [wrap].</param>
        public static void PrintSimpleExplanation(string text, Color fontColor, int fontSize = EditorUtils.DEFAULT_EXPLANATION_FONT_SIZE, bool wrap = true) {
            EditorGUILayout.BeginVertical();
            if (wrap) {
                EditorUtils.PrintSizedLabelWrap(text, fontSize, fontColor);
            } else {
                EditorUtils.PrintSizedLabel(text, fontSize, fontColor);
            }
            GUILayout.Space(5);
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Prints the explanation label.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="icon">The icon.</param>
        /// <param name="textColor">Color of the text.</param>
        /// <param name="startSpace">The start space.</param>
        /// <param name="fontSize">Size of the font.</param>
        /// <param name="offsetHeight">Height of the offset.</param>
        /// <param name="wrap">if set to <c>true</c> [wrap].</param>
        public static void PrintExplanationLabel(string text, string icon, Color textColor, float startSpace = 15f, int fontSize = 13, int offsetHeight = 0, bool wrap = true, float iconSpace = DEFAULT_ICON_SPACING) {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(startSpace);
            if (GUILayout.Button(Resources.Load(icon) as Texture, GUIStyle.none, GUILayout.Width(18), GUILayout.Height(18))) {
            }
            GUILayout.Space(iconSpace);
            EditorGUILayout.BeginVertical();
            GUILayout.Space(offsetHeight);
            if (wrap) {
                EditorUtils.PrintSizedLabelWrap(text, fontSize, textColor);
            } else {
                EditorUtils.PrintSizedLabel(text, fontSize, textColor);
            }
            EditorGUILayout.EndVertical();
            GUILayout.Space(20);
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Prints the explanation label.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="icon">The icon.</param>
        public static void PrintExplanationLabel(string text, string icon, float iconWidth = 18.0f, float iconHeight = 18.0f) {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(15);
            if (GUILayout.Button(Resources.Load(icon) as Texture, GUIStyle.none, GUILayout.Width(iconWidth), GUILayout.Height(iconHeight))) {
            }
            GUILayout.Space(DEFAULT_ICON_SPACING);
            EditorUtils.PrintSizedLabelWrap(text, EditorUtils.DEFAULT_EXPLANATION_FONT_SIZE, EditorUtils.EXPLANATION_FONT_COLOR);
            GUILayout.Space(20);
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Prints the explanation label.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="icon">The icon.</param>
        public static void PrintExplanationLabelColored(string text, string icon, Color color, float iconWidth = 18.0f, float iconHeight = 18.0f) {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(15);
            if (GUILayout.Button(Resources.Load(icon) as Texture, GUIStyle.none, GUILayout.Width(iconWidth), GUILayout.Height(iconHeight))) {
            }
            GUILayout.Space(DEFAULT_ICON_SPACING);
            EditorUtils.PrintSizedLabelWrap(text, EditorUtils.DEFAULT_EXPLANATION_FONT_SIZE, color);
            GUILayout.Space(20);
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Prints the sized label wrap.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="fontSize">Size of the font.</param>
        /// <param name="color">The color.</param>
        public static void PrintSizedLabelWrap(String text, int fontSize, Color color) {
            int previousSize = GUI.skin.label.fontSize;
            GUI.skin.label.fontSize = fontSize;
            Color previousFontColor = GUI.contentColor;
            GUI.contentColor = color;
            GUILayout.Label(text, EditorStyles.wordWrappedLabel);
            GUI.skin.label.fontSize = previousSize;
            GUI.contentColor = previousFontColor;
        }

        /// <summary>
        /// Prints the sized label.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="fontSize">Size of the font.</param>
        /// <param name="color">The color.</param>
        public static void PrintSizedLabel(String text, int fontSize, Color color) {
            int previousSize = GUI.skin.label.fontSize;
            GUI.skin.label.fontSize = fontSize;
            Color previousFontColor = GUI.contentColor;
            GUI.contentColor = color;
            GUILayout.Label(text);
            GUI.skin.label.fontSize = previousSize;
            GUI.contentColor = previousFontColor;
        }

        /// <summary>
        /// Prints the image button.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="icon">The icon.</param>
        /// <param name="buttonColor">Color of the button.</param>
        /// <param name="fontColor">Color of the font.</param>
        /// <param name="onClick">The on click.</param>
        public static void PrintImageButton(string text, string icon, Color buttonColor, Color fontColor, Action onClick) {
            Rect r = EditorGUILayout.BeginHorizontal("Box");
            if (GUI.Button(r, GUIContent.none)) {
                onClick.Invoke();
            }
            EditorGUILayout.BeginVertical(BackgroundStyle.Get(buttonColor));
            GUILayout.Space(5.0f);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(15);
            if (GUILayout.Button(Resources.Load(icon) as Texture, GUIStyle.none, GUILayout.Width(18), GUILayout.Height(18))) {
            }
            GUILayout.Space(DEFAULT_ICON_SPACING);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            GUILayout.Space(-2);
            EditorUtils.PrintSizedLabel(text, EditorUtils.DEFAULT_IMAGE_BUTTON_FONT_SIZE, fontColor);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(20);
            EditorGUILayout.EndVertical();


            GUILayout.Space(5.0f);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Prints the image button.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="icon">The icon.</param>
        /// <param name="buttonColor">Color of the button.</param>
        /// <param name="fontColor">Color of the font.</param>
        /// <param name="fontSize">Size of the font.</param>
        /// <param name="iconWdth">The icon WDTH.</param>
        /// <param name="iconHeight">Height of the icon.</param>
        /// <param name="onClick">The on click.</param>
        public static void PrintImageButton(string text, string icon, Color buttonColor, Color fontColor, int fontSize, float iconWdth, float iconHeight, Action onClick) {
            Rect r = EditorGUILayout.BeginHorizontal("Box");
            if (GUI.Button(r, GUIContent.none)) {
                onClick.Invoke();
            }
            EditorGUILayout.BeginVertical(BackgroundStyle.Get(buttonColor));
            GUILayout.Space(5.0f);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(15);
            if (GUILayout.Button(Resources.Load(icon) as Texture, GUIStyle.none, GUILayout.Width(iconWdth), GUILayout.Height(iconHeight))) {
            }
            GUILayout.Space(DEFAULT_ICON_SPACING);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            GUILayout.Space(-2);
            EditorUtils.PrintSizedLabel(text, fontSize, fontColor);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(20);
            EditorGUILayout.EndVertical();

            GUILayout.Space(5.0f);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Prints the sized label.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="fontSize">Size of the font.</param>
        private static void PrintSizedLabel(String text, int fontSize) {
            int previousSize = GUI.skin.label.fontSize;
            GUI.skin.label.fontSize = fontSize;
            Color previousFontColor = GUI.contentColor;
            GUI.contentColor = Color.white;
            GUILayout.Label(text);
            GUI.skin.label.fontSize = previousSize;
            GUI.contentColor = previousFontColor;
        }

        /// <summary>
        /// Return a colro with alpha scale applied
        /// </summary>
        /// <param name="color">Color to apply alpha</param>
        /// <param name="alpha">Alpha value</param>
        /// <returns></returns>
        public static Color WithAlpha(this Color color, float alpha) {
            color.a = alpha;
            return color;
        }

        /// <summary>
        /// Return a grayscale of color
        /// </summary>
        /// <param name="color">Color to apply gray</param>
        /// <param name="brightness">Brightness value</param>
        /// <returns></returns>
        public static Color Gray(this Color color, float brightness) {
            return new Color(brightness, brightness, brightness);
        }

        /// <summary>
        /// Return a colro with alpha scale applied
        /// </summary>
        /// <param name="color">Color to apply alpha</param>
        /// <param name="alphaMultiplier">Alpha value</param>
        /// <returns></returns>
        public static Color WithAlphaMultiplied(this Color color, float alphaMultiplier) {
            color.a *= alphaMultiplier;
            return color;
        }
    }
#endif
}