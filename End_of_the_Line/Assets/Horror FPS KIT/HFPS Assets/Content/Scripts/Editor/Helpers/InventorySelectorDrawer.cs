using UnityEngine;
using UnityEditor;
using ThunderWire.Editors;
using HFPS.Systems;

namespace HFPS.Editors
{
    [CustomPropertyDrawer(typeof(InventorySelectorAttribute))]
    public class InventorySelectorDrawer : PropertyDrawer
    {
        private Inventory InventoryIns => Inventory.Instance;

        private string itemTitle;
        private Item cachedItem;

        private Item GetItem(int id)
        {
            if(InventoryIns != null)
            {
                foreach (var item in InventoryIns.inventoryDatabase.ItemDatabase)
                {
                    if (item.ID == id)
                        return new Item(item.ID, item);
                }
            }

            return null;
        }

        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            if (prop.propertyType != SerializedPropertyType.Integer)
            {
                EditorUtils.TrHelpIconText($"Wrong property type \"{prop.type}\" expected \"integer\"!", MessageType.Error);
                throw new System.Exception($"Wrong property type of \"{prop.propertyPath}\" field!");
            }

            var target = prop.serializedObject.targetObject;
            string targetID = target.GetInstanceID() + "." + prop.propertyPath;

            GUIContent linkIcon = Inventory.HasReference ? EditorUtils.Styles.Linked : EditorUtils.Styles.UnLinked;
            GUIContent databaseIcon = EditorUtils.Styles.Database;

            if ((cachedItem = GetItem(prop.intValue) ?? null) != null)
            {
                itemTitle = cachedItem.Title;
                linkIcon.tooltip = string.Empty;
                databaseIcon.tooltip = string.Empty;
                EditorPrefs.SetString(targetID, itemTitle);
            }
            else
            {
                linkIcon.tooltip = "Item or Inventory script not found!";
                databaseIcon.tooltip = "Item or Inventory script not found!";

                if (EditorPrefs.HasKey(targetID))
                    itemTitle = EditorPrefs.GetString(targetID);
            }

            EditorGUI.BeginProperty(pos, label, prop);

            Rect bgRect = pos;
            bgRect.xMin = EditorGUIUtility.singleLineHeight;
            bgRect.yMax -= 3;
            GUI.Box(bgRect, new GUIContent(), GUI.skin.box);

            float oldX = pos.x;
            Rect prefixRect = pos;
            prefixRect.x += 4;
            prefixRect.y += 3;

            pos = EditorGUI.PrefixLabel(prefixRect, GUIUtility.GetControlID(FocusType.Passive), label, EditorStyles.boldLabel);

            Rect linkBtn = pos;
            linkBtn.width = EditorGUIUtility.singleLineHeight;
            linkBtn.height = EditorGUIUtility.singleLineHeight;
            linkBtn.x += pos.width - (EditorGUIUtility.singleLineHeight * 2) - 7;
            linkBtn.y += EditorGUIUtility.standardVerticalSpacing;

            Rect databaseBtn = linkBtn;
            databaseBtn.x += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            using (new EditorGUI.DisabledGroupScope(InventoryIns == null || InventoryIns.inventoryDatabase == null))
            {
                if (GUI.Button(databaseBtn, databaseIcon, EditorUtils.Styles.IconButton))
                {
                    InventoryScriptableEditor.OpenDatabaseEditor(InventoryIns.inventoryDatabase);
                }
            }

            using (new EditorGUI.DisabledGroupScope(InventoryIns == null))
            {
                if (GUI.Button(linkBtn, linkIcon, EditorUtils.Styles.IconButton))
                {
                    EditorWindow browser = EditorWindow.GetWindow<InteractiveItemEditor.ItemBrowserWindow>(true, "Inventroy Item Browser", true);
                    browser.minSize = new Vector2(320, 500);
                    browser.maxSize = new Vector2(320, 500);

                    InteractiveItemEditor.ItemBrowserWindow window = (InteractiveItemEditor.ItemBrowserWindow)browser;
                    window.Show(InventoryIns);

                    window.OnSelectItem += value =>
                    {
                        cachedItem = null;
                        prop.intValue = value;
                        prop.serializedObject.ApplyModifiedProperties();
                    };

                    browser.Show();
                }
            }

            GUIContent lightIcon = cachedItem != null ? EditorUtils.Styles.GreenLight : EditorUtils.Styles.RedLight;

            Rect secondLine = pos;
            secondLine.height += EditorGUIUtility.singleLineHeight;
            secondLine.y += EditorGUIUtility.singleLineHeight;
            secondLine.x = oldX;

            Rect lightRect = secondLine;
            lightRect.width = EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(lightRect, lightIcon, GUIContent.none);

            Rect textRect = secondLine;
            textRect.xMax = 500;
            textRect.xMin += lightRect.width + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.LabelField(textRect, $"{prop.intValue} {itemTitle}", EditorStyles.miniBoldLabel);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) + EditorGUIUtility.singleLineHeight + 10;
        }
    }
}