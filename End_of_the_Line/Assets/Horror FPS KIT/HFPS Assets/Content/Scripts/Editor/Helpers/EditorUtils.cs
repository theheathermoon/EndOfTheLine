using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if TW_LOCALIZATION_PRESENT
using ThunderWire.Localization;
using ThunderWire.Localization.Editor;
#endif

namespace ThunderWire.Editors
{
    public static class EditorUtils
    {
        public static class Styles
        {
            public static GUIStyle IconButton => GUI.skin.FindStyle("IconButton");
            public static readonly GUIContent PlusIcon = EditorGUIUtility.TrIconContent("Toolbar Plus", "Add Item");
            public static readonly GUIContent MinusIcon = EditorGUIUtility.TrIconContent("Toolbar Minus", "Remove Item");
            public static readonly GUIContent RefreshIcon = EditorGUIUtility.TrIconContent("Refresh", "Refresh");
            public static readonly GUIContent Linked = EditorGUIUtility.TrIconContent("Linked");
            public static readonly GUIContent UnLinked = EditorGUIUtility.TrIconContent("Unlinked");
            public static readonly GUIContent Database = EditorGUIUtility.TrIconContent("Package Manager");
            public static readonly GUIContent GreenLight = EditorGUIUtility.TrIconContent("greenLight");
            public static readonly GUIContent OrangeLight = EditorGUIUtility.TrIconContent("orangeLight");
            public static readonly GUIContent RedLight = EditorGUIUtility.TrIconContent("redLight");
            public static readonly GUIContent DLLinked = EditorGUIUtility.TrIconContent("d_Linked", "Open Localization System Key Browser");
            public static readonly GUIContent DLUnLinked = EditorGUIUtility.TrIconContent("d_Unlinked", "Localization System not found!");

            public static GUIStyle RichLabel => new GUIStyle(EditorStyles.label)
            {
                richText = true
            };
        }

        public static void DrawOutline(Rect rect, RectOffset border)
        {
            Color color = new Color(0.6f, 0.6f, 0.6f, 1.333f);
            if (EditorGUIUtility.isProSkin)
            {
                color.r = 0.12f;
                color.g = 0.12f;
                color.b = 0.12f;
            }

            if (Event.current.type != EventType.Repaint)
                return;

            Color orgColor = GUI.color;
            GUI.color *= color;
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, border.top), EditorGUIUtility.whiteTexture); //top
            GUI.DrawTexture(new Rect(rect.x, rect.yMax - border.bottom, rect.width, border.bottom), EditorGUIUtility.whiteTexture); //bottom
            GUI.DrawTexture(new Rect(rect.x, rect.y + 1, border.left, rect.height - 2 * border.left), EditorGUIUtility.whiteTexture); //left
            GUI.DrawTexture(new Rect(rect.xMax - border.right, rect.y + 1, border.right, rect.height - 2 * border.right), EditorGUIUtility.whiteTexture); //right

