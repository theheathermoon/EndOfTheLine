using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using ThunderWire.Editors;
using HFPS.Systems;

#if TW_LOCALIZATION_PRESENT
using ThunderWire.Localization;
using ThunderWire.Localization.Editor;
#endif

namespace HFPS.Editors
{
    public class InventoryEditorWindow : EditorWindow
    {
        private InventoryScriptable Target;

        [SerializeField]
        private TreeViewState ItemsViewState;
        private ItemsTreeView ItemsTreeViewPanel;

        private bool LocalizationExist = false;

        private bool ConfirmSaveChangesIfNeeded()
        {
            if (EditorUtility.IsDirty(Target))
                AssetDatabase.SaveAssets();

            return true;
        }

        private bool EditorWantsToQuit()
        {
            return ConfirmSaveChangesIfNeeded();
        }

        private void OnDestroy()
        {
            ConfirmSaveChangesIfNeeded();
            EditorApplication.wantsToQuit -= EditorWantsToQuit;
        }

        private void OnEnable()
        {
            EditorApplication.wantsToQuit += EditorWantsToQuit;
        }

        public void Show(InventoryScriptable inventory)
        {
            Target = inventory;
            ItemsViewState = new TreeViewState();
            ItemsTreeViewPanel = new ItemsTreeView(ItemsViewState, inventory);

#if TW_LOCALIZATION_PRESENT
            LocalizationExist = LocalizationSystem.HasReference;
#endif
        }

        private void OnGUI()
        {
            Rect toolbarRect = new Rect(0, 0, position.width, 20f);
            EditorGUI.LabelField(toolbarRect, GUIContent.none, EditorStyles.toolbar);

            Rect exportToLoc = toolbarRect;
            exportToLoc.xMin = toolbarRect.xMax - 180f;

            using (new EditorGUI.DisabledGroupScope(!LocalizationExist))
            {
                if (GUI.Button(exportToLoc, "Export Items to Localization", EditorStyles.toolbarButton))
                {
#if TW_LOCALIZATION_PRESENT
                    EditorWindow browser = GetWindow<InventoryItemsExport>(true, "Export Items to Localization Map", true);
                    browser.minSize = new Vector2(600, 200);
                    browser.maxSize = new Vector2(600, 200);
                    ((InventoryItemsExport)browser).Show(Target);
#endif
                }
            }

            Rect treeViewRect = new Rect(5f, 25f, position.width - 10f, position.height - 30f);
            ItemsTreeViewPanel.OnGUI(treeViewRect);
        }

        internal class ItemsTreeView : TreeView
        {
            protected InventoryScriptable target;
            protected SerializedObject serializedObject;

            protected SerializedProperty itemsList;
            protected SerializedProperty localization;

            private List<ItemTreeElement> treeElements;

            protected int selectedIndex;
            protected Vector2 scrollPosition;

            protected bool localizationExist = false;
            protected bool m_InitiateContextMenuOnNextRepaint = false;

            static float Spacing => EditorGUIUtility.standardVerticalSpacing * 2;

            private class ItemTreeElement : TreeViewItem
            {
                public SerializedProperty ItemProperty;

                public SerializedProperty p_Title;
                public SerializedProperty p_Description;
                public SerializedProperty p_ItemType;
                public SerializedProperty p_UseActionType;
                public SerializedProperty p_ItemSprite;
                public SerializedProperty p_DropObject;
                public SerializedProperty p_PackDropObject;

                public SerializedProperty p_ItemToggles;
                public SerializedProperty p_ItemSounds;
                public SerializedProperty p_ItemSettings;
                public SerializedProperty p_UseActionSettings;
                public SerializedProperty p_CombineSettings;
                public SerializedProperty p_LocalizationSettings;

