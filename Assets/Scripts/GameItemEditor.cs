using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static UnityEngine.GraphicsBuffer;
using UnityEditor.UIElements;
using Unity.VisualScripting;
using System.IO;

public class GameItemEditor : EditorWindow
{
    private enum SortMode
    {
        ByName,
        ByID,
    }
    [SerializeField] private GameItemData selectedGameItem = null;
    [SerializeField] private int selectedIndex = -1;
    private List<GameItemData> gameItems = new List<GameItemData>();
    private string assetPath = $"Assets/GameItem/";
    private SortMode sortMode = SortMode.ByName;

    // UI Elements
    private VisualElement itemsTab;
    private static VisualTreeAsset gameItemRowTemplate;
    private ListView gameItemListView;
    private static Sprite gameItemDefaultIcon = null;
    private ScrollView detailSection;
    private VisualElement largeDisplayIcon;
    private Button sortButton;
    private GameItemData activeGameItem;


    [MenuItem("Tools/Open Game Item Editor")]
    public static void ShowWindow()
    {
        GameItemEditor wnd = GetWindow<GameItemEditor>();
        wnd.titleContent = new GUIContent("Game Item Editor");
    }

    private void CreateGUI()
    {
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI Toolkit/GameItemEditor.uxml");
        VisualElement rootFromUXML = visualTree.Instantiate();
        rootVisualElement.Add(rootFromUXML);

        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/UI Toolkit/GameItemEditor.uss");
        rootVisualElement.styleSheets.Add(styleSheet);

        gameItemRowTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI Toolkit/GameItemRow.uxml");
        gameItemDefaultIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI Toolkit/default-icon.png");

        itemsTab = rootVisualElement.Q<VisualElement>("ItemsTab");

        rootVisualElement.Q<Button>("Btn_AddItem").clicked += AddGameItem;
        rootVisualElement.Q<Button>("Btn_DeleteItem").clicked += DeleteGameItem;
        sortButton = rootVisualElement.Q<Button>("Btn_SortItem");
        sortButton.clicked += SortGameItem;

        CreateDetailSection();
        LoadAllGameItems();
        GenerateListView();
    }

