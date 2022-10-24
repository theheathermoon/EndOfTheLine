using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using ThunderWire.Editors;
using HFPS.Systems;

namespace HFPS.Editors
{
    [CustomEditor(typeof(InteractiveItem)), CanEditMultipleObjects]
    public class InteractiveItemEditor : Editor
    {
        private Inventory inventory;
        private readonly bool[] foldout = new bool[5];

        private SerializedProperty m_itemType;
        private SerializedProperty m_examineType;
        private SerializedProperty m_examineRotate;
        private SerializedProperty m_messageType;
        private SerializedProperty m_disableType;

        private SerializedProperty m_messageTips;

        private SerializedProperty m_titleOrMsg;
        private SerializedProperty m_examineTitle;
        private SerializedProperty m_paperText;

        private SerializedProperty m_titleMsgKey;
        private SerializedProperty m_examineKey;
        private SerializedProperty m_itemTitle;

        private SerializedProperty m_inventoryID;
        private SerializedProperty m_switcherID;

        private SerializedProperty m_invExpandAmount;
        private SerializedProperty m_itemAmount;
        private SerializedProperty m_paperFontSize;
        private SerializedProperty m_examineDistance;
        private SerializedProperty m_messageTime;

        private SerializedProperty m_useInvTitle;
        private SerializedProperty m_useItemTitle;
        private SerializedProperty m_useDefaultHint;
        private SerializedProperty m_autoShortcut;
        private SerializedProperty m_autoSwitch;
        private SerializedProperty m_interactShowTitle;
        private SerializedProperty m_allowExamineTake;
        private SerializedProperty m_floatingIcon;
        private SerializedProperty m_faceCamera;

        private SerializedProperty m_mouseClickPickup;
        private SerializedProperty m_enableCursor;

        private SerializedProperty m_pickupSound;
        private SerializedProperty m_pickupVolume;
        private SerializedProperty m_examineSound;
        private SerializedProperty m_examineVolume;

        private SerializedProperty m_faceRotation;
        private SerializedProperty m_itemCustomData;
        private SerializedProperty m_collidersDisable;
        private SerializedProperty m_collidersEnable;
        private SerializedProperty m_allowedInteracts;