                public ItemTreeElement(int id, int depth, string name, SerializedProperty property) : base(id, depth, name)
                {
                    ItemProperty = property;

                    p_Title = property.FindPropertyRelative("Title");
                    p_Description = property.FindPropertyRelative("Description");
                    p_ItemType = property.FindPropertyRelative("itemType");
                    p_UseActionType = property.FindPropertyRelative("useActionType");
                    p_ItemSprite = property.FindPropertyRelative("itemSprite");
                    p_DropObject = property.FindPropertyRelative("DropObject");
                    p_PackDropObject = property.FindPropertyRelative("PackDropObject");

                    p_ItemToggles = property.FindPropertyRelative("itemToggles");
                    p_ItemSounds = property.FindPropertyRelative("itemSounds");
                    p_ItemSettings = property.FindPropertyRelative("itemSettings");
                    p_UseActionSettings = property.FindPropertyRelative("useActionSettings");
                    p_CombineSettings = property.FindPropertyRelative("combineSettings");
                    p_LocalizationSettings = property.FindPropertyRelative("localizationSettings");
                }

                public ItemTreeElement(int id, int depth, string displayName)
                    : base(id, depth, displayName)
                {
                }
            }

            public ItemsTreeView(TreeViewState viewState, InventoryScriptable inventory) : base(viewState)
            {
                target = inventory;
                serializedObject = new SerializedObject(inventory);

                itemsList = serializedObject.FindProperty("ItemDatabase");
                localization = serializedObject.FindProperty("enableLocalization");

#if TW_LOCALIZATION_PRESENT
            localizationExist = LocalizationSystem.HasReference;
#endif

                Reload();
            }

            private void RebuildDatabase()
            {
                for (int i = 0; i < target.ItemDatabase.Count; i++)
                {
                    target.ItemDatabase[i].ID = i;
                }

                EditorUtility.SetDirty(target);
                serializedObject.ApplyModifiedProperties();
                Reload();
            }

            protected override TreeViewItem BuildRoot()
            {
                serializedObject.Update();
                treeElements = new List<ItemTreeElement>();

                var root = new TreeViewItem(0, -1);
                treeElements.Add(new ItemTreeElement(0, -1, "Root"));

                for (int i = 0; i < itemsList.arraySize; i++)
                {
                    SerializedProperty property = itemsList.GetArrayElementAtIndex(i);
                    SerializedProperty title = property.FindPropertyRelative("Title");

                    ItemTreeElement item = new ItemTreeElement(1 + i, 0, title.stringValue, property);
                    treeElements.Add(item);
                    root.AddChild(item);
                }

                if (root.children == null)
                    root.children = new List<TreeViewItem>();

                return root;
            }

            protected override void ContextClickedItem(int id)
            {
                m_InitiateContextMenuOnNextRepaint = true;
                Repaint();
            }
            protected override bool CanRename(TreeViewItem item) => true;
            protected override void RenameEnded(RenameEndedArgs args)
            {
                string newName = args.newName;

                if (string.IsNullOrEmpty(newName))
                {
                    int count = itemsList.arraySize;
                    newName = count > 0 ? "New Item " + count : "New Item";
                }

                if (args.acceptedRename && !target.ItemDatabase.Any(x => x.Title.Equals(newName)))
                {
                    target.ItemDatabase[args.itemID - 1].Title = newName;
                    RebuildDatabase();
                }
            }
            protected override float GetCustomRowHeight(int row, TreeViewItem item) => 20;
            protected override bool CanMultiSelect(TreeViewItem item) => true;
            protected override bool CanStartDrag(CanStartDragArgs args) => true;
            protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
            {
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.SetGenericData("IDs", args.draggedItemIDs.Select(x => x - 1).ToArray());
                DragAndDrop.SetGenericData("Type", "InventoryItems");
                DragAndDrop.StartDrag("Items");
            }
            protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
            {
                int[] draggedIDs = (int[])DragAndDrop.GetGenericData("IDs");
                string type = (string)DragAndDrop.GetGenericData("Type");

                switch (args.dragAndDropPosition)
                {
                    case DragAndDropPosition.BetweenItems:
                        {
                            if (type.Equals("InventoryItems"))
                            {
                                if (args.performDrop)
                                {
                                    MoveElements(draggedIDs, args.insertAtIndex);
                                }

                                return DragAndDropVisualMode.Move;
                            }
                            return DragAndDropVisualMode.Rejected;
                        }
                    case DragAndDropPosition.UponItem:
                    case DragAndDropPosition.OutsideItems:
                        return DragAndDropVisualMode.Rejected;
                    default:
                        Debug.LogError("Unhandled enum " + args.dragAndDropPosition);
                        return DragAndDropVisualMode.None;
                }
            }

