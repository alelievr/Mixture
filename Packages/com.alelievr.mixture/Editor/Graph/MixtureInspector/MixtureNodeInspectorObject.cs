using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;
using GraphProcessor;

namespace Mixture
{
    [CustomEditor(typeof(MixtureNodeInspectorObject))]
    public class MixtureNodeInspectorObjectEditor : NodeInspectorObjectEditor
    {
        Event e => Event.current;

        internal MixtureNodeInspectorObject mixtureInspector;

        Dictionary<BaseNode, VisualElement> nodeInspectorCache = new Dictionary<BaseNode, VisualElement>();
        internal List<MixtureNode> nodeWithPreviews = new List<MixtureNode>();

        ComparisonPopupWindow comparisonWindow;

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

        const float maxZoom = 256;

        internal int previewTextureIndex = 0;
        internal int comparisonTextureIndex = 0;
        internal float compareSlider;

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

            base.OnEnable();

            selectedNodeList.styleSheets.Add(Resources.Load<StyleSheet>("MixtureCommon"));
            selectedNodeList.styleSheets.Add(Resources.Load<StyleSheet>("MixtureNodeInspector"));
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

            // Put pinned in first.
            // nodeWithPreviews.Reverse();
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
                    UpdateNodeInspectorList();
                }
                else
                {
                    (nodeView as MixtureNodeView)?.UnpinView();
                }
            });

            unpinButton.style.unityBackgroundImageTintColor = selection ? (Color)new Color32(15, 134, 255, 255) : (Color)new Color32(245, 127, 23, 255);

            // if (nodeView.nodeTarget is MixtureNode n && n.hasPreview)
            //     nodePreview.Add(new Image{ image = n.previewTexture});
            // else
                // nodeFoldout.Remove(nodePreview);

            // nodeName.text = nodeView.nodeTarget.name;
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

        public override void OnPreviewSettings()
        {
            var options = nodeWithPreviews.Select(n => n.name).ToArray();

            if (options.Length == 0)
            {
                EditorGUILayout.PrefixLabel("Nothing Selected");
                return;
            }

            if (GUILayout.Button(MixtureEditorUtils.fitIcon, EditorStyles.toolbarButton, GUILayout.Width(26)))
                Fit();
            
            previewTextureIndex = EditorGUILayout.Popup(previewTextureIndex, options, GUILayout.Width(120));
            if (EditorGUILayout.DropdownButton(new GUIContent(MixtureEditorUtils.compareIcon), FocusType.Passive, GUILayout.Width(40)))
            {
                comparisonWindow = new ComparisonPopupWindow(this);
                UnityEditor.PopupWindow.Show(new Rect(EditorGUIUtility.currentViewWidth - ComparisonPopupWindow.width, -ComparisonPopupWindow.height, 0, 0), comparisonWindow);
            }
        }

        void Fit()
        {
            zoomTarget = 1;
            positionOffsetTarget = Vector2.zero;
        }

        public override void OnInteractivePreviewGUI(Rect previewRect, GUIStyle background)
        {
            Texture previewTexture = null;
            Texture compareTexture = null;

            HandleZoomAndPan(previewRect);

            previewTextureIndex = 0;

            if (previewTextureIndex >= 0 && previewTextureIndex < nodeWithPreviews.Count)
                previewTexture = nodeWithPreviews[previewTextureIndex].previewTexture;
            if (comparisonTextureIndex >= 0 && comparisonTextureIndex < nodeWithPreviews.Count)
                compareTexture = nodeWithPreviews[comparisonTextureIndex].previewTexture;

            if (previewTexture != null && e.type == EventType.Repaint)
            {
                previewMaterial.SetTexture("_MainTex0", previewTexture);
                previewMaterial.SetTexture("_MainTex1", compareTexture);
                previewMaterial.SetFloat("_ComparisonSlider", compareSlider);
                previewMaterial.SetFloat("_Zoom", zoom);
                previewMaterial.SetVector("_Pan", shaderPos / previewRect.size);
                EditorGUI.DrawPreviewTexture(previewRect, previewTexture, previewMaterial);
            }
            else
                EditorGUI.DrawRect(previewRect, new Color(1, 0, 1, 1));
        }

        Vector2 T(Vector2 pos)
        {
            pos *= zoom;
            pos += positionOffset;
            return pos;
        }

        bool IsMiddleMouse(int button, EventModifiers mods) => e.button == 2 || (e.button == 0 && e.modifiers == EventModifiers.Alt);

        public void HandleZoomAndPan(Rect previewRect)
        {
            timeSinceStartup = EditorApplication.timeSinceStartup;
            deltaTime = timeSinceStartup - latestTime;
            latestTime = timeSinceStartup;

            lastMousePosition = e.mousePosition;
            switch (e.type)
            {
                case EventType.MouseDown:
                    if (IsMiddleMouse(e.button, e.modifiers))
                    {
                        middleClickMousePosition = lastMousePosition;
                        middleClickCameraPosition = positionOffset;
                        Repaint();
                    }
                    break;
                case EventType.MouseDrag:
                    if (IsMiddleMouse(e.button, e.modifiers))
                    {
                        positionOffset = middleClickCameraPosition + (lastMousePosition - middleClickMousePosition);
                        positionOffsetTarget = positionOffset;
                        Repaint();
                    }
                    if (e.button == 1)
                    {
                        compareSlider = (e.mousePosition.x - positionOffsetTarget.x) / zoom / (float)previewRect.width % 1;
                        Repaint();
                    }
                    break;
                case EventType.ScrollWheel:
                    float delta = Mathf.Clamp(1f + Mathf.Abs((float)e.delta.y * 0.1f), 0.1f, 2f);
                    delta = e.delta.y > 0 ? 1f / delta : delta;
                    zoomTarget *= delta;
                    if (zoomTarget < maxZoom)
                        positionOffsetTarget = lastMousePosition + (positionOffsetTarget - lastMousePosition) * delta;
                    break;
            }

            zoomTarget = Mathf.Clamp(zoomTarget, 0.05f, maxZoom);
            float zoomDiff = zoomTarget - zoom;
            Vector2 offsetDiff = positionOffsetTarget - positionOffset;

            if (Mathf.Abs(zoomDiff) > 0.001f || offsetDiff.magnitude > 0.001f)
            {
                zoom += zoomDiff * zoomSpeed * (float)deltaTime;
                positionOffset += offsetDiff * zoomSpeed * (float)deltaTime;
                Repaint();
            }
            else
            {
               zoom = zoomTarget;
               positionOffset = positionOffsetTarget;
            }
            shaderPos = T(Vector2.zero);
        }
        Vector2 shaderPos;
    }

    public class MixtureNodeInspectorObject : NodeInspectorObject
    {
        public event Action pinnedNodeUpdate;

        public HashSet<BaseNodeView> pinnedNodes = new HashSet<BaseNodeView>();

        public void AddPinnedView(BaseNodeView view)
        {
            Selection.activeObject = this;
            if (pinnedNodes.Add(view))
                pinnedNodeUpdate?.Invoke();
        }

        public void RemovePinnedView(BaseNodeView view)
        {
            Selection.activeObject = this;
            if (pinnedNodes.Remove(view))
                pinnedNodeUpdate?.Invoke();
        }
    }
}