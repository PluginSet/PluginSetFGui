//#define NEED_PACKAGES

using FairyGUI;
using FairyGUIEditor;
using UnityEditor;
using UnityEngine;

namespace PluginSet.FGUI.Editor
{
    [CustomEditor(typeof(PluginSetFGUIConfig))]
    public class PluginSetFGUIConfigEditor : UnityEditor.Editor
    {
        string[] propertyToExclude;
        bool itemsFoldout;
#if NEED_PACKAGES
        bool packagesFoldOut;
        int errorState;
#endif
        
        SerializedProperty scaleMode;
        SerializedProperty screenMatchMode;
        SerializedProperty designResolutionX;
        SerializedProperty designResolutionY;
        SerializedProperty fallbackScreenDPI;
        SerializedProperty defaultSpriteDPI;
        SerializedProperty constantScaleFactor;
        SerializedProperty ignoreOrientation;
        
        private const float kButtonWidth = 18f;

        void OnEnable()
        {
            propertyToExclude = new string[] { "m_Script", "Items", "PreloadPackages"
                    , "scaleMode", "screenMatchMode", "designResolutionX", "designResolutionY"
                    , "fallbackScreenDPI", "defaultSpriteDPI", "constantScaleFactor", "ignoreOrientation"};

            itemsFoldout = EditorPrefs.GetBool("itemsFoldOut");
#if NEED_PACKAGES
            packagesFoldOut = EditorPrefs.GetBool("packagesFoldOut");
#endif
            
            scaleMode = serializedObject.FindProperty("scaleMode");
            screenMatchMode = serializedObject.FindProperty("screenMatchMode");
            designResolutionX = serializedObject.FindProperty("designResolutionX");
            designResolutionY = serializedObject.FindProperty("designResolutionY");
            fallbackScreenDPI = serializedObject.FindProperty("fallbackScreenDPI");
            defaultSpriteDPI = serializedObject.FindProperty("defaultSpriteDPI");
            constantScaleFactor = serializedObject.FindProperty("constantScaleFactor");
            ignoreOrientation = serializedObject.FindProperty("ignoreOrientation");

#if NEED_PACKAGES
            errorState = 0;
#endif
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawPropertiesExcluding(serializedObject, propertyToExclude);
            
            EditorGUILayout.PropertyField(scaleMode);
            if ((UIContentScaler.ScaleMode)scaleMode.enumValueIndex == UIContentScaler.ScaleMode.ScaleWithScreenSize)
            {
                EditorGUILayout.PropertyField(designResolutionX);
                EditorGUILayout.PropertyField(designResolutionY);
                EditorGUILayout.PropertyField(screenMatchMode);
                EditorGUILayout.PropertyField(ignoreOrientation);
            }
            else if ((UIContentScaler.ScaleMode)scaleMode.enumValueIndex == UIContentScaler.ScaleMode.ConstantPhysicalSize)
            {
                EditorGUILayout.PropertyField(fallbackScreenDPI);
                EditorGUILayout.PropertyField(defaultSpriteDPI);
            }
            else
                EditorGUILayout.PropertyField(constantScaleFactor);
            
            PluginSetFGUIConfig config = (PluginSetFGUIConfig)target;

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            itemsFoldout = EditorGUILayout.Foldout(itemsFoldout, "Config Items");
            if (EditorGUI.EndChangeCheck())
                EditorPrefs.SetBool("itemsFoldOut", itemsFoldout);
            EditorGUILayout.EndHorizontal();

            bool modified = false;

            if (itemsFoldout)
            {
                Undo.RecordObject(config, "Items");

                int len = config.Items.Count;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Add");
                UIConfig.ConfigKey selectedKey = (UIConfig.ConfigKey)EditorGUILayout.EnumPopup((System.Enum)UIConfig.ConfigKey.PleaseSelect);

                if (selectedKey != UIConfig.ConfigKey.PleaseSelect)
                {
                    int index = (int)selectedKey;

                    if (index > len - 1)
                    {
                        for (int i = len; i < index; i++)
                            config.Items.Add(new UIConfig.ConfigValue());

                        UIConfig.ConfigValue value = new UIConfig.ConfigValue();
                        value.valid = true;
                        UIConfig.SetDefaultValue(selectedKey, value);
                        config.Items.Add(value);
                    }
                    else
                    {
                        UIConfig.ConfigValue value = config.Items[index];
                        if (value == null)
                        {
                            value = new UIConfig.ConfigValue();
                            value.valid = true;
                            UIConfig.SetDefaultValue(selectedKey, value);
                            config.Items[index] = value;
                        }
                        else if (!value.valid)
                        {
                            value.valid = true;
                            UIConfig.SetDefaultValue(selectedKey, value);
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();

                for (int i = 0; i < len; i++)
                {
                    UIConfig.ConfigValue value = config.Items[i];
                    if (value == null || !value.valid)
                        continue;

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel(((UIConfig.ConfigKey)i).ToString());
                    switch ((UIConfig.ConfigKey)i)
                    {
                        case UIConfig.ConfigKey.ClickDragSensitivity:
                        case UIConfig.ConfigKey.DefaultComboBoxVisibleItemCount:
                        case UIConfig.ConfigKey.DefaultScrollStep:
                        case UIConfig.ConfigKey.TouchDragSensitivity:
                        case UIConfig.ConfigKey.TouchScrollSensitivity:
                        case UIConfig.ConfigKey.InputCaretSize:
                            value.i = EditorGUILayout.IntField(value.i);
                            break;

                        case UIConfig.ConfigKey.ButtonSound:
                        case UIConfig.ConfigKey.GlobalModalWaiting:
                        case UIConfig.ConfigKey.HorizontalScrollBar:
                        case UIConfig.ConfigKey.LoaderErrorSign:
                        case UIConfig.ConfigKey.PopupMenu:
                        case UIConfig.ConfigKey.PopupMenu_seperator:
                        case UIConfig.ConfigKey.TooltipsWin:
                        case UIConfig.ConfigKey.VerticalScrollBar:
                        case UIConfig.ConfigKey.WindowModalWaiting:
                        case UIConfig.ConfigKey.DefaultFont:
                            value.s = EditorGUILayout.TextField(value.s);
                            break;

                        case UIConfig.ConfigKey.DefaultScrollBounceEffect:
                        case UIConfig.ConfigKey.DefaultScrollTouchEffect:
                        case UIConfig.ConfigKey.RenderingTextBrighterOnDesktop:
                        case UIConfig.ConfigKey.AllowSoftnessOnTopOrLeftSide:
                        case UIConfig.ConfigKey.DepthSupportForPaintingMode:
                            value.b = EditorGUILayout.Toggle(value.b);
                            break;

                        case UIConfig.ConfigKey.ButtonSoundVolumeScale:
                            value.f = EditorGUILayout.Slider(value.f, 0, 1);
                            break;

                        case UIConfig.ConfigKey.ModalLayerColor:
                        case UIConfig.ConfigKey.InputHighlightColor:
                            value.c = EditorGUILayout.ColorField(value.c);
                            break;

                        case UIConfig.ConfigKey.Branch:
                            EditorGUI.BeginChangeCheck();
                            value.s = EditorGUILayout.TextField(value.s);
                            if (EditorGUI.EndChangeCheck())
                                modified = true;
                            break;
                    }

                    if (GUILayout.Button(new GUIContent("X", "Delete Item"), EditorStyles.miniButtonRight, GUILayout.Width(30)))
                    {
                        config.Items[i].Reset();
                        UIConfig.SetDefaultValue((UIConfig.ConfigKey)i, config.Items[i]);
                        modified = true;
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }

#if NEED_PACKAGES
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            packagesFoldOut = EditorGUILayout.Foldout(packagesFoldOut, "Preload Packages");
            if (EditorGUI.EndChangeCheck())
                EditorPrefs.SetBool("packagesFoldOut", packagesFoldOut);
            EditorGUILayout.EndHorizontal();

            if (packagesFoldOut)
            {
                Undo.RecordObject(config, "PreloadPackages");

                EditorToolSet.LoadPackages();

                if (EditorToolSet.packagesPopupContents != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel("Add");
                    int selected = EditorGUILayout.Popup(EditorToolSet.packagesPopupContents.Length - 1, EditorToolSet.packagesPopupContents);
                    EditorGUILayout.EndHorizontal();

                    if (selected != EditorToolSet.packagesPopupContents.Length - 1)
                    {
                        UIPackage pkg = UIPackage.GetPackages()[selected];
                        string tmp = pkg.assetPath.ToLower();
                        int pos = tmp.LastIndexOf("resources/");
                        if (pos != -1)
                        {
                            string packagePath = pkg.assetPath.Substring(pos + 10);
                            if (config.PreloadPackages.IndexOf(packagePath) == -1)
                                config.PreloadPackages.Add(packagePath);

                            errorState = 0;
                        }
                        else
                        {
                            errorState = 10;
                        }
                    }
                }

                if (errorState > 0)
                {
                    errorState--;
                    EditorGUILayout.HelpBox("Package is not in resources folder.", MessageType.Warning);
                }

                int cnt = config.PreloadPackages.Count;
                int pi = 0;
                while (pi < cnt)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel("" + pi + ".");
                    config.PreloadPackages[pi] = EditorGUILayout.TextField(config.PreloadPackages[pi]);
                    if (GUILayout.Button(new GUIContent("X", "Delete Item"), EditorStyles.miniButtonRight, GUILayout.Width(30)))
                    {
                        config.PreloadPackages.RemoveAt(pi);
                        cnt--;
                    }
                    else
                        pi++;
                    EditorGUILayout.EndHorizontal();
                }
            }
            else
                errorState = 0;
#endif

            serializedObject.ApplyModifiedProperties();
            Unused(modified);
        }

        private void Unused(object _)
        {
            
        }
    }
}