            private void MoveElements(int[] drag, int insert)
            {
                var _tmpItems = (from item in target.ItemDatabase
                                 let i = target.ItemDatabase.IndexOf(item)
                                 where drag.Any(x => x == i)
                                 select item).ToArray();

                for (int i = 0; i < _tmpItems.Count(); i++)
                {
                    var item = _tmpItems[i];
                    int index = target.ItemDatabase.IndexOf(item);
                    int insertTo = insert > index ? insert - 1 : insert;

                    target.ItemDatabase.RemoveAt(index);
                    target.ItemDatabase.Insert(insertTo, item);
                }

                SetSelection(new int[0]);
                RebuildDatabase();
            }

            private void PopUpContextMenu()
            {
                var menu = new GenericMenu();

                menu.AddItem(new GUIContent("Insert Key"), false, () =>
                {
                    AddNewItem(GetSelection()[0]);
                });

                if (GetSelection().Count <= 1)
                {
                    menu.AddItem(new GUIContent("Rename"), false, () =>
                    {
                        BeginRename(treeElements[GetSelection()[0]]);
                    });
                }
                else
                {
                    menu.AddDisabledItem(new GUIContent("Rename"));
                }

                menu.AddItem(new GUIContent("Delete"), false, () =>
                {
                    DeleteSelected();
                });

                menu.ShowAsContext();
            }

            private void AddNewItem(int insert = -1)
            {
                int index = itemsList.arraySize;

                string itemTitle = index > 0 ? "New Item " + index : "New Item";
                var newItem = new InventoryScriptable.ItemMapper() { Title = itemTitle };

                if (insert < 0) target.ItemDatabase.Add(newItem);
                else
                {
                    target.ItemDatabase.Insert(insert, newItem);
                    index = insert;
                }

                RebuildDatabase();

                SetSelection(new List<int>() { index + 1 });
                selectedIndex = index + 1;
            }

            private void DeleteSelected()
            {
                foreach (var index in GetSelection().Select(x => x - 1).OrderByDescending(i => i))
                {
                    target.ItemDatabase.RemoveAt(index);
                }

                selectedIndex = -1;
                SetSelection(new int[0]);

                RebuildDatabase();
            }

            public override void OnGUI(Rect rect)
            {
                Rect treeViewRect = rect;
                treeViewRect.width = 300;

                Rect invHeader = EditorUtils.DrawHeaderWithBorder("Inventory Database", 20, ref treeViewRect, true);

                var addItemButton = invHeader;
                addItemButton.width = EditorGUIUtility.singleLineHeight;
                addItemButton.x += invHeader.width - EditorGUIUtility.singleLineHeight - EditorGUIUtility.standardVerticalSpacing;
                addItemButton.y += EditorGUIUtility.standardVerticalSpacing;

                if (GUI.Button(addItemButton, EditorUtils.Styles.PlusIcon, EditorUtils.Styles.IconButton))
                {
                    AddNewItem();
                }

                Rect itemViewRect = rect;
                itemViewRect.width -= 300 + 2f;
                itemViewRect.x = treeViewRect.width + 8f;

                var selected = GetSelection();
                if (selected.Count == 1)
                {
                    selectedIndex = GetSelection()[0];

                    Rect itemHeader = EditorUtils.DrawHeaderWithBorder("Item View", 20, ref itemViewRect, true);

                    GUIContent idText = new GUIContent("ITEM ID: " + (treeElements[selectedIndex].id - 1));
                    Vector2 idTextSize = EditorStyles.miniBoldLabel.CalcSize(idText);

                    var idRect = itemHeader;
                    idRect.xMin = idRect.xMax - idTextSize.x - EditorGUIUtility.standardVerticalSpacing;
                    idRect.y += EditorGUIUtility.standardVerticalSpacing;
                    EditorGUI.LabelField(idRect, idText, EditorStyles.miniBoldLabel);

                    OnSelectedGUI(itemViewRect);
                }
                else if (selected.Count > 1)
                {
                    selectedIndex = -1;

                    GUIContent text = new GUIContent("Multi-selected items cannot be edited.");
                    Vector2 textSize = EditorStyles.miniBoldLabel.CalcSize(text);

                    Rect multiSelectMsg = itemViewRect;
                    multiSelectMsg.x += (itemViewRect.width / 2) - (textSize.x / 2);
                    multiSelectMsg.y += 50f;

                    EditorGUI.LabelField(multiSelectMsg, text, EditorStyles.miniBoldLabel);
                }

                if (m_InitiateContextMenuOnNextRepaint)
                {
                    m_InitiateContextMenuOnNextRepaint = false;
                    PopUpContextMenu();
                }

                base.OnGUI(treeViewRect);
            }