        private void OnEnable()
        {
            m_itemType = serializedObject.FindProperty("itemType");
            m_examineType = serializedObject.FindProperty("examineType");
            m_examineRotate = serializedObject.FindProperty("examineRotate");
            m_messageType = serializedObject.FindProperty("messageType");
            m_disableType = serializedObject.FindProperty("disableType");

            m_messageTips = serializedObject.FindProperty("messageTips");

            m_titleOrMsg = serializedObject.FindProperty("titleOrMsg");
            m_examineKey = serializedObject.FindProperty("examineKey");
            m_paperText = serializedObject.FindProperty("paperText");

            m_titleMsgKey = serializedObject.FindProperty("titleMsgKey");
            m_examineTitle = serializedObject.FindProperty("examineTitle");
            m_itemTitle = serializedObject.FindProperty("itemTitle");

            m_inventoryID = serializedObject.FindProperty("inventoryID");
            m_switcherID = serializedObject.FindProperty("switcherID");

            m_invExpandAmount = serializedObject.FindProperty("invExpandAmount");
            m_itemAmount = serializedObject.FindProperty("itemAmount");
            m_paperFontSize = serializedObject.FindProperty("paperFontSize");
            m_examineDistance = serializedObject.FindProperty("examineDistance");
            m_messageTime = serializedObject.FindProperty("messageTime");

            m_useInvTitle = serializedObject.FindProperty("useInvTitle");
            m_useItemTitle = serializedObject.FindProperty("useItemTitle");
            m_useDefaultHint = serializedObject.FindProperty("useDefaultHint");
            m_autoShortcut = serializedObject.FindProperty("autoShortcut");
            m_autoSwitch = serializedObject.FindProperty("autoSwitch");
            m_interactShowTitle = serializedObject.FindProperty("interactShowTitle");
            m_allowExamineTake = serializedObject.FindProperty("allowExamineTake");
            m_floatingIcon = serializedObject.FindProperty("floatingIcon");
            m_faceCamera = serializedObject.FindProperty("faceCamera");

            m_mouseClickPickup = serializedObject.FindProperty("mouseClickPickup");
            m_enableCursor = serializedObject.FindProperty("enableCursor");

            m_pickupSound = serializedObject.FindProperty("pickupSound");
            m_pickupVolume = serializedObject.FindProperty("pickupVolume");
            m_examineSound = serializedObject.FindProperty("examineSound");
            m_examineVolume = serializedObject.FindProperty("examineVolume");

            m_faceRotation = serializedObject.FindProperty("faceRotation");
            m_itemCustomData = serializedObject.FindProperty("itemCustomData");
            m_collidersDisable = serializedObject.FindProperty("collidersDisable");
            m_collidersEnable = serializedObject.FindProperty("collidersEnable");
            m_allowedInteracts = serializedObject.FindProperty("allowedInteracts");

            if (Inventory.HasReference)
                inventory = Inventory.Instance;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            InteractiveItem.ItemType itemType = (InteractiveItem.ItemType)m_itemType.enumValueIndex;
            InteractiveItem.ExamineType examineType = (InteractiveItem.ExamineType)m_examineType.enumValueIndex;
            InteractiveItem.MessageType messageType = (InteractiveItem.MessageType)m_messageType.enumValueIndex;

            if (itemType == InteractiveItem.ItemType.OnlyExamine && examineType == InteractiveItem.ExamineType.None)
            {
                m_examineType.enumValueIndex = (int)InteractiveItem.ExamineType.Object;
            }

            if (messageType == InteractiveItem.MessageType.Message && m_useInvTitle.boolValue || itemType != InteractiveItem.ItemType.InventoryItem)
            {
                m_useInvTitle.boolValue = false;
            }

            EditorGUILayout.PropertyField(m_itemType);

            if (itemType != InteractiveItem.ItemType.BackpackExpand)
                EditorGUILayout.PropertyField(m_examineType);

            if (itemType != InteractiveItem.ItemType.OnlyExamine)
            {
                EditorGUILayout.PropertyField(m_messageType);
                EditorGUILayout.PropertyField(m_disableType);
                EditorGUILayout.Space();

                if (itemType == InteractiveItem.ItemType.InventoryItem || itemType == InteractiveItem.ItemType.SwitcherItem)
                {
                    DrawItemSelector("Inventory Item");
                }
            }

            EditorGUILayout.Space();

            if (HFPS_GameManager.LocalizationEnabled)
                EditorGUILayout.HelpBox("Global Localization is enabled, all Titles/Messages will be used as the Localization Key.", MessageType.Info);

            if (foldout[0] = DrawHeader(foldout[0], "Item Settings"))
            {
                EditorGUI.indentLevel++;

                if (itemType != InteractiveItem.ItemType.OnlyExamine)
                {
                    switch (itemType)
                    {
                        case InteractiveItem.ItemType.InventoryItem:
                            EditorGUILayout.PropertyField(m_itemAmount);
                            EditorGUILayout.PropertyField(m_autoShortcut);
                            break;

                        case InteractiveItem.ItemType.SwitcherItem:
                            EditorGUILayout.PropertyField(m_switcherID);
                            EditorGUILayout.PropertyField(m_itemAmount);
                            EditorGUILayout.PropertyField(m_autoSwitch);
                            EditorGUILayout.PropertyField(m_autoShortcut);
                            break;

                        case InteractiveItem.ItemType.BackpackExpand:
                            EditorGUILayout.PropertyField(m_invExpandAmount, new GUIContent("Expand Amount"));
                            break;
                    }
                }

                if ((itemType == InteractiveItem.ItemType.InventoryItem || itemType == InteractiveItem.ItemType.SwitcherItem) &&
                    (messageType == InteractiveItem.MessageType.ItemName || messageType == InteractiveItem.MessageType.PickupHint))
                    EditorGUILayout.PropertyField(m_useInvTitle, new GUIContent("Use Inventory Title"));

                EditorGUILayout.PropertyField(m_interactShowTitle, new GUIContent("Show Interact Title"));
                EditorGUILayout.PropertyField(m_floatingIcon, new GUIContent("Floating Icon"));

                EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
                EditorGUI.indentLevel--;
            }

            if (messageType != InteractiveItem.MessageType.None && itemType != InteractiveItem.ItemType.OnlyExamine &&
                itemType != InteractiveItem.ItemType.BackpackExpand && !m_useInvTitle.boolValue || messageType == InteractiveItem.MessageType.PickupHint)
            {
                if (foldout[1] = DrawHeader(foldout[1], "Title/Message Settings"))
                {
                    EditorGUI.indentLevel++;

                    if (!m_useInvTitle.boolValue || itemType == InteractiveItem.ItemType.GenericItem)
                    {
                        string prefixName = messageType.ToString();

                        if (messageType == InteractiveItem.MessageType.ItemName)
                            prefixName = "Item Title";
                        else if (messageType == InteractiveItem.MessageType.Message)
                            prefixName = "Message";
                        else if (messageType == InteractiveItem.MessageType.PickupHint)
                        {
                            if (!m_useDefaultHint.boolValue)
                                prefixName = "Pickup Hint";
                            else
                                prefixName = "Item Title";
                        }

                        if (!HFPS_GameManager.LocalizationEnabled)
                        {
                            EditorGUILayout.PropertyField(m_titleOrMsg, new GUIContent(prefixName));
                        }
                        else
                        {
                            Rect rect = GUILayoutUtility.GetRect(1, 20);
                            EditorUtils.DrawLocaleSelector(rect, m_titleMsgKey, new GUIContent(prefixName + " Key"));
                        }
                    }
                    if (messageType == InteractiveItem.MessageType.PickupHint)
                    {
                        EditorGUILayout.PropertyField(m_useDefaultHint);
                        EditorGUILayout.PropertyField(m_messageTime);
                        EditorGUILayout.PropertyField(m_messageTips, true);
                    }

                    EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
                    EditorGUI.indentLevel--;
                }
            }

            if (examineType != InteractiveItem.ExamineType.None && itemType != InteractiveItem.ItemType.BackpackExpand)
            {
                if (foldout[2] = DrawHeader(foldout[2], "Examine Settings"))
                {
                    EditorGUI.indentLevel++;

                    if (examineType == InteractiveItem.ExamineType.Object || examineType == InteractiveItem.ExamineType.AdvancedObject)
                    {
                        if (!m_useItemTitle.boolValue)
                        {
                            if (!HFPS_GameManager.LocalizationEnabled)
                            {
                                EditorGUILayout.PropertyField(m_examineTitle, new GUIContent("Examine Name"));
                            }
                            else
                            {
                                Rect rect = GUILayoutUtility.GetRect(1, 20);
                                EditorUtils.DrawLocaleSelector(rect, m_examineKey, new GUIContent("Examine Name Key"));
                            }

                            EditorGUILayout.Space();
                        }
                    }
                    else if (examineType == InteractiveItem.ExamineType.Paper)
                    {
                        if (!HFPS_GameManager.LocalizationEnabled)
                        {
                            EditorGUILayout.PropertyField(m_paperText, new GUIContent("Paper Text"));
                        }
                        else
                        {
                            Rect rect = GUILayoutUtility.GetRect(1, 20);
                            EditorUtils.DrawLocaleSelector(rect, m_examineKey, new GUIContent("Paper Text Key"));
                        }

                        EditorGUILayout.PropertyField(m_paperFontSize, new GUIContent("Font Size"));
                        EditorGUILayout.Space();
                    }

                    EditorGUILayout.PropertyField(m_examineRotate);
                    EditorGUILayout.PropertyField(m_examineDistance, new GUIContent("Examine Distance"));
                    EditorGUILayout.PropertyField(m_useItemTitle, new GUIContent("Examine Item Title"));
                    EditorGUILayout.PropertyField(m_allowExamineTake, new GUIContent("Allow Examine Take"));

                    EditorGUILayout.PropertyField(m_enableCursor);
                    if (m_enableCursor.boolValue)
                    {
                        EditorGUILayout.PropertyField(m_mouseClickPickup, new GUIContent("Click Interact"));
                    }

                    EditorGUILayout.PropertyField(m_faceCamera, new GUIContent("Face to Camera"));
                    if (m_faceCamera.boolValue)
                    {
                        EditorGUILayout.PropertyField(m_faceRotation);
                    }

                    if (examineType == InteractiveItem.ExamineType.AdvancedObject)
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.PropertyField(m_allowedInteracts, new GUIContent("Allowed Interacts"), true);
                        EditorGUILayout.PropertyField(m_collidersDisable, new GUIContent("Colliders Disable"), true);
                        EditorGUILayout.PropertyField(m_collidersEnable, new GUIContent("Colliders Enable"), true);
                    }

                    EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
                    EditorGUI.indentLevel--;
                }
            }

