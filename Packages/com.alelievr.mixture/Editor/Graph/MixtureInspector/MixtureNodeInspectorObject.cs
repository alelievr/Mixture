using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine.Rendering;

namespace Mixture
{
    [CustomEditor(typeof(MixtureNodeInspectorObject))]
    public class MixtureNodeInspectorObjectEditor : NodeInspectorObjectEditor
    {
        internal enum CompareMode
        {
            SideBySide,
            OnionSkin,
            Difference,
        }

        // TODO
        // internal enum PreviewMode
        // {
        //     Color,
        //     Normal,
        //     Height,
        // }

        Event e => Event.current;

        internal MixtureNodeInspectorObject mixtureInspector;

        Dictionary<BaseNode, VisualElement> nodeInspectorCache = new Dictionary<BaseNode, VisualElement>();
        internal List<MixtureNode> nodeWithPreviews = new List<MixtureNode>();

        NodeInspectorSettingsPopupWindow comparisonWindow;

        Material previewMaterial;

        // Preview params:
        Vector2 middleClickMousePosition;
        Vector2 middleClickCameraPosition;
        Vector2 positionOffset;
        Vector2 positionOffsetTarget;
        Vector2 lastMousePosition;
        float zoomTarget = 1;
        float zoom = 1;
        float zoomSpeed = 20f;
        double timeSinceStartup, latestTime, deltaTime;

        const float maxZoom = 512;
        const float minZoom = 0.05f;
        const int buttonWidth = 25;

        float compareSlider = 0.5f;
        bool compareEnabled = false;
        bool lockFirstPreview = false;
        bool lockSecondPreview = false;
        MixtureNode firstLockedPreviewTarget;
        MixtureNode secondLockedPreviewTarget;
        Vector2 shaderPos;
        bool needsRepaint;

        // Preview settings
        internal FilterMode filterMode;
        internal float exposure;
        internal PreviewChannels channels = PreviewChannels.RGB;
        internal CompareMode compareMode;
        internal float mipLevel;

        VisualTreeAsset nodeInspectorFoldout;

        protected override void OnHeaderGUI()
        {
            base.OnHeaderGUI();
            // EditorGUILayout.LabelField(new GUIContent("Node Inspector", MixtureUtils.icon));
        }

        protected override void OnEnable()
        {
            mixtureInspector = target as MixtureNodeInspectorObject;

            nodeInspectorFoldout = Resources.Load("UI Blocks/InspectorNodeFoldout") as VisualTreeAsset;
            nodeInspectorFoldout.hideFlags = HideFlags.HideAndDontSave;

            base.OnEnable();

            mixtureInspector.pinnedNodeUpdate += UpdateNodeInspectorList;
            previewMaterial = new Material(Shader.Find("Hidden/MixtureInspectorPreview")) { hideFlags = HideFlags.HideAndDontSave };
        }

        protected override void OnDisable()
        {
            mixtureInspector.pinnedNodeUpdate -= UpdateNodeInspectorList;
        }

        protected override void UpdateNodeInspectorList()
        {
            selectedNodeList.Clear();
            nodeWithPreviews.Clear();

            if (mixtureInspector.selectedNodes.Count == 0 && mixtureInspector.pinnedNodes.Count == 0)
                selectedNodeList.Add(placeholder);

            // Selection first
            foreach (var nodeView in mixtureInspector.selectedNodes)
            {
                var view = CreateMixtureNodeBlock(nodeView, true);
                view.AddToClassList("SelectedNode");
                selectedNodeList.Add(view);

                if (nodeView.nodeTarget is MixtureNode n && n.hasPreview & n.previewTexture != null)
                    nodeWithPreviews.Add(n);
            }

            // Then pinned nodes                
            foreach (var nodeView in mixtureInspector.pinnedNodes)
            {
                var view = CreateMixtureNodeBlock(nodeView, false);
                view.AddToClassList("PinnedView");
                selectedNodeList.Add(view);
                
                if (nodeView.nodeTarget is MixtureNode n && n.hasPreview & n.previewTexture != null)
                    nodeWithPreviews.Add(n);
            }
        }