            private void OnSelectedGUI(Rect rect)
            {
                Rect itemViewArea = rect;
                itemViewArea.y += Spacing;
                itemViewArea.yMax -= Spacing;
                itemViewArea.xMin += Spacing;
                itemViewArea.xMax -= Spacing;

                ItemTreeElement item = treeElements[selectedIndex];

                GUILayout.BeginArea(itemViewArea);
                {
                    scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

                    // draw icon
                    Rect iconRect = GUILayoutUtility.GetRect(100, 100);
                    iconRect.xMax = 100;
                    item.p_ItemSprite.objectReferenceValue = EditorGUI.ObjectField(iconRect, item.p_ItemSprite.objectReferenceValue, typeof(Sprite), false);

                    // draw title field
                    Rect titleRect = GUILayoutUtility.GetRect(1, 20);
                    titleRect.y = 0f;
                    titleRect.xMin = iconRect.xMax + Spacing;

                    EditorGUI.BeginChangeCheck();
                    {
                        EditorGUI.PropertyField(titleRect, item.p_Title, new GUIContent());
                    }
                    if (EditorGUI.EndChangeCheck())
                    {
                        treeElements[selectedIndex].displayName = item.p_Title.stringValue;          
                    }

                    using (new EditorGUI.DisabledGroupScope(localization.boolValue))
                    {
                        // draw description field
                        Rect descriptionRect = titleRect;
                        descriptionRect.y = 20f + Spacing;
                        descriptionRect.height = 80f - Spacing;
                        EditorGUI.PropertyField(descriptionRect, item.p_Description, new GUIContent());
                    }

                    // draw properties
                    EditorGUILayout.Space(-20 + EditorGUIUtility.standardVerticalSpacing);
                    EditorGUILayout.PropertyField(item.p_ItemType);
                    EditorGUILayout.PropertyField(item.p_UseActionType);
                    EditorGUILayout.PropertyField(item.p_DropObject);
                    EditorGUILayout.PropertyField(item.p_PackDropObject);
                    EditorGUILayout.Space(5);

                    // draw foldouts
                    DrawPropertyWithHeader(item.p_ItemToggles, "Item Toggles");
                    DrawPropertyWithHeader(item.p_ItemSounds, "Item Sounds");
                    DrawPropertyWithHeader(item.p_ItemSettings, "Item Settings");
                    DrawPropertyWithHeader(item.p_UseActionSettings, "Use Action Settings");
                    DrawPropertyWithHeader(item.p_CombineSettings, "Combine Settings");
                    DrawLocalizationHeader(item.p_LocalizationSettings, "Localization Settings");

                    EditorGUILayout.EndScrollView();
                }
                GUILayout.EndArea();

                serializedObject.ApplyModifiedProperties();
                Repaint();
            }