            if (itemType == InteractiveItem.ItemType.InventoryItem || itemType == InteractiveItem.ItemType.SwitcherItem)
            {
                if (foldout[3] = DrawHeader(foldout[3], "Custom Data Settings"))
                {
                    EditorGUI.indentLevel++;

                    EditorGUILayout.PropertyField(m_itemCustomData, new GUIContent("Item Custom Data"), true);

                    EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
                    EditorGUI.indentLevel--;
                }
            }

            bool soundFlag1 = itemType != InteractiveItem.ItemType.OnlyExamine;
            bool soundFlag2 = examineType != InteractiveItem.ExamineType.None;

            if (soundFlag1 || soundFlag2)
            {
                if (foldout[4] = DrawHeader(foldout[4], "Sound Settings"))
                {
                    EditorGUI.indentLevel++;

                    if (soundFlag1)
                    {
                        EditorGUILayout.PropertyField(m_pickupSound);
                        EditorGUILayout.PropertyField(m_pickupVolume);
                        EditorGUILayout.Space();
                    }
                    if (soundFlag2)
                    {
                        EditorGUILayout.PropertyField(m_examineSound);
                        EditorGUILayout.PropertyField(m_examineVolume);
                    }

                    EditorGUI.indentLevel--;
                }
            }

            Repaint();
            serializedObject.ApplyModifiedProperties();
        }