        VisualElement CreateMixtureNodeBlock(BaseNodeView nodeView, bool selection)
        {
            var nodeFoldout = nodeInspectorFoldout.CloneTree();
            var foldout = nodeFoldout.Q("Foldout") as Foldout;
            var nodeName = nodeFoldout.Q("NodeName") as Label;
            var nodePreview = nodeFoldout.Q("NodePreview") as VisualElement;
            var foldoutContainer = nodeFoldout.Q("FoldoutContainer");
            var unpinButton = nodeFoldout.Q("UnpinButton") as VisualElement;

            unpinButton.RegisterCallback<MouseDownEvent>((e) =>
            {
                if (selection)
                {
                    mixtureInspector.selectedNodes.Remove(nodeView);
                    nodeView.owner.RemoveFromSelection(nodeView);
                    UpdateNodeInspectorList();
                }
                else
                {
                    (nodeView as MixtureNodeView)?.UnpinView();
                }
            });

            unpinButton.style.unityBackgroundImageTintColor = selection ? (Color)new Color32(15, 134, 255, 255) : (Color)new Color32(245, 127, 23, 255);

            foldout.text = nodeView.nodeTarget.name;

            var tmp = nodeView.controlsContainer;
            nodeView.controlsContainer = foldoutContainer;
            nodeView.Enable(true);
            nodeView.controlsContainer.AddToClassList("NodeControls");
            var block = nodeView.controlsContainer;
            nodeView.controlsContainer = tmp;

            return nodeFoldout;
        }

        public override bool HasPreviewGUI() => nodeWithPreviews.Count > 0;

        static GUILayoutOption buttonLayout = GUILayout.Width(buttonWidth);

        // TODO: move interactive preview to another class
        public override void OnPreviewSettings()
        {
            var options = nodeWithPreviews.Select(n => n.name).ToArray();

            if (options.Length == 0)
            {
                EditorGUILayout.PrefixLabel("Nothing Selected");
                return;
            }

            if (GUILayout.Button(MixtureEditorUtils.fitIcon, EditorStyles.toolbarButton, buttonLayout))
                Fit();

            GUILayout.Space(2);
            
            if (!lockFirstPreview)
                firstLockedPreviewTarget = nodeWithPreviews.FirstOrDefault();
            if (!lockSecondPreview)
            {
                if (lockFirstPreview)
                    secondLockedPreviewTarget = nodeWithPreviews.FirstOrDefault();
                else
                    secondLockedPreviewTarget = nodeWithPreviews.Count > 1 ? nodeWithPreviews[1] : nodeWithPreviews.FirstOrDefault();
            }

            GUILayout.Label(firstLockedPreviewTarget.name, EditorStyles.toolbarButton);
            lockFirstPreview = GUILayout.Toggle(lockFirstPreview, GetLockIcon(lockFirstPreview), EditorStyles.toolbarButton, buttonLayout);
            if (compareEnabled)
            {
                compareMode = (CompareMode)EditorGUILayout.Popup((int)compareMode, new string[]{"|", "🍕", "-"}, EditorStyles.toolbarButton, buttonLayout);
                GUILayout.Label(secondLockedPreviewTarget.name, EditorStyles.toolbarButton);
                lockSecondPreview = GUILayout.Toggle(lockSecondPreview, GetLockIcon(lockSecondPreview), EditorStyles.toolbarButton, buttonLayout);
            }

            compareEnabled = GUILayout.Toggle(compareEnabled, MixtureEditorUtils.compareIcon, EditorStyles.toolbarButton, buttonLayout);

            GUILayout.Space(2);

            if (GUILayout.Button(MixtureEditorUtils.settingsIcon, EditorStyles.toolbarButton, buttonLayout))
            {
                comparisonWindow = new NodeInspectorSettingsPopupWindow(this);
                UnityEditor.PopupWindow.Show(new Rect(EditorGUIUtility.currentViewWidth - NodeInspectorSettingsPopupWindow.width, -NodeInspectorSettingsPopupWindow.height, 0, 0), comparisonWindow);
            }
        }

        Texture2D GetLockIcon(bool locked) => locked ? MixtureEditorUtils.lockClose : MixtureEditorUtils.lockOpen;

        void Fit()
        {
            zoomTarget = 1;
            positionOffsetTarget = Vector2.zero;
        }