    private void CreateDetailSection()
    {
        detailSection = rootVisualElement.Q<ScrollView>("ScrollView_Details");
        largeDisplayIcon = detailSection.Q<VisualElement>("Icon");
        detailSection.style.visibility = Visibility.Hidden;

        detailSection.Q<TextField>("GameItemName").RegisterValueChangedCallback(evt =>
        {
            string error = AssetDatabase.RenameAsset($"{assetPath}/{activeGameItem.name}.asset", evt.newValue);
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError(error);
            }
            gameItemListView.Rebuild();
        });
        detailSection.Q<ObjectField>("IconPicker").RegisterValueChangedCallback(evt =>
        {
            if (evt.newValue == null) return;
            Sprite newSprite = evt.newValue as Sprite;
            activeGameItem.icon = newSprite ? newSprite : gameItemDefaultIcon;
            largeDisplayIcon.style.backgroundImage = newSprite ? newSprite.texture : gameItemDefaultIcon.texture;
            gameItemListView.Rebuild();
        });
        detailSection.Q<EnumField>("GameItemUsageType").RegisterValueChangedCallback(evt =>
        {
            GameItemUsageType usageType = (GameItemUsageType)evt.newValue;
            switch(usageType)
            {
                case GameItemUsageType.None:
                    detailSection.Q<VisualElement>("EquipmentRow").style.display = DisplayStyle.None;
                    break;
                case GameItemUsageType.Equippable:
                    detailSection.Q<VisualElement>("EquipmentRow").style.display = DisplayStyle.Flex;
                    break;
            }
        });
    }

    private void LoadAllGameItems()
    {
        gameItems.Clear();
        var gameItemGUID = AssetDatabase.FindAssets("t:GameItem");
        foreach (var GUID in gameItemGUID)
        {
            var gameItemPath = AssetDatabase.GUIDToAssetPath(GUID);
            var gameItem = AssetDatabase.LoadAssetAtPath<GameItemData>(gameItemPath);
            gameItems.Add(gameItem);
        }
    }

    private void GenerateListView()
    {
        gameItemListView = new ListView(gameItems, 45, MakeListItem, BindListItem);
        gameItemListView.selectionType = SelectionType.Single;
        itemsTab.Add(gameItemListView);
        gameItemListView.onSelectionChange += ListView_onSelectionChange;
    }

    private VisualElement MakeListItem()
    {
        return gameItemRowTemplate.CloneTree();
    }

    private void BindListItem(VisualElement element, int index)
    {
        if (gameItems[index] && gameItems[index].icon)
            element.Q<VisualElement>("Icon").style.backgroundImage = gameItems[index].icon.texture;
        else if(gameItemDefaultIcon)
            element.Q<VisualElement>("Icon").style.backgroundImage = gameItemDefaultIcon.texture;
        // element.Q<VisualElement>("Icon").style.backgroundImage = gameItems[index].icon ? gameItems[index].icon.texture : gameItemDefaultIcon.texture;
        element.Q<Label>("Name").text = gameItems[index].name;
        element.Q<Label>("Id").text = gameItems[index].id.ToString();

        element.AddManipulator(new ContextualMenuManipulator((ContextualMenuPopulateEvent evt) =>
        {
            evt.menu.AppendAction("Remove", (x) =>
            {
                var gameItem = gameItems[index];
                AssetDatabase.DeleteAsset($"{assetPath}/{gameItem.name}.asset");
                AssetDatabase.Refresh();
                detailSection.style.visibility = Visibility.Hidden;
                gameItems.RemoveAt(index);
                gameItemListView.Rebuild();
            });
        }));
    }

    private void AddGameItem()
    {
        GameItemData gameItem = (GameItemData)ScriptableObject.CreateInstance(typeof(GameItemData));
        string fileName = "New Item";
        string path = $"{assetPath}/{fileName}.asset";
        int fileNumber = 0;
        while (File.Exists(path))
        {
            fileNumber++;
            fileName = $"New Item_{fileNumber}";
            path = $"{assetPath}/{fileName}.asset";

        }
        gameItem.displayName = fileName;
        gameItem.id = (ulong)gameItems.Count + 1;
        AssetDatabase.CreateAsset(gameItem, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        gameItems.Add(gameItem);
        gameItemListView.Rebuild();
    }

    private void DeleteGameItem()
    {
        string path = AssetDatabase.GetAssetPath(activeGameItem);
        AssetDatabase.DeleteAsset(path);
        AssetDatabase.Refresh();
        gameItems.Remove(activeGameItem);
        gameItemListView.Rebuild();
        detailSection.style.visibility = Visibility.Hidden;
    }

    private void SortGameItem()
    {
        switch (sortMode)
        {
            case SortMode.ByName:
                sortButton.text = "ID";
                sortMode = SortMode.ByID;
                gameItems.Sort((item1, item2) =>
                {
                    return item1.id.CompareTo(item2.id);
                });
                break;
            case SortMode.ByID:
                sortButton.text = "NAME";
                sortMode = SortMode.ByName;
                gameItems.Sort((item1, item2) =>
                {
                    return item1.name.CompareTo(item2.name);
                });
                break;
            default:
                break;
        }
        gameItemListView.Rebuild();

    }

    private void ListView_onSelectionChange(IEnumerable<object> selectedItems)
    {
        activeGameItem = (GameItemData)selectedItems.First();
        SerializedObject so = new SerializedObject(activeGameItem);
        detailSection.Bind(so);
        if(activeGameItem.icon)
        {
            largeDisplayIcon.style.backgroundImage = activeGameItem.icon.texture;
        }
        else
        {
            largeDisplayIcon.style.backgroundImage = null;
        }

        detailSection.Q<VisualElement>("EquipmentRow").style.display = DisplayStyle.None;
        if (activeGameItem.usageType == GameItemUsageType.Equippable)
        {
            detailSection.Q<VisualElement>("EquipmentRow").style.display = DisplayStyle.Flex;
        }
        
        detailSection.style.visibility = Visibility.Visible;
    }
}