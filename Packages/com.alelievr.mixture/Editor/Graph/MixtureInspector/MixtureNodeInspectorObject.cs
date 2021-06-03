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
        const float maxZoom = 512;
        const float minZoom = 0.05f;
        const int buttonWidth = 25;

        Event e => Event.current;

        internal MixtureNodeInspectorObject mixtureInspector;

        Dictionary<BaseNode, VisualElement> nodeInspectorCache = new Dictionary<BaseNode, VisualElement>();
        internal List<MixtureNode> nodeWithPreviews = new List<MixtureNode>();

        NodeInspectorSettingsPopupWindow comparisonWindow;

        Material previewMaterial;

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

            // There is a really weird issue where the resources.Load returns null when building a player
            if (nodeInspectorFoldout == null)
                return;

            nodeInspectorFoldout.hideFlags = HideFlags.HideAndDontSave;

            base.OnEnable();

            mixtureInspector.pinnedNodeUpdate += UpdateNodeInspectorList;
            previewMaterial = new Material(Shader.Find("Hidden/MixtureInspectorPreview")) { hideFlags = HideFlags.HideAndDontSave };

            // Workaround because UIElements is not able to correctly detect mouse enter / leave events :(
            var repaint = root.schedule.Execute(() => {
                Repaint();
            }).Every(16);
            root.Insert(0, new IMGUIContainer(() => {
                if (selectedNodeList.localBound.Contains(Event.current.mousePosition))
                    repaint.Resume();
                else
                {
                    // Unselect all nodes:
                    foreach (var view in mixtureInspector.selectedNodes)
                        view.RemoveFromClassList("highlight");
                    foreach (var view in mixtureInspector.pinnedNodes)
                        view.RemoveFromClassList("highlight");
                    repaint.Pause();
                }
            }));
            // End workaround

            // Handle the always refresh option
            root.Add(new IMGUIContainer(() => {
                if (!Application.runInBackground && !UnityEditorInternal.InternalEditorUtility.isApplicationActive && mixtureInspector.alwaysRefresh)
                    return;

                if (mixtureInspector.alwaysRefresh)
                {
                    if (Event.current.type == EventType.Layout)
                        Repaint();
                }
            }));

            Fit();
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

                if (nodeView.nodeTarget is MixtureNode n && n.hasPreview && n.previewTexture != null)
                    nodeWithPreviews.Add(n);
            }

            // Then pinned nodes                
            foreach (var nodeView in mixtureInspector.pinnedNodes)
            {
                var view = CreateMixtureNodeBlock(nodeView, false);
                view.AddToClassList("PinnedView");
                selectedNodeList.Add(view);
                
                if (nodeView.nodeTarget is MixtureNode n && n.hasPreview && n.previewTexture != null)
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
                nodeView.RemoveFromClassList("highlight");
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

            nodeFoldout.RegisterCallback<MouseEnterEvent>(e => {
                nodeView.AddToClassList("highlight");
            });
            nodeFoldout.RegisterCallback<MouseLeaveEvent>(e => {
                nodeView.RemoveFromClassList("highlight");
            });

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
            
            if (!mixtureInspector.lockFirstPreview)
                mixtureInspector.firstLockedPreviewTarget = nodeWithPreviews.FirstOrDefault();
            if (!mixtureInspector.lockSecondPreview)
            {
                if (mixtureInspector.lockFirstPreview)
                    mixtureInspector.secondLockedPreviewTarget = nodeWithPreviews.FirstOrDefault();
                else
                    mixtureInspector.secondLockedPreviewTarget = nodeWithPreviews.Count > 1 ? nodeWithPreviews[1] : nodeWithPreviews.FirstOrDefault();
            }

            GUILayout.Label(mixtureInspector.firstLockedPreviewTarget.name, EditorStyles.toolbarButton);
            mixtureInspector.lockFirstPreview = GUILayout.Toggle(mixtureInspector.lockFirstPreview, GetLockIcon(mixtureInspector.lockFirstPreview), EditorStyles.toolbarButton, buttonLayout);
            if (mixtureInspector.compareEnabled)
            {
                var style = EditorStyles.toolbarButton;
                style.alignment = TextAnchor.MiddleLeft;
                mixtureInspector.compareMode = (MixtureNodeInspectorObject.CompareMode)EditorGUILayout.Popup((int)mixtureInspector.compareMode, new string[]{" 1  -  Side By Side", " 2  -  Onion Skin", " 3  -  Difference", " 4  -  Swap"}, style, buttonLayout, GUILayout.Width(24));
                GUILayout.Label(mixtureInspector.secondLockedPreviewTarget.name, EditorStyles.toolbarButton);
                mixtureInspector.lockSecondPreview = GUILayout.Toggle(mixtureInspector.lockSecondPreview, GetLockIcon(mixtureInspector.lockSecondPreview), EditorStyles.toolbarButton, buttonLayout);
            }

            mixtureInspector.compareEnabled = GUILayout.Toggle(mixtureInspector.compareEnabled, MixtureEditorUtils.compareIcon, EditorStyles.toolbarButton, buttonLayout);

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
            // position offset is wrong?
            mixtureInspector.zoomTarget = 1;
            mixtureInspector.positionOffsetTarget = Vector2.zero;
            mixtureInspector.cameraXAxis = 30;
            mixtureInspector.cameraYAxis = 15;
            mixtureInspector.cameraZoom = 6;
        }

        public override void OnInteractivePreviewGUI(Rect previewRect, GUIStyle background)
        {
            HandleZoomAndPan(previewRect);

            if (mixtureInspector.firstLockedPreviewTarget?.previewTexture != null && e.type == EventType.Repaint)
            {
                mixtureInspector.volumeCameraMatrix = Matrix4x4.Rotate(Quaternion.Euler(mixtureInspector.cameraYAxis, mixtureInspector.cameraXAxis, 0));

                MixtureUtils.SetupDimensionKeyword(previewMaterial, mixtureInspector.firstLockedPreviewTarget.previewTexture.dimension);

                // Set texture property based on the dimension
                MixtureUtils.SetTextureWithDimension(previewMaterial, "_MainTex0", mixtureInspector.firstLockedPreviewTarget.previewTexture);
                MixtureUtils.SetTextureWithDimension(previewMaterial, "_MainTex1", mixtureInspector.secondLockedPreviewTarget.previewTexture);

                previewMaterial.SetFloat("_ComparisonSlider", mixtureInspector.compareSlider);
                previewMaterial.SetFloat("_ComparisonSlider3D", mixtureInspector.compareSlider3D);
                previewMaterial.SetVector("_MouseUV", mixtureInspector.mouseUV);
                previewMaterial.SetMatrix("_CameraMatrix", mixtureInspector.volumeCameraMatrix);
                previewMaterial.SetFloat("_CameraZoom", mixtureInspector.cameraZoom);
                previewMaterial.SetFloat("_ComparisonEnabled", mixtureInspector.compareEnabled ? 1 : 0);
                previewMaterial.SetFloat("_CompareMode", (int)mixtureInspector.compareMode);
                previewMaterial.SetFloat("_PreviewMip", mixtureInspector.mipLevel);
                previewMaterial.SetFloat("_YRatio", previewRect.height / previewRect.width);
                previewMaterial.SetFloat("_Zoom", mixtureInspector.zoom);
                previewMaterial.SetVector("_Pan", mixtureInspector.shaderPos / previewRect.size);
                previewMaterial.SetFloat("_FilterMode", (int)mixtureInspector.filterMode);
                previewMaterial.SetFloat("_Exp", mixtureInspector.exposure);
                previewMaterial.SetVector("_TextureSize", new Vector4(mixtureInspector.firstLockedPreviewTarget.previewTexture.width, mixtureInspector.firstLockedPreviewTarget.previewTexture.height, 1.0f / mixtureInspector.firstLockedPreviewTarget.previewTexture.width, 1.0f / mixtureInspector.firstLockedPreviewTarget.previewTexture.height));
                previewMaterial.SetVector("_Channels", MixtureEditorUtils.GetChannelsMask(mixtureInspector.channels));
                previewMaterial.SetFloat("_IsSRGB0", mixtureInspector.firstLockedPreviewTarget is OutputNode o0 && o0.mainOutput.sRGB ? 1 : 0);
                previewMaterial.SetFloat("_IsSRGB1", mixtureInspector.secondLockedPreviewTarget is OutputNode o1 && o1.mainOutput.sRGB ? 1 : 0);
                previewMaterial.SetFloat("_PreserveAspect", mixtureInspector.preserveAspect ? 1 : 0);
                previewMaterial.SetFloat("_Texture3DMode", (int)mixtureInspector.texture3DPreviewMode);
                previewMaterial.SetFloat("_Density", mixtureInspector.texture3DDensity);
                previewMaterial.SetFloat("_SDFOffset", mixtureInspector.texture3DDistanceFieldOffset);
                previewMaterial.SetFloat("_SDFChannel", (int)mixtureInspector.sdfChannel);
                EditorGUI.DrawPreviewTexture(previewRect, Texture2D.whiteTexture, previewMaterial);
            }
            else
                EditorGUI.DrawRect(previewRect, new Color(1, 0, 1, 1));
        }

        Vector2 LocalToWorld(Vector2 pos)
        {
            pos *= mixtureInspector.zoom;
            pos += mixtureInspector.positionOffset;
            return pos;
        }

        bool IsMoveMouse(int button, EventModifiers mods) => e.button == 2 || e.button == 0;

        public void HandleZoomAndPan(Rect previewRect)
        {
            mixtureInspector.timeSinceStartup = EditorApplication.timeSinceStartup;
            mixtureInspector.deltaTime = mixtureInspector.timeSinceStartup - mixtureInspector.latestTime;
            mixtureInspector.latestTime = mixtureInspector.timeSinceStartup;

            mixtureInspector.lastMousePosition = e.mousePosition;
            switch (e.type)
            {
                case EventType.MouseDown:
                    if (IsMoveMouse(e.button, e.modifiers))
                    {
                        mixtureInspector.middleClickMousePosition = mixtureInspector.lastMousePosition;
                        mixtureInspector.middleClickCameraPosition = mixtureInspector.positionOffset;
                        mixtureInspector.needsRepaint = true;
                    }
                    Vector2 t = (e.mousePosition - mixtureInspector.positionOffset) / mixtureInspector.zoom;
                    mixtureInspector.comparisonOffset = Mathf.Floor(t.x / (float)previewRect.width);
                    break;
                case EventType.MouseDrag:
                    if (IsMoveMouse(e.button, e.modifiers))
                    {
                        mixtureInspector.positionOffset = mixtureInspector.middleClickCameraPosition + (mixtureInspector.lastMousePosition - mixtureInspector.middleClickMousePosition);
                        mixtureInspector.positionOffsetTarget = mixtureInspector.positionOffset;
                        mixtureInspector.needsRepaint = true;

                        mixtureInspector.cameraXAxis += e.delta.x / 4.0f;
                        mixtureInspector.cameraYAxis += e.delta.y / 4.0f;
                        mixtureInspector.cameraXAxis = Mathf.Repeat(mixtureInspector.cameraXAxis, 360);
                        mixtureInspector.cameraYAxis = Mathf.Repeat(mixtureInspector.cameraYAxis, 360);
                    }
                    if (e.button == 1)
                    {
                        float pos = (e.mousePosition.x - mixtureInspector.positionOffsetTarget.x) / mixtureInspector.zoom / (float)previewRect.width;
                        mixtureInspector.compareSlider = Mathf.Clamp01(pos - mixtureInspector.comparisonOffset);
                        mixtureInspector.compareSlider3D = Mathf.Clamp01(e.mousePosition.x / (float)previewRect.width);
                        mixtureInspector.needsRepaint = true;
                        mixtureInspector.mouseUV = new Vector2(e.mousePosition.x / previewRect.width, e.mousePosition.y / previewRect.height);
                    }
                    break;
                case EventType.ScrollWheel:
                    float delta = Mathf.Clamp(1f + Mathf.Abs((float)e.delta.y * 0.1f), 0.1f, 2f);
                    delta = e.delta.y > 0 ? 1f / delta : delta;
                    if (mixtureInspector.zoomTarget * delta > maxZoom || mixtureInspector.zoomTarget * delta < minZoom)
                        delta = 1;
                    mixtureInspector.zoomTarget *= delta;
                    mixtureInspector.positionOffsetTarget = mixtureInspector.lastMousePosition + (mixtureInspector.positionOffsetTarget - mixtureInspector.lastMousePosition) * delta;

                    mixtureInspector.cameraZoom *= 1.0f / delta;
                    break;
            }

            float zoomDiff = mixtureInspector.zoomTarget - mixtureInspector.zoom;
            Vector2 offsetDiff = mixtureInspector.positionOffsetTarget - mixtureInspector.positionOffset;

            if (Mathf.Abs(zoomDiff) > 0.01f || offsetDiff.magnitude > 0.01f)
            {
                mixtureInspector.zoom += zoomDiff * mixtureInspector.zoomSpeed * (float)mixtureInspector.deltaTime;
                mixtureInspector.positionOffset += offsetDiff * mixtureInspector.zoomSpeed * (float)mixtureInspector.deltaTime;
                mixtureInspector.needsRepaint = true;
            }
            else
            {
               mixtureInspector.zoom = mixtureInspector.zoomTarget;
               mixtureInspector.positionOffset = mixtureInspector.positionOffsetTarget;
            }
            mixtureInspector.shaderPos = LocalToWorld(Vector2.zero);

            if (mixtureInspector.needsRepaint)
            {
                Repaint();
                mixtureInspector.needsRepaint = false;
            }
        }
    }

    [Serializable]
    public class MixtureNodeInspectorObject : NodeInspectorObject
    {
        internal enum CompareMode
        {
            SideBySide,
            OnionSkin,
            Difference,
            Swap,
        }

        internal enum Texture3DPreviewMode
        {
            Volumetric,
            DistanceFieldNormal,
            DistanceFieldColor,
        }

        internal enum SDFChannel
        {
            R,
            G,
            B,
            A
        }

        // Preview params:
        internal Vector2 middleClickMousePosition;
        internal Vector2 middleClickCameraPosition;
        internal Vector2 positionOffset;
        internal Vector2 positionOffsetTarget;
        internal Vector2 lastMousePosition;
        internal float zoomTarget = 1;
        internal float zoom = 1;
        internal float zoomSpeed = 20f;
        internal double timeSinceStartup, latestTime, deltaTime;

        internal float compareSlider = 0.5f;
        internal float compareSlider3D = 0.5f;
        internal Vector2 mouseUV;
        internal Matrix4x4 volumeCameraMatrix = Matrix4x4.identity;
        internal float cameraZoom;
        internal float cameraXAxis;
        internal float cameraYAxis;
        internal bool compareEnabled = false;
        internal bool lockFirstPreview = false;
        internal bool lockSecondPreview = false;
        internal MixtureNode firstLockedPreviewTarget;
        internal MixtureNode secondLockedPreviewTarget;
        internal Vector2 shaderPos;
        internal bool needsRepaint;

        // Preview settings
        internal FilterMode filterMode;
        internal float exposure;
        internal PreviewChannels channels = PreviewChannels.RGB;
        internal CompareMode compareMode;
        internal bool alwaysRefresh = true;
        internal float mipLevel;
        internal bool preserveAspect = true;
        internal Texture3DPreviewMode texture3DPreviewMode;
        internal float texture3DDensity = 1;
        internal float texture3DDistanceFieldOffset = 0;
        internal SDFChannel sdfChannel = SDFChannel.R;
        internal float comparisonOffset;

        // TODO
        // internal enum PreviewMode
        // {
        //     Color,
        //     Normal,
        //     Height,
        // }

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