            GUI.color = orgColor;
        }

        public static void DrawOutline(Rect rect, RectOffset border, Color color)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            Color orgColor = GUI.color;
            GUI.color *= color;
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, border.top), EditorGUIUtility.whiteTexture); //top
            GUI.DrawTexture(new Rect(rect.x, rect.yMax - border.bottom, rect.width, border.bottom), EditorGUIUtility.whiteTexture); //bottom
            GUI.DrawTexture(new Rect(rect.x, rect.y + 1, border.left, rect.height - 2 * border.left), EditorGUIUtility.whiteTexture); //left
            GUI.DrawTexture(new Rect(rect.xMax - border.right, rect.y + 1, border.right, rect.height - 2 * border.right), EditorGUIUtility.whiteTexture); //right

            GUI.color = orgColor;
        }

        public static Rect DrawHeaderWithBorder(string title, float height, ref Rect rect, bool rounded)
        {
            GUI.Box(rect, GUIContent.none, new GUIStyle(rounded ? "HelpBox" : "Tooltip"));
            rect.x += 1;
            rect.y += 1;
            rect.height -= 1;
            rect.width -= 2;

            var headerRect = rect;
            headerRect.height = height + EditorGUIUtility.standardVerticalSpacing;

            rect.y += headerRect.height;
            rect.height -= headerRect.height;

            EditorGUI.DrawRect(headerRect, new Color(0.1f, 0.1f, 0.1f, 0.4f));

            var labelRect = headerRect;
            labelRect.y += EditorGUIUtility.standardVerticalSpacing;
            labelRect.x += 2f;

            EditorGUI.LabelField(labelRect, title, EditorStyles.miniBoldLabel);

            return headerRect;
        }

        public static Rect DrawHeaderWithBorder(string title, float height, ref Rect rect, GUIStyle boxStyle)
        {
            GUI.Box(rect, GUIContent.none, boxStyle);
            rect.x += 1;
            rect.y += 1;
            rect.height -= 1;
            rect.width -= 2;

            var headerRect = rect;
            headerRect.height = height + EditorGUIUtility.standardVerticalSpacing;

            rect.y += headerRect.height;
            rect.height -= headerRect.height;

            EditorGUI.DrawRect(headerRect, new Color(0.1f, 0.1f, 0.1f, 0.4f));

            var labelRect = headerRect;
            labelRect.y += EditorGUIUtility.standardVerticalSpacing;
            labelRect.x += 2f;

            EditorGUI.LabelField(labelRect, title, EditorStyles.miniBoldLabel);

            return headerRect;
        }

        public static Rect DrawHeaderWithBorder(string title, float height, ref Rect rect, RectOffset border)
        {
            DrawOutline(rect, border);
            rect.x += 1;
            rect.y += 1;
            rect.height -= 1;
            rect.width -= 2;

            var headerRect = rect;
            headerRect.height = height + EditorGUIUtility.standardVerticalSpacing;

            rect.y += headerRect.height;
            rect.height -= headerRect.height;

            EditorGUI.DrawRect(headerRect, new Color(0.1f, 0.1f, 0.1f, 0.4f));

            var labelRect = headerRect;
            labelRect.y += EditorGUIUtility.standardVerticalSpacing;
            labelRect.x += 2f;

            EditorGUI.LabelField(labelRect, title, EditorStyles.miniBoldLabel);

            return headerRect;
        }

        public static bool DrawFoldoutHeader(float height, string title, bool state, bool miniLabel = false, bool hoverable = true)
        {
            Rect rect = GUILayoutUtility.GetRect(1f, height + EditorGUIUtility.standardVerticalSpacing);
            state = DrawFoldoutHeader(rect, title, state, miniLabel, hoverable);
            return state;
        }

        public static bool DrawFoldoutHeader(Rect rect, string title, bool state, bool miniLabel, bool hoverable)
        {
            Color headerColor = new Color(0.1f, 0.1f, 0.1f, 0f);

            var foldoutRect = rect;
            foldoutRect.y += 4f;
            foldoutRect.x += 2f;
            foldoutRect.width = 13f;
            foldoutRect.height = 13f;

            var labelRect = rect;
            labelRect.y += miniLabel ? EditorGUIUtility.standardVerticalSpacing : 0f;
            labelRect.xMin += 16f;
            labelRect.xMax -= 20f;

            // events
            var e = Event.current;
            if (rect.Contains(e.mousePosition))
            {
                if(hoverable) headerColor = new Color(0.6f, 0.6f, 0.6f, 0.2f);

                if (e.type == EventType.MouseDown && e.button == 0)
                {
                    state = !state;
                    e.Use();
                }
            }

            // background
            EditorGUI.DrawRect(rect, headerColor);

            // foldout toggle
            state = GUI.Toggle(foldoutRect, state, GUIContent.none, EditorStyles.foldout);

            // title
            EditorGUI.LabelField(labelRect, new GUIContent(title), miniLabel ? EditorStyles.miniBoldLabel : EditorStyles.boldLabel);

            return state;
        }

        public static bool DrawFoldoutHeader(Rect rect, GUIContent content, bool hoverable, bool state)
        {
            Color headerColor = new Color(0.1f, 0.1f, 0.1f, 0f);

            var foldoutRect = rect;
            foldoutRect.y += 4f;
            foldoutRect.x += 2f;
            foldoutRect.width = 13f;
            foldoutRect.height = 13f;

            var labelRect = rect;
            labelRect.xMin += 16f;
            labelRect.xMax -= 20f;

            // events
            var e = Event.current;
            if (rect.Contains(e.mousePosition))
            {
                if (hoverable) headerColor = new Color(0.6f, 0.6f, 0.6f, 0.2f);

                if (e.type == EventType.MouseDown && e.button == 0)
                {
                    state = !state;
                    e.Use();
                }
            }

            // background
            EditorGUI.DrawRect(rect, headerColor);

            // title
            EditorGUIUtility.SetIconSize(new Vector2(15, 15));
            EditorGUI.LabelField(labelRect, content);

            // foldout toggle
            state = GUI.Toggle(foldoutRect, state, GUIContent.none, EditorStyles.foldout);
            return state;
        }

        public static void DrawHeaderProperty(string title, string icon, SerializedProperty property)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                GUIContent iconTitle = EditorGUIUtility.TrTextContentWithIcon(" " + title, icon);
                Rect rect = GUILayoutUtility.GetRect(1, 20f);

                property.isExpanded = DrawFoldoutHeader(rect, iconTitle, true, property.isExpanded);

                if (property.isExpanded)
                {
                    EditorGUILayout.Space(3f);
                    DrawRelativeProperties(property, 20f);
                }
            }
            EditorGUILayout.EndVertical();
        }

        public static void HelpBox(string text, MessageType type, bool smallIcon = false)
        {
            Vector2 iconSize = EditorGUIUtility.GetIconSize();
            if (smallIcon) iconSize = new Vector2(20, 20);
            using (new EditorGUIUtility.IconSizeScope(iconSize))
            {
                GUIContent iconWText = EditorGUIUtility.TrTextContentWithIcon(text, type);
                GUIStyle labelStyle = EditorStyles.helpBox;
                labelStyle.alignment = TextAnchor.MiddleLeft;
                labelStyle.richText = true;
                EditorGUILayout.LabelField(new GUIContent(), iconWText, labelStyle);
            }
        }

        public static void DrawRelativeProperties(SerializedProperty root, float width)
        {
            var childrens = root.GetVisibleChildrens();

            foreach (var childProperty in childrens)
            {
                float height = EditorGUI.GetPropertyHeight(childProperty, true);

                Rect rect = GUILayoutUtility.GetRect(1f, height);
                rect.xMin += width;
                EditorGUI.PropertyField(rect, childProperty, true);
                EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
            }
        }

        public static IEnumerable<SerializedProperty> GetVisibleChildrens(this SerializedProperty serializedProperty)
        {
            SerializedProperty currentProperty = serializedProperty.Copy();
            SerializedProperty nextSiblingProperty = serializedProperty.Copy();
            {
                nextSiblingProperty.NextVisible(false);
            }

            if (currentProperty.NextVisible(true))
            {
                do
                {
                    if (SerializedProperty.EqualContents(currentProperty, nextSiblingProperty))
                        break;

                    yield return currentProperty;
                }
                while (currentProperty.NextVisible(false));
            }
        }

        public static void TrHelpIconText(string message, string icon, bool rich = false)
        {
            GUIStyle style = new GUIStyle(EditorStyles.helpBox)
            {
                richText = rich
            };

            EditorGUILayout.LabelField(GUIContent.none, EditorGUIUtility.TrTextContentWithIcon(" " + message, icon), style, new GUILayoutOption[0]);
        }

        public static void TrHelpIconText(Rect rect, string message, string icon, bool rich = false)
        {
            GUIStyle style = new GUIStyle(EditorStyles.helpBox)
            {
                richText = rich
            };

            EditorGUI.LabelField(rect, GUIContent.none, EditorGUIUtility.TrTextContentWithIcon(" " + message, icon), style);
        }

        public static void TrHelpIconText(string message, MessageType messageType, bool rich = false, bool space = true)
        {
            string icon = string.Empty;

            GUIStyle style = new GUIStyle(EditorStyles.helpBox)
            {
                richText = rich
            };

            switch (messageType)
            {
                case MessageType.Info:
                    icon = "console.infoicon.sml";
                    break;
                case MessageType.Warning:
                    icon = "console.warnicon.sml";
                    break;
                case MessageType.Error:
                    icon = "console.erroricon.sml";
                    break;
            }

            if (!string.IsNullOrEmpty(icon))
            {
                string text = space ? " " + message : message;
                EditorGUILayout.LabelField(GUIContent.none, EditorGUIUtility.TrTextContentWithIcon(text, icon), style, new GUILayoutOption[0]);
            }
            else
            {
                EditorGUILayout.LabelField(GUIContent.none, EditorGUIUtility.TrTextContent(message), style, new GUILayoutOption[0]);
            }
        }

        public static void TrHelpIconText(Rect rect, string message, MessageType messageType, bool rich = false, bool space = true)
        {
            string icon = string.Empty;

            GUIStyle style = new GUIStyle(EditorStyles.helpBox)
            {
                richText = rich
            };

            switch (messageType)
            {
                case MessageType.Info:
                    icon = "console.infoicon.sml";
                    break;
                case MessageType.Warning:
                    icon = "console.warnicon.sml";
                    break;
                case MessageType.Error:
                    icon = "console.erroricon.sml";
                    break;
            }

            if (!string.IsNullOrEmpty(icon))
            {
                string text = space ? " " + message : message;
                EditorGUI.LabelField(rect, GUIContent.none, EditorGUIUtility.TrTextContentWithIcon(text, icon), style);
            }
            else
            {
                EditorGUI.LabelField(rect, GUIContent.none, EditorGUIUtility.TrTextContent(message), style);
            }
        }

        public static void TrIconText(string message, string icon, GUIStyle style, bool rich = false, bool space = true)
        {
            style.richText = rich;
            string text = space ? " " + message : message;
            EditorGUILayout.LabelField(GUIContent.none, EditorGUIUtility.TrTextContentWithIcon(text, icon), style, new GUILayoutOption[0]);
        }

        public static void TrIconText(string message, MessageType messageType, GUIStyle style, bool rich = false, bool space = true)
        {
            string icon = string.Empty;
            style.richText = rich;

            switch (messageType)
            {
                case MessageType.Info:
                    icon = "console.infoicon.sml";
                    break;
                case MessageType.Warning:
                    icon = "console.warnicon.sml";
                    break;
                case MessageType.Error:
                    icon = "console.erroricon.sml";
                    break;
            }

            if (!string.IsNullOrEmpty(icon))
            {
                string text = space ? " " + message : message;
                EditorGUILayout.LabelField(GUIContent.none, EditorGUIUtility.TrTextContentWithIcon(text, icon), style, new GUILayoutOption[0]);
            }
            else
            {
                EditorGUILayout.LabelField(GUIContent.none, EditorGUIUtility.TrTextContent(message), style, new GUILayoutOption[0]);
            }
        }

        public static void TrIconText(Rect rect, string message, MessageType messageType, GUIStyle style, bool rich = false, bool space = true)
        {
            string icon = string.Empty;
            style.richText = rich;

            switch (messageType)
            {
                case MessageType.Info:
                    icon = "console.infoicon.sml";
                    break;
                case MessageType.Warning:
                    icon = "console.warnicon.sml";
                    break;
                case MessageType.Error:
                    icon = "console.erroricon.sml";
                    break;
            }

            if (!string.IsNullOrEmpty(icon))
            {
                string text = space ? " " + message : message;
                EditorGUI.LabelField(rect, GUIContent.none, EditorGUIUtility.TrTextContentWithIcon(text, icon), style);
            }
            else
            {
                EditorGUI.LabelField(rect, GUIContent.none, EditorGUIUtility.TrTextContent(message), style);
            }
        }

        public static void DrawLocaleSelector(Rect pos, SerializedProperty prop, GUIContent label, bool disabled = false)
        {
            bool localizationExist = false;

            if (prop.propertyType != SerializedPropertyType.String)
            {
                throw new System.Exception($"Wrong property type of \"{prop.propertyPath}\" field!");
            }

#if TW_LOCALIZATION_PRESENT
            localizationExist = LocalizationSystem.HasReference;
#endif

            pos.xMax -= EditorGUIUtility.singleLineHeight;
            using (new EditorGUI.DisabledGroupScope(disabled))
            {
                EditorGUI.PropertyField(pos, prop, label);
            }

            Rect selectRect = pos;
            selectRect.width = EditorGUIUtility.singleLineHeight;
            selectRect.x = pos.xMax;

            GUIContent icon = localizationExist ? Styles.DLLinked : Styles.DLUnLinked;

            using (new EditorGUI.DisabledGroupScope(!localizationExist))
            {
                if (GUI.Button(selectRect, icon, Styles.IconButton))
                {
#if TW_LOCALIZATION_PRESENT
                    EditorWindow browser = EditorWindow.GetWindow<LocalizationUtility.LocaleKeyBrowserWindow>(true, "Localization Key Browser", true);
                    browser.minSize = new Vector2(320, 500);
                    browser.maxSize = new Vector2(320, 500);

                    LocalizationUtility.LocaleKeyBrowserWindow keyBrowser = browser as LocalizationUtility.LocaleKeyBrowserWindow;
                    keyBrowser.OnSelectKey += key =>
                    {
                        prop.stringValue = key;
                        prop.serializedObject.ApplyModifiedProperties();
                    };

                    keyBrowser.Show(LocalizationSystem.Instance);
#endif
                }
            }
        }
    }
}