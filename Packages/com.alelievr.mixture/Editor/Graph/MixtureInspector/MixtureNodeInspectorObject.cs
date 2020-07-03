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

        // Preview material params:
        Vector2 pan;
        float zoom = 1;

        const float maxZoom = 256;

        internal int previewTextureIndex = 0;
        internal int comparisonTextureIndex = 0;
        internal float compareSlider;

        protected override void OnHeaderGUI()
        {
            base.OnHeaderGUI();
        }

        protected override void OnEnable()
        {
            mixtureInspector = target as MixtureNodeInspectorObject;
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
                var view = CreateNodeBlock(nodeView);
                view.AddToClassList("SelectedNode");
                selectedNodeList.Add(view);

                if (nodeView.nodeTarget is MixtureNode n && n.hasPreview & n.previewTexture != null)
                    nodeWithPreviews.Add(n);
            }

            // Then pinned nodes                
            foreach (var nodeView in mixtureInspector.pinnedNodes)
            {
                var view = CreateNodeBlock(nodeView);
                view.AddToClassList("PinnedView");
                selectedNodeList.Add(view);
                
                if (nodeView.nodeTarget is MixtureNode n && n.hasPreview & n.previewTexture != null)
                    nodeWithPreviews.Add(n);
            }

            // Put pinned in first.
            // nodeWithPreviews.Reverse();
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
            if (e.type != EventType.Repaint)
                Repaint();
        }

        void Fit()
        {
            m_ZoomTarget = 1;
            m_PositionOffsetTarget = Vector2.zero;
        }

        public override void OnInteractivePreviewGUI(Rect previewRect, GUIStyle background)
        {
            Texture previewTexture = null;
            Texture compareTexture = null;

            OnGUI2(previewRect);

            if (e.type == EventType.ScrollWheel)
            {
                float step = e.delta.y * 0.01f;
                zoom *= 1.0f + step;

                Vector2 localPos = (e.mousePosition / previewRect.size) * 2.0f - Vector2.one;

                var p = Matrix4x4.Translate(localPos) * Matrix4x4.Scale(Vector3.one * zoom) * Matrix4x4.Translate(-localPos) * localPos;

                pan = new Vector2(p.x, p.y);
                // pan += new Vector2(-localPos.x, localPos.y) * step * (1.0f / zoom);

                Repaint();
            }

            if (e.type == EventType.MouseDrag)
            {
                if (e.button == 0)
                    pan += new Vector2(-e.delta.x / previewRect.width, e.delta.y / previewRect.height) / zoom;
                else if (e.button == 1)
                    compareSlider = e.mousePosition.x / (float)previewRect.width;
                Repaint();
            }

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
                previewMaterial.SetFloat("_Zoom", m_Zoom);
                previewMaterial.SetVector("_Pan", shaderPos / previewRect.size);
                EditorGUI.DrawPreviewTexture(previewRect, previewTexture, previewMaterial);
            }
            else
                EditorGUI.DrawRect(previewRect, new Color(1, 0, 1, 1));

            Color oldHandlesColor = Handles.color;
            Handles.color = new Color(1.2f, 0.2f, 0.2f, 1);
            Handles.DrawLine(new Vector3(-10000, shaderPos.y), new Vector3(10000, shaderPos.y));
            Handles.DrawLine(new Vector3(shaderPos.x, -1000), new Vector3(shaderPos.x, 1000));
            Handles.color = oldHandlesColor;
        }

        Vector2 T(Vector2 pos)
        {
            pos *= m_Zoom;
            pos += m_PositionOffset;
            return pos;
        }

        Vector2 InvT(Vector2 pos)
        {
            pos -= m_PositionOffset;
            pos /= m_Zoom;

            return pos;
        }

        Vector2 m_MiddleClickMousePosition;
        Vector2 m_MiddleClickCameraPosition;
        Vector2 m_PositionOffset;
        Vector2 m_PositionOffsetTarget;
        float m_ZoomTarget = 1;
        Vector2 lastMousePosition;
        float m_Zoom = 1;
        float m_ZoomSpeed = 20f;
        double timeSinceStartup, latestTime, deltaTime;
        void HandleZoomAndPan(Rect previewRect)
        {
            timeSinceStartup = EditorApplication.timeSinceStartup;
            deltaTime = timeSinceStartup  - latestTime;
            latestTime = timeSinceStartup;

            lastMousePosition = e.mousePosition / previewRect.size * 2 - Vector2.one;
            lastMousePosition.x = -lastMousePosition.x;
            lastMousePosition = InvT(lastMousePosition);
            if (e.type == EventType.ScrollWheel)
                Debug.Log(lastMousePosition);
            switch (e.type)
            {
                case EventType.MouseDown:
                    if (IsMiddleMouse(e.button, e.modifiers))
                    {
                        m_MiddleClickMousePosition = lastMousePosition;
                        m_MiddleClickCameraPosition = m_PositionOffset;
                    }
                    break;

                case EventType.MouseDrag:
                    if (IsMiddleMouse(e.button, e.modifiers))
                    {
                        m_PositionOffset = m_MiddleClickCameraPosition + (lastMousePosition - m_MiddleClickMousePosition);
                        // m_PositionOffset /= previewRect.size * 2.0f - Vector2.one;
                        m_PositionOffsetTarget = m_PositionOffset;
                    }
                    break;
                case EventType.MouseUp:
                    // if (SeedPictHandlePos != Vector2.zero)
                    { 
                        // graph.backgroundPictPivot = (backgroundPictPos - SeedPictHandlePos) / backgroundPictHeight;
                        // SeedPictHandlePos = Vector2.zero;
                    }
                    break;
                case EventType.ScrollWheel:
                    float delta = 1f + Mathf.Abs((float)e.delta.y * 0.1f);
                    delta = e.delta.y > 0 ? delta = 1 / delta : delta;
                    m_ZoomTarget *= delta;
                    m_PositionOffsetTarget = lastMousePosition + (m_PositionOffsetTarget - lastMousePosition) * delta;
                    Debug.Log("m_PositionOffset: " + m_PositionOffsetTarget + " | " + delta);
                    break;
            }

            float zoomDiff = m_ZoomTarget - m_Zoom;
            Vector2 offsetDiff = m_PositionOffsetTarget - m_PositionOffset;

            // if (Mathf.Abs(zoomDiff) > 0.001f || offsetDiff.magnitude > 0.001f)
            // {
            //     //m_Zoom += zoomDiff * m_ZoomSpeed * Time.deltaTime;
            //     //m_PositionOffset += offsetDiff * m_ZoomSpeed * Time.deltaTime;
            //     m_Zoom += zoomDiff * m_ZoomSpeed * (float)deltaTime;
            //     m_PositionOffset += offsetDiff * m_ZoomSpeed * (float)deltaTime;
            //     //Debug.Log(deltaTime.ToString("f4"));    
            // }
            // else
            {
               m_Zoom = m_ZoomTarget;
               m_PositionOffset = m_PositionOffsetTarget;
            }

        }
        bool IsMiddleMouse(int button, EventModifiers mods) => e.button == 2 || (e.button == 0 && e.modifiers == EventModifiers.Alt);

        public void OnGUI2(Rect previewRect)
        {
            timeSinceStartup = EditorApplication.timeSinceStartup;
            deltaTime = timeSinceStartup  - latestTime;
            latestTime = timeSinceStartup;

            lastMousePosition = e.mousePosition;
            switch (e.type)
            {

                case EventType.MouseDown:
                    {
                        if (IsMiddleMouse(e.button, e.modifiers))
                        {
                            m_MiddleClickMousePosition = lastMousePosition;
                            m_MiddleClickCameraPosition = m_PositionOffset;
                        }

                        break;
                    }

                case EventType.MouseDrag:
                    {
                        if (IsMiddleMouse(e.button, e.modifiers))
                        {
                            m_PositionOffset = m_MiddleClickCameraPosition + (lastMousePosition - m_MiddleClickMousePosition);
                            m_PositionOffsetTarget = m_PositionOffset;
                        }

                        break;
                    }

                case EventType.MouseUp:
                    {
                        // if (SeedPictHandlePos != Vector2.zero)
                        // { 
                        //     graph.backgroundPictPivot = (backgroundPictPos - SeedPictHandlePos) / backgroundPictHeight;
                        //     SeedPictHandlePos = Vector2.zero;
                        // }
                        break;
                    }

                case EventType.ScrollWheel:
                    {
                        float delta = Mathf.Clamp(1f + Mathf.Abs((float)e.delta.y * 0.1f), 0.1f, 2f);
                        delta = e.delta.y > 0 ? 1f / delta : delta;
                        m_ZoomTarget *= delta;
                        if (m_ZoomTarget < maxZoom)
                            m_PositionOffsetTarget = lastMousePosition + (m_PositionOffsetTarget - lastMousePosition) * delta;
                        break;
                    }

                // case EventType.KeyDown:
                //     if (e.keyCode == KeyCode.R)
                //     {
                //         ViewReset();
                //     }
                //     if (e.keyCode == KeyCode.F)
                //     {
                //         ViewFit();
                //     }
                    // break;

                

                default:
                    break;
            }



            m_ZoomTarget = Mathf.Clamp(m_ZoomTarget, 0.05f, maxZoom);
            float zoomDiff = m_ZoomTarget - m_Zoom;
            Vector2 offsetDiff = m_PositionOffsetTarget - m_PositionOffset;

            if (Mathf.Abs(zoomDiff) > 0.001f || offsetDiff.magnitude > 0.001f)
            {
                //m_Zoom += zoomDiff * m_ZoomSpeed * Time.deltaTime;
                //m_PositionOffset += offsetDiff * m_ZoomSpeed * Time.deltaTime;
                m_Zoom += zoomDiff * m_ZoomSpeed * (float)deltaTime;
                m_PositionOffset += offsetDiff * m_ZoomSpeed * (float)deltaTime;
                //Debug.Log(deltaTime.ToString("f4"));    
            }
            else
            {
               m_Zoom = m_ZoomTarget;
               m_PositionOffset = m_PositionOffsetTarget;
            }

            //backgroundPictPivot = Vector2.one * 0.25f;
            // if (graph.backgroundImage != null && graph.displayBackground)
            // {
            //     ComputeBackgroundTransform();
            //     DrawTextureBottomCenter(backgroundPictPos, backgroundPictHeight, graph.backgroundImage);

            //     if (graph.HeightHandlePos != Vector2.zero)
            //     {
            //         graph.HeightHandlePos = new Vector2(backgroundPictPos.x, graph.HeightHandlePos.y);
            //         graph.HeightHandlePos = GenHandle(graph.HeightHandlePos, 8, true, false, true);
            //     }
            //     else
            //         graph.HeightHandlePos = new Vector2(0, -1000);
            // }

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