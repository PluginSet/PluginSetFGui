#if FAIRYGUI_DRAGONBONES

/**
 * The MIT License (MIT)
 *
 * Copyright (c) 2012-2017 DragonBones team and other contributors
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of
 * this software and associated documentation files (the "Software"), to deal in
 * the Software without restriction, including without limitation the rights to
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
 * the Software, and to permit persons to whom the Software is furnished to do so,
 * subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
 * FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
 * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
 * IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Reflection;
using System.IO;
using PluginSet.Core;

namespace DragonBones
{
    [InitializeOnLoad]
    public class DragonBonesIcons : Editor
    {

        static Texture2D /*textureBone,*/ textureArmature, textureImg, textureMesh/* , textureIk */;
        static bool isInited = false;
        static DragonBonesIcons()
        {
            Initialize();
        }

        static void Initialize()
        {
            if (isInited)
            {
                return;
            }

            var packageName = "com.pluginset.fairygui";
            var editorGUIPath = Path.Combine("Packages", packageName, "Editor", "Resources", "DragonBonesGUI");

            textureArmature = EditorGUIUtility.Load( Path.Combine(editorGUIPath, "icon-skeleton.png")) as Texture2D;
            textureImg      = EditorGUIUtility.Load( Path.Combine(editorGUIPath, "icon-image.png")) as Texture2D;
            textureMesh     = EditorGUIUtility.Load( Path.Combine(editorGUIPath, "icon-mesh.png")) as Texture2D;
            
            EditorApplication.hierarchyWindowItemOnGUI -= HierarchyIconsOnGUI;
            EditorApplication.hierarchyWindowItemOnGUI += HierarchyIconsOnGUI;
            isInited = true;
        }

        static void HierarchyIconsOnGUI(int instanceId, Rect selectionRect)
        {
            GameObject go = (GameObject)EditorUtility.InstanceIDToObject(instanceId);
            if (!go)
            {
                return;
            }

            Rect rect = new Rect(selectionRect.x - 25f, selectionRect.y + 2, 15f, 15f);

            if (go.GetComponent<UnityArmatureComponent>())
            {
                rect.x = selectionRect.x + selectionRect.width - 15f;
                GUI.Label(rect, textureArmature);
                return;
            }

            UnityUGUIDisplay ugui = go.GetComponent<UnityUGUIDisplay>();
            if (ugui && ugui.sharedMesh)
            {
                if (ugui.sharedMesh.vertexCount == 4)
                {
                    GUI.Label(rect, textureImg);
                }
                else
                {
                    GUI.Label(rect, textureMesh);
                }
                return;
            }

            MeshFilter mf = go.GetComponent<MeshFilter>();
            if (mf && mf.sharedMesh &&
                mf.transform.parent != null &&
                mf.transform.parent.GetComponent<UnityArmatureComponent>() != null)
            {
                if (mf.sharedMesh.vertexCount == 4)
                {
                    GUI.Label(rect, textureImg);
                }
                else
                {
                    GUI.Label(rect, textureMesh);
                }
                return;
            }
        }
    }

}
#endif