            protected override void RowGUI(RowGUIArgs args)
            {
                var item = (ItemTreeElement)args.item;

                var labelRect = args.rowRect;
                labelRect.x += 2f;

                GUIContent label = EditorGUIUtility.TrTextContentWithIcon($" [{item.id - 1}] {item.displayName}", "PreMatCube");
                EditorGUI.LabelField(labelRect, label);
            }

            private void DrawPropertyWithHeader(SerializedProperty property, string label)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                if (property.isExpanded = EditorUtils.DrawFoldoutHeader(20f, label, property.isExpanded, true))
                {
                    EditorGUILayout.Space(Spacing);
                    EditorUtils.DrawRelativeProperties(property, 5f);
                    EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
                }
                EditorGUILayout.EndVertical();
            }

            private void DrawLocalizationHeader(SerializedProperty property, string label)
            {
                var p_TitleKey = property.FindPropertyRelative("titleKey");
                var p_DescriptionKey = property.FindPropertyRelative("descriptionKey");

                EditorGUILayout.BeginVertical(GUI.skin.box);
                if (property.isExpanded = EditorUtils.DrawFoldoutHeader(20f, label, property.isExpanded, true))
                {
                    EditorGUILayout.Space(Spacing);

#if !TW_LOCALIZATION_PRESENT
                    EditorUtils.TrHelpIconText("<b>HFPS Localization System Integration</b> is required in order to translate Title and Description!", MessageType.Warning, true, false);
                    EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
#else
                if (localization.boolValue && localizationExist)
                {
                    EditorUtils.TrHelpIconText("<b>Title</b> and <b>Description</b> will change depending on the current localization.", MessageType.Warning, true, false);
                    EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
                }
#endif

                    using (new EditorGUI.DisabledGroupScope(!localization.boolValue || !localizationExist))
                    {
                        Rect titleKeyRect = GUILayoutUtility.GetRect(1, 20);
                        Rect descKeyRect = GUILayoutUtility.GetRect(1, 20);
                        descKeyRect.y += EditorGUIUtility.standardVerticalSpacing * 2;

                        DrawLocalizationSelector(ref titleKeyRect, p_TitleKey);
                        DrawLocalizationSelector(ref descKeyRect, p_DescriptionKey);

                        EditorGUI.PropertyField(titleKeyRect, p_TitleKey);
                        EditorGUI.PropertyField(descKeyRect, p_DescriptionKey);
                    }

                    EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
                }
                EditorGUILayout.EndVertical();
            }

            private void DrawLocalizationSelector(ref Rect pos, SerializedProperty property)
            {
                pos.xMax -= EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                Rect selectRect = pos;
                selectRect.width = EditorGUIUtility.singleLineHeight;
                selectRect.x = pos.xMax + EditorGUIUtility.standardVerticalSpacing;
                selectRect.y += 0.6f;

                GUIContent Linked = EditorGUIUtility.TrIconContent("d_Linked", "Open Localization System Key Browser");
                GUIContent UnLinked = EditorGUIUtility.TrIconContent("d_Unlinked", "Localization System not found!");

                GUIContent icon = localizationExist ? Linked : UnLinked;

                using (new EditorGUI.DisabledGroupScope(!localization.boolValue && !localizationExist))
                {
                    if (GUI.Button(selectRect, icon, EditorUtils.Styles.IconButton))
                    {
#if TW_LOCALIZATION_PRESENT
                    EditorWindow browser = EditorWindow.GetWindow<LocalizationUtility.LocaleKeyBrowserWindow>(true, "Localization Key Browser", true);
                    browser.minSize = new Vector2(320, 500);
                    browser.maxSize = new Vector2(320, 500);

                    LocalizationUtility.LocaleKeyBrowserWindow keyBrowser = browser as LocalizationUtility.LocaleKeyBrowserWindow;
                    keyBrowser.OnSelectKey += key =>
                    {
                        property.stringValue = key;
                        property.serializedObject.ApplyModifiedProperties();
                    };

                    keyBrowser.Show(LocalizationSystem.Instance);
#endif
                    }
                }
            }
        }
    }
}