        private bool DrawHeader(bool state, string label)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            state = EditorUtils.DrawFoldoutHeader(20f, label, state, true, true);
            EditorGUILayout.EndVertical();

            return state;
        }

        void DrawItemSelector(string headerLabel)
        {
            Item item = inventory != null ? inventory.GetNewItem(m_inventoryID.intValue) : null;
            GUIContent linkIcon = inventory != null ? EditorUtils.Styles.Linked : EditorUtils.Styles.UnLinked;

            if (item != null)
            {
                m_itemTitle.stringValue = item.Title;
            }

            EditorGUILayout.BeginVertical(GUI.skin.box);

            var headerRect = GUILayoutUtility.GetRect(1f, 20f);
            EditorGUI.LabelField(headerRect, headerLabel, EditorStyles.boldLabel);

            var linkBtn = headerRect;
            linkBtn.width = EditorGUIUtility.singleLineHeight;
            linkBtn.x += headerRect.width - (EditorGUIUtility.singleLineHeight * 2);
            linkBtn.y += EditorGUIUtility.standardVerticalSpacing;

            var databaseBtn = linkBtn;
            databaseBtn.x += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            using (new EditorGUI.DisabledGroupScope(inventory == null || inventory.inventoryDatabase == null))
            {
                if (GUI.Button(databaseBtn, EditorUtils.Styles.Database, EditorUtils.Styles.IconButton))
                {
                    InventoryScriptableEditor.OpenDatabaseEditor(inventory.inventoryDatabase);
                }
            }

            using (new EditorGUI.DisabledGroupScope(inventory == null))
            {
                if (GUI.Button(linkBtn, linkIcon, EditorUtils.Styles.IconButton))
                {
                    EditorWindow browser = EditorWindow.GetWindow<ItemBrowserWindow>(true, "Inventroy Item Browser", true);
                    browser.minSize = new Vector2(320, 500);
                    browser.maxSize = new Vector2(320, 500);

                    ItemBrowserWindow window = (ItemBrowserWindow)browser;
                    window.Show(inventory);

                    window.OnSelectItem += value =>
                    {
                        m_inventoryID.intValue = value;
                        serializedObject.ApplyModifiedProperties();
                    };

                    browser.Show();
                }
            }

            var infoRect = GUILayoutUtility.GetRect(1f, 20f);

            GUIContent lightIcon = new GUIContent();
            if (item != null)
            {
                lightIcon = EditorUtils.Styles.GreenLight;
                lightIcon.tooltip = "Linked";
            }
            else
            {
                lightIcon = EditorUtils.Styles.RedLight;
                lightIcon.tooltip = "UnLinked";
            }

            var lightRect = infoRect;
            lightRect.width = EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(lightRect, lightIcon, GUIContent.none);

            var textRect = infoRect;
            textRect.xMin += lightRect.width + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.LabelField(textRect, $"[{m_inventoryID.intValue}] {m_itemTitle.stringValue}", EditorStyles.miniBoldLabel);

            if (inventory == null)
            {
                EditorUtils.TrHelpIconText("Inventory script not found!", MessageType.Error);
            }
            else if (item == null)
            {
                EditorUtils.TrHelpIconText($"Inventory Item ID [{m_inventoryID.intValue}] not found!", MessageType.Error);
            }

            EditorGUILayout.EndVertical();
        }

        internal class ItemBrowserWindow : EditorWindow
        {
            protected Inventory inventory;

            protected SearchField searchField;
            protected string searchString;
            protected Vector2 scrollPos;

            public event System.Action<int> OnSelectItem;

            public void Show(Inventory inventory)
            {
                this.inventory = inventory;
                searchField = new SearchField();
            }

            void OnGUI()
            {
                var searchRect = GUILayoutUtility.GetRect(1f, 20f);
                searchRect.y += 6f;
                searchRect.x += 3f;
                searchRect.xMax -= 4f;
                searchString = searchField.OnGUI(searchRect, searchString);
                EditorGUILayout.Space();

                var searchResult = GetSearchResult(searchString);

                EditorGUILayout.LabelField($"Result [{searchResult.Length}]", EditorStyles.boldLabel);
                EditorGUILayout.BeginVertical(GUI.skin.box);
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

                if (searchResult.Length > 0)
                {
                    foreach (var key in searchResult)
                    {
                        var rect = GUILayoutUtility.GetRect(1f, 20f);

                        EditorGUILayout.BeginHorizontal();
                        EditorGUI.LabelField(rect, $"[{key.ID}] {key.Title}");

                        var assignBtn = rect;
                        var assignTxt = new GUIContent("Assign");
                        var scaleX = EditorStyles.miniButton.CalcSize(assignTxt).x;
                        assignBtn.xMin = rect.xMax - scaleX - 5f;

                        if (GUI.Button(assignBtn, assignTxt, EditorStyles.miniButton))
                        {
                            OnSelectItem?.Invoke(key.ID);
                            Close();
                        }

                        EditorGUILayout.EndHorizontal();
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("The specified key could not be found!");
                }

                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();
            }

            InventoryScriptable.ItemMapper[] GetSearchResult(string search)
            {
                if (!string.IsNullOrEmpty(search))
                {
                    if (int.TryParse(search, out int i))
                    {
                        return (from item in inventory.inventoryDatabase.ItemDatabase
                                where item.ID == i
                                select item).ToArray();
                    }
                    else
                    {
                        string m_search = search.ToLower().Replace(" ", "");

                        return (from item in inventory.inventoryDatabase.ItemDatabase
                                let title = item.Title.ToLower().Replace(" ", "")
                                where title.Contains(m_search)
                                select item).ToArray();
                    }
                }

                return inventory.inventoryDatabase.ItemDatabase.ToArray();
            }
        }
    }
}