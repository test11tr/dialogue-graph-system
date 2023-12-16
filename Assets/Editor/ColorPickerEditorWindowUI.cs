using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

public class ColorPickerEditorWindowUI : EditorWindow
{
    [SerializeField]
    private StyleSheet m_StyleSheet = default;

    [MenuItem("Kaan/UI/CaseStudy-ColorPickerUI")]
    public static void ShowExample()
    {
        ColorPickerEditorWindowUI wnd = GetWindow<ColorPickerEditorWindowUI>();
        wnd.titleContent = new GUIContent("CaseStudy-ColorPickerUI");
    }

    public void CreateGUI()
    {
        VisualElement container = new VisualElement();
        rootVisualElement.Add(container);
        StyleSheet styleSheet = (StyleSheet) EditorGUIUtility.Load("ColorPickerEditorStyle.uss");
        rootVisualElement.styleSheets.Add(styleSheet);

        Label title = new Label("Color Picker");
        ColorField colorField = new ColorField()
        {
            name = "color-picker"
        };

        container.Add(title);
        container.Add(colorField);

        VisualElement buttonContainer = new VisualElement();
        Button randomColorButton = (Button) CreateButton("Random Color");
        Button resetColorButton = (Button)CreateButton("Reset Color");
        Button copyColorButton = (Button)CreateButton("Copy Color");
        Button pasteColorButton = (Button)CreateButton("Paste Color");

        buttonContainer.Add(randomColorButton);
        buttonContainer.Add(resetColorButton);
        buttonContainer.Add(copyColorButton);
        buttonContainer.Add(pasteColorButton);

        container.Add(buttonContainer);

        buttonContainer.AddToClassList("horizontal-container");
        randomColorButton.AddToClassList("dark-button");
        resetColorButton.AddToClassList("dark-button");
        copyColorButton.AddToClassList("dark-button");
        pasteColorButton.AddToClassList("dark-button");
    }
    
    private VisualElement CreateButton(string text)
    {
        return new Button() { text = text };
    }
}