        public override void OnInteractivePreviewGUI(Rect previewRect, GUIStyle background)
        {
            HandleZoomAndPan(previewRect);

            if (firstLockedPreviewTarget?.previewTexture != null && e.type == EventType.Repaint)
            {
                MixtureUtils.SetupDimensionKeyword(previewMaterial, firstLockedPreviewTarget.previewTexture.dimension);

                // Set texture property based on the dimension
                MixtureUtils.SetTextureWithDimension(previewMaterial, "_MainTex0", firstLockedPreviewTarget.previewTexture);
                MixtureUtils.SetTextureWithDimension(previewMaterial, "_MainTex1", secondLockedPreviewTarget.previewTexture);

                previewMaterial.SetFloat("_ComparisonSlider", compareSlider);
                previewMaterial.SetFloat("_ComparisonEnabled", compareEnabled ? 1 : 0);
                previewMaterial.SetFloat("_CompareMode", (int)compareMode);
                previewMaterial.SetFloat("_PreviewMip", mipLevel);
                previewMaterial.SetFloat("_YRatio", previewRect.height / previewRect.width);
                previewMaterial.SetFloat("_Zoom", zoom);
                previewMaterial.SetVector("_Pan", shaderPos / previewRect.size);
                previewMaterial.SetFloat("_FilterMode", (int)filterMode);
                previewMaterial.SetFloat("_Exp", exposure);
                previewMaterial.SetVector("_TextureSize", new Vector4(firstLockedPreviewTarget.previewTexture.width, firstLockedPreviewTarget.previewTexture.height, 1.0f / firstLockedPreviewTarget.previewTexture.width, 1.0f / firstLockedPreviewTarget.previewTexture.height));
                previewMaterial.SetVector("_Channels", MixtureEditorUtils.GetChannelsMask(channels));
                EditorGUI.DrawPreviewTexture(previewRect, Texture2D.whiteTexture, previewMaterial);
            }
            else
                EditorGUI.DrawRect(previewRect, new Color(1, 0, 1, 1));
        }

        Vector2 LocalToWorld(Vector2 pos)
        {
            pos *= zoom;
            pos += positionOffset;
            return pos;
        }

        bool IsMoveMouse(int button, EventModifiers mods) => e.button == 2 || e.button == 0;

        public void HandleZoomAndPan(Rect previewRect)
        {
            timeSinceStartup = EditorApplication.timeSinceStartup;
            deltaTime = timeSinceStartup - latestTime;
            latestTime = timeSinceStartup;

            lastMousePosition = e.mousePosition;
            switch (e.type)
            {
                case EventType.MouseDown:
                    if (IsMoveMouse(e.button, e.modifiers))
                    {
                        middleClickMousePosition = lastMousePosition;
                        middleClickCameraPosition = positionOffset;
                        needsRepaint = true;
                    }
                    break;
                case EventType.MouseDrag:
                    if (IsMoveMouse(e.button, e.modifiers))
                    {
                        positionOffset = middleClickCameraPosition + (lastMousePosition - middleClickMousePosition);
                        positionOffsetTarget = positionOffset;
                        needsRepaint = true;
                    }
                    if (e.button == 1)
                    {
                        float pos = (e.mousePosition.x - positionOffsetTarget.x) / zoom / (float)previewRect.width % 1;
                        compareSlider = pos < 0 ? pos + 1 : pos;
                        needsRepaint = true;
                    }
                    break;
                case EventType.ScrollWheel:
                    float delta = Mathf.Clamp(1f + Mathf.Abs((float)e.delta.y * 0.1f), 0.1f, 2f);
                    delta = e.delta.y > 0 ? 1f / delta : delta;
                    if (zoomTarget * delta > maxZoom || zoomTarget * delta < minZoom)
                        delta = 1;
                    zoomTarget *= delta;
                    positionOffsetTarget = lastMousePosition + (positionOffsetTarget - lastMousePosition) * delta;
                    break;
            }

            float zoomDiff = zoomTarget - zoom;
            Vector2 offsetDiff = positionOffsetTarget - positionOffset;

            if (Mathf.Abs(zoomDiff) > 0.01f || offsetDiff.magnitude > 0.01f)
            {
                zoom += zoomDiff * zoomSpeed * (float)deltaTime;
                positionOffset += offsetDiff * zoomSpeed * (float)deltaTime;
                needsRepaint = true;
            }
            else
            {
               zoom = zoomTarget;
               positionOffset = positionOffsetTarget;
            }
            shaderPos = LocalToWorld(Vector2.zero);

            if (needsRepaint)
            {
                Repaint();
                needsRepaint = false;
            }
        }
    }

    public class MixtureNodeInspectorObject : NodeInspectorObject
    {
        public event Action pinnedNodeUpdate;

        public List<BaseNodeView> pinnedNodes = new List<BaseNodeView>();

        public void AddPinnedView(BaseNodeView view)
        {
            Selection.activeObject = this;
            if (!pinnedNodes.Any(b => b.nodeTarget.GUID == view.nodeTarget.GUID))
            {
                pinnedNodes.Add(view);
                pinnedNodeUpdate?.Invoke();
            }
        }

        public void RemovePinnedView(BaseNodeView view)
        {
            Selection.activeObject = this;
            if (pinnedNodes.Remove(view))
                pinnedNodeUpdate?.Invoke();
        }

        public override void NodeViewRemoved(BaseNodeView view)
        {
            pinnedNodes.Remove(view);
            base.NodeViewRemoved(view);
        }
    }
}