#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace NuoYan.DialogSystem.Editor
{
    /// <summary>
    /// 单个对话节点的 GraphView 视图
    /// </summary>
    public class DialogueNodeView : Node
    {
        readonly DialogueGraph graph;
        readonly List<Port> outputPorts = new List<Port>();
        bool hasSavedLayout;
        Vector2 savedLayoutPosition;

        static readonly Color SeqColor = new Color(0.20f, 0.55f, 0.72f);
        static readonly Color OptColor = new Color(0.80f, 0.50f, 0.18f);
        static readonly Color DefaultBg = new Color(0.24f, 0.24f, 0.24f);

        public DialogueNodeBase Node { get; }

        public Port InputPort { get; private set; }

        public DialogueNodeView(DialogueNodeBase node, DialogueGraph graph)
        {
            Node = node;
            this.graph = graph;

            viewDataKey = node.GetInstanceID().ToString();
            capabilities |= Capabilities.Selectable | Capabilities.Movable | Capabilities.Deletable;

            ApplyNodeStyle();
            BuildPorts();
            BuildEventIndicator();

            style.width = 150;
            RefreshExpandedState();
            RefreshPorts();
        }

        void ApplyNodeStyle()
        {
            bool isOption = Node is DialogueOptionNode;
            Color accent = isOption ? OptColor : SeqColor;

            // 标题栏
            titleContainer.style.backgroundColor = accent;
            titleContainer.style.paddingTop = 4;
            titleContainer.style.paddingBottom = 4;
            titleContainer.style.borderTopLeftRadius = 6;
            titleContainer.style.borderTopRightRadius = 6;

            // 类型标签
            titleContainer.Add(new Label(isOption ? "OPTION" : "TALK")
            {
                name = "node-type-badge",
                pickingMode = PickingMode.Ignore,
                style =
                {
                    fontSize = 9,
                    color = new Color(1, 1, 1, 0.65f),
                    unityFontStyleAndWeight = FontStyle.Bold,
                    marginLeft = 6,
                    marginTop = 2,
                    unityTextAlign = TextAnchor.UpperLeft,
                }
            });

            // 主题边框
            mainContainer.style.backgroundColor = DefaultBg;
            mainContainer.style.borderLeftWidth = 2;
            mainContainer.style.borderRightWidth = 2;
            mainContainer.style.borderBottomWidth = 2;
            mainContainer.style.borderLeftColor = accent;
            mainContainer.style.borderRightColor = accent;
            mainContainer.style.borderBottomColor = accent;
            mainContainer.style.borderBottomLeftRadius = 4;
            mainContainer.style.borderBottomRightRadius = 4;

            RefreshTitle();
        }

        void BuildPorts()
        {
            InputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));
            InputPort.portName = "In";
            inputContainer.Add(InputPort);

            if (Node is DialogueOptionNode)
                RebuildChoicePorts();
            else
                AddSingleOutputPort("Out");
        }

        public void RefreshTitle()
        {
            string text = Node.DialogText;
            if (!string.IsNullOrEmpty(text) && text.Length > 20)
                text = text.Substring(0, 20) + "…";
            title = $"[{Node.NodeId}] {Node.SpeakerName}\n{text}";
        }

        void AddSingleOutputPort(string portName)
        {
            var port = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
            port.portName = portName;
            outputContainer.Add(port);
            outputPorts.Add(port);
        }

        public void RebuildChoicePorts()
        {
            outputContainer.Clear();
            outputPorts.Clear();

            if (Node is not DialogueOptionNode optNode)
            {
                AddSingleOutputPort("Out");
                return;
            }

            int count = Mathf.Max(1, optNode.ChoiceList?.Count ?? 0);
            for (int i = 0; i < count; i++)
            {
                var port = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
                string label = "新选项";
                if (optNode.ChoiceList != null && i < optNode.ChoiceList.Count && optNode.ChoiceList[i] != null)
                    label = string.IsNullOrEmpty(optNode.ChoiceList[i].labelText) ? $"选项{i + 1}" : optNode.ChoiceList[i].labelText;
                port.portName = label;
                port.userData = i;
                outputContainer.Add(port);
                outputPorts.Add(port);
            }

            RefreshExpandedState();
            RefreshPorts();
        }

        public Port OutputPort => outputPorts.Count > 0 ? outputPorts[0] : null;

        public Port GetOutputPort(int index)
        {
            if (index < 0 || index >= outputPorts.Count)
                return null;
            return outputPorts[index];
        }

        public int GetOutputPortIndex(Port port) => outputPorts.IndexOf(port);

        static readonly Color StartBorderColor = new Color(0.20f, 0.85f, 0.35f);

        public void MarkAsStartNode()
        {
            titleContainer.style.borderTopWidth = 3;
            titleContainer.style.borderTopColor = StartBorderColor;
        }

        Label eventBadge;

        void BuildEventIndicator()
        {
            // 标题栏事件计数徽章
            eventBadge = new Label
            {
                name = "node-event-badge",
                pickingMode = PickingMode.Ignore,
                text = "",
                style =
                {
                    fontSize = 9,
                    color = new Color(1, 1, 0.5f, 0.85f),
                    unityFontStyleAndWeight = FontStyle.Bold,
                    marginLeft = 6,
                }
            };
            titleContainer.Add(eventBadge);

            RefreshEvents();
        }

        internal void RefreshEvents()
        {
            // 更新徽章
            int count = 0;
            var so = new UnityEditor.SerializedObject(Node);
            var listProp = so.FindProperty("m_NodeEvents");
            if (listProp != null)
                count = listProp.arraySize;
            eventBadge.text = count > 0 ? $"⚡{count}" : "";

            // 重建扩展区（事件行 + 工具栏）
            extensionContainer.Clear();
            extensionContainer.style.display = DisplayStyle.None;

            if (count == 0) return;

            extensionContainer.style.display = DisplayStyle.Flex;

            // 事件行
            for (int i = 0; i < listProp.arraySize; i++)
            {
                var elemProp = listProp.GetArrayElementAtIndex(i);
                var evRef = elemProp.objectReferenceValue;
                if (evRef == null) continue;

                int index = i;
                var row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.alignItems = Align.Center;
                row.style.minHeight = 22;
                row.style.backgroundColor = new Color(1, 1, 0.5f, 0.06f);
                row.style.paddingLeft = 6;
                row.style.paddingRight = 2;
                row.style.borderBottomWidth = 1;
                row.style.borderBottomColor = new Color(1, 1, 0.5f, 0.15f);

                // 点击整行 Ping 资产
                row.RegisterCallback<MouseDownEvent>(_ =>
                {
                    UnityEditor.Selection.activeObject = evRef;
                    EditorGUIUtility.PingObject(evRef);
                });

                var delBtn = new Label("×");
                delBtn.style.fontSize = 14;
                delBtn.style.color = new Color(1, 0.3f, 0.3f, 0.75f);
                delBtn.style.width = 22;
                delBtn.style.height = 18;
                delBtn.style.unityTextAlign = TextAnchor.MiddleCenter;
                delBtn.RegisterCallback<MouseDownEvent>(_ =>
                {
                    var s = new UnityEditor.SerializedObject(Node);
                    var p = s.FindProperty("m_NodeEvents");
                    if (index < p.arraySize)
                    {
                        var ev = p.GetArrayElementAtIndex(index).objectReferenceValue;
                        p.DeleteArrayElementAtIndex(index);
                        s.ApplyModifiedProperties();
                        if (ev != null) { AssetDatabase.RemoveObjectFromAsset(ev); UnityEngine.Object.DestroyImmediate(ev, true); }
                        EditorUtility.SetDirty(Node); AssetDatabase.SaveAssets();
                        RefreshEvents();
                    }
                });
                row.Add(delBtn);
                row.Add(new Label($"{evRef.name}")
                {
                    style =
                    {
                        fontSize = 6,
                        color = new Color(1, 1, 0.5f, 0.85f),
                        unityFontStyleAndWeight = FontStyle.Bold,
                        flexGrow = 1,
                    }
                });
                extensionContainer.Add(row);
            }

            RefreshExpandedState();
        }

        public void SaveLayout()
        {
            if (graph == null || Node == null)
                return;

            Vector2 position = GetPosition().position;
            if (hasSavedLayout && Vector2.SqrMagnitude(position - savedLayoutPosition) < 0.01f)
                return;

            hasSavedLayout = true;
            savedLayoutPosition = position;
            graph.SetLayout(Node, position);
            EditorUtility.SetDirty(graph);
        }
    }

    /// <summary>
    /// 对话图 GraphView 画布
    /// </summary>
    public class DialogueGraphView : GraphView
    {
        const float DefaultNodeWidth = 240f;
        const float DefaultNodeHeight = 120f;
        const float MinorGridSpacing = 20f;
        const float MajorGridSpacing = 100f;
        const int MiddleMouseButton = 2;

        readonly DialogueGraphEditorWindow ownerWindow;
        readonly Dictionary<DialogueNodeBase, DialogueNodeView> nodeViews = new Dictionary<DialogueNodeBase, DialogueNodeView>();
        readonly IMGUIContainer gridBackground;

        DialogueGraph currentGraph;
        Vector2 lastPanMousePosition;
        bool isPopulating;
        bool isMiddleMousePanning;
        bool suppressSelectionBroadcast;

        public DialogueGraph CurrentGraph => currentGraph;

        public DialogueGraphView(DialogueGraphEditorWindow ownerWindow)
        {
            this.ownerWindow = ownerWindow;

            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ClickSelector());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            gridBackground = new IMGUIContainer(DrawGridBackground)
            {
                name = "dialog-graph-grid"
            };
            gridBackground.pickingMode = PickingMode.Ignore;
            Insert(0, gridBackground);
            gridBackground.StretchToParentSize();

            graphViewChanged = OnGraphViewChanged;

            style.flexGrow = 1;
            pickingMode = PickingMode.Position;
            focusable = true;
            RegisterCallback<MouseDownEvent>(_ => Focus());
            RegisterCallback<MouseDownEvent>(OnMouseDown, TrickleDown.TrickleDown);
            RegisterCallback<MouseMoveEvent>(OnMouseMove, TrickleDown.TrickleDown);
            RegisterCallback<MouseUpEvent>(OnMouseUp, TrickleDown.TrickleDown);
            RegisterCallback<WheelEvent>(_ => gridBackground.MarkDirtyRepaint());
        }

        void DrawGridBackground()
        {
            var rect = new Rect(0f, 0f, resolvedStyle.width, resolvedStyle.height);
            if (rect.width <= 0f || rect.height <= 0f)
                rect = new Rect(0f, 0f, layout.width, layout.height);

            EditorGUI.DrawRect(rect, new Color(0.14f, 0.14f, 0.14f));
            DrawGridLines(rect, MinorGridSpacing, new Color(0.23f, 0.23f, 0.23f, 0.7f), 1f);
            DrawGridLines(rect, MajorGridSpacing, new Color(0.31f, 0.31f, 0.31f, 0.9f), 1f);
        }

        void DrawGridLines(Rect rect, float baseSpacing, Color color, float lineWidth)
        {
            float scale = Mathf.Max(0.05f, viewTransform.scale.x);
            float spacing = baseSpacing * scale;
            if (spacing < 4f)
                return;

            Vector3 offset = viewTransform.position;
            float xStart = offset.x % spacing;
            float yStart = offset.y % spacing;

            if (xStart > 0f)
                xStart -= spacing;
            if (yStart > 0f)
                yStart -= spacing;

            for (float x = xStart; x < rect.width; x += spacing)
                EditorGUI.DrawRect(new Rect(x, 0f, lineWidth, rect.height), color);

            for (float y = yStart; y < rect.height; y += spacing)
                EditorGUI.DrawRect(new Rect(0f, y, rect.width, lineWidth), color);
        }

        // 画布选中变化后同步左侧树 / 右侧连线属性
        void SyncGraphSelectionToTree()
        {
            if (suppressSelectionBroadcast || isPopulating)
                return;

            // 优先检测连线选中
            foreach (var item in selection)
            {
                if (item is Edge edge && edge.userData != null && edge.output?.node != null)
                {
                    var sourceView = edge.output.node as DialogueNodeView;
                    var sourceNode = sourceView?.Node;
                    if (sourceNode != null)
                    {
                        ownerWindow.SelectEdgeData(edge.userData, sourceNode);
                        return;
                    }
                }
            }

            // 没有选中连线 → 清除连线属性面板
            ownerWindow.ClearEdgeSelection();

            foreach (var item in selection)
            {
                if (item is DialogueNodeView nodeView)
                {
                    ownerWindow.SelectObjectInTree(nodeView.Node);
                    return;
                }
            }

            if (!selection.Any() && currentGraph != null)
                ownerWindow.SelectObjectInTree(currentGraph);
        }

        // 点击连线时同步右侧属性面板
        static void RegisterEdgeClick(Edge edge, DialogueGraphView gv)
        {
            edge.RegisterCallback<MouseDownEvent>(_ =>
            {
                gv.schedule.Execute(gv.SyncGraphSelectionToTree);
            });
        }

        static Port FindPort(VisualElement element)
        {
            while (element != null)
            {
                if (element is Port port)
                    return port;

                element = element.parent;
            }

            return null;
        }

        void OnMouseDown(MouseDownEvent evt)
        {
            if (evt.button != MiddleMouseButton)
                return;

            isMiddleMousePanning = true;
            lastPanMousePosition = evt.mousePosition;
            this.CaptureMouse();
            Focus();
            evt.StopImmediatePropagation();
        }

        void OnMouseUp(MouseUpEvent evt)
        {
            if (evt.button == 0)
            {
                if (FindPort(evt.target as VisualElement) != null)
                    return;

                schedule.Execute(SyncGraphSelectionToTree);
                return;
            }

            if (!isMiddleMousePanning || evt.button != MiddleMouseButton)
                return;

            isMiddleMousePanning = false;
            this.ReleaseMouse();
            evt.StopImmediatePropagation();
        }

        void OnMouseMove(MouseMoveEvent evt)
        {
            if (!isMiddleMousePanning || !this.HasMouseCapture())
                return;

            Vector2 delta = evt.mousePosition - lastPanMousePosition;
            lastPanMousePosition = evt.mousePosition;
            UpdateViewTransform(viewTransform.position + new Vector3(delta.x, delta.y, 0f), viewTransform.scale);
            gridBackground.MarkDirtyRepaint();
            evt.StopImmediatePropagation();
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.ToList().Where(p =>
                p.direction != startPort.direction &&
                p.node != startPort.node).ToList();
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (currentGraph != null)
            {
                var mousePos = contentViewContainer.WorldToLocal(evt.localMousePosition);
                evt.menu.AppendAction("Create Sequence Node", _ => CreateSequenceNodeAtMouse(mousePos));
                evt.menu.AppendAction("Create Option Node", _ => CreateOptionNodeAtMouse(mousePos));
                evt.menu.AppendSeparator();
            }

            base.BuildContextualMenu(evt);
        }

        void CreateSequenceNodeAtMouse(Vector2 localPosition)
        {
            if (currentGraph == null)
                return;

            var node = ownerWindow.CreateNodeAsset<DialogueNode>(currentGraph);
            if (node == null)
                return;

            currentGraph.AddNode(node);
            EditorUtility.SetDirty(currentGraph);

            var view = CreateNodeView(node, localPosition);
            view.SaveLayout();
            EditorApplication.delayCall += () =>
            {
                ownerWindow.ForceMenuTreeRebuild();
                ownerWindow.ResetSelectionSync();
            };
        }

        void CreateOptionNodeAtMouse(Vector2 localPosition)
        {
            if (currentGraph == null)
                return;

            var node = ownerWindow.CreateNodeAsset<DialogueOptionNode>(currentGraph);
            if (node == null)
                return;

            // 预置 2 个默认选项使创建后即可连接
            node.AddChoice(new DialogueChoice { labelText = "选项1", condition = new DialogueCondition() });
            node.AddChoice(new DialogueChoice { labelText = "选项2", condition = new DialogueCondition() });

            currentGraph.AddNode(node);
            EditorUtility.SetDirty(currentGraph);

            var view = CreateNodeView(node, localPosition);
            view.SaveLayout();
            EditorApplication.delayCall += () =>
            {
                ownerWindow.ForceMenuTreeRebuild();
                ownerWindow.ResetSelectionSync();
            };
        }

        GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            if (currentGraph == null || isPopulating)
                return change;

            if (change.edgesToCreate != null)
            {
                foreach (var edge in change.edgesToCreate)
                {
                    RegisterEdgeClick(edge, this);
                    ApplyEdgeCreate(edge);
                }
            }

            if (change.movedElements != null)
            {
                foreach (var element in change.movedElements)
                {
                    if (element is DialogueNodeView nodeView)
                        nodeView.SaveLayout();
                }
            }

            if (change.elementsToRemove != null)
            {
                foreach (var element in change.elementsToRemove)
                {
                    if (element is Edge edge)
                        ApplyEdgeRemove(edge);
                    else if (element is DialogueNodeView nodeView)
                        RemoveNodeFromGraph(nodeView);
                }
            }

            return change;
        }

        void ApplyEdgeCreate(Edge edge)
        {
            var sourceView = edge.output.node as DialogueNodeView;
            var targetView = edge.input.node as DialogueNodeView;
            if (sourceView == null || targetView == null)
                return;

            var sourceNode = sourceView.Node;
            var targetNode = targetView.Node;

            if (sourceNode is DialogueOptionNode optNode)
            {
                int portIndex = sourceView.GetOutputPortIndex(edge.output);
                EnsureChoiceCount(optNode, portIndex + 1);
                optNode.ChoiceList[portIndex].toNode = targetNode;
            }
            else if (sourceNode is DialogueNode seqNode)
            {
                seqNode.AddLink(new DialogueLink()
                {
                    toNode = targetNode,
                    condition = new DialogueCondition()
                });
            }

            EditorUtility.SetDirty(sourceNode);
        }

        void ApplyEdgeRemove(Edge edge)
        {
            var sourceView = edge.output.node as DialogueNodeView;
            var targetView = edge.input.node as DialogueNodeView;
            if (sourceView == null || targetView == null)
                return;

            var sourceNode = sourceView.Node;
            var targetNode = targetView.Node;

            if (sourceNode is DialogueOptionNode optNode)
            {
                int portIndex = sourceView.GetOutputPortIndex(edge.output);
                if (portIndex >= 0 && portIndex < optNode.ChoiceList.Count)
                    optNode.ChoiceList[portIndex].toNode = null;
            }
            else if (sourceNode is DialogueNode seqNode && seqNode.LinkList != null)
            {
                seqNode.LinkList.RemoveAll(link => link != null && link.toNode == targetNode);
            }

            EditorUtility.SetDirty(sourceNode);
        }

        void RemoveNodeFromGraph(DialogueNodeView nodeView)
        {
            if (currentGraph == null || nodeView?.Node == null)
                return;

            var node = nodeView.Node;
            currentGraph.RemoveNode(node);
            nodeViews.Remove(node);

            // 删除节点资产（sub-asset 用 RemoveObjectFromAsset，独立资产用 DeleteAsset）
            if (AssetDatabase.IsSubAsset(node))
            {
                AssetDatabase.RemoveObjectFromAsset(node);
                UnityEngine.Object.DestroyImmediate(node, true);
            }
            else
            {
                string assetPath = AssetDatabase.GetAssetPath(node);
                if (!string.IsNullOrEmpty(assetPath))
                    AssetDatabase.DeleteAsset(assetPath);
            }

            EditorUtility.SetDirty(currentGraph);
            ownerWindow.ForceMenuTreeRebuild();
            ownerWindow.ResetSelectionSync();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        static void EnsureChoiceCount(DialogueOptionNode node, int count)
        {
            if (node.ChoiceList == null)
                return;

            while (node.ChoiceList.Count < count)
            {
                node.AddChoice(new DialogueChoice
                {
                    labelText = $"选项{node.ChoiceList.Count + 1}",
                    condition = new DialogueCondition()
                });
            }
        }

        public void Populate(DialogueGraph graph, bool focusStartNode = false)
        {
            currentGraph = graph;
            isPopulating = true;
            try
            {
                DeleteElements(graphElements.ToList());
                nodeViews.Clear();

                if (graph?.NodeList == null)
                    return;

                int index = 0;
                foreach (var node in graph.NodeList)
                {
                    if (node == null)
                        continue;

                    var layout = graph.GetLayout(node);
                    if (layout == Vector2.zero)
                        layout = new Vector2(260f * index, 80f * (index % 4));

                    CreateNodeView(node, layout);
                    index++;
                }

                BuildEdges();
                HighlightStartNode();
                gridBackground.MarkDirtyRepaint();

                if (focusStartNode)
                    FocusStartNodeOrGraph();
            }
            finally
            {
                isPopulating = false;
            }
        }

        DialogueNodeView CreateNodeView(DialogueNodeBase node, Vector2 position)
        {
            var view = new DialogueNodeView(node, currentGraph);
            view.SetPosition(new Rect(position, new Vector2(DefaultNodeWidth, DefaultNodeHeight)));
            AddElement(view);
            nodeViews[node] = view;

            view.capabilities |= Capabilities.Selectable | Capabilities.Movable | Capabilities.Deletable;
            view.RegisterCallback<ContextualMenuPopulateEvent>(evt =>
            {
                if (node is DialogueOptionNode optNode)
                {
                    int index = (optNode.ChoiceList?.Count ?? 0) + 1;
                    evt.menu.AppendAction("Add Choice", _ =>
                    {
                        optNode.AddChoice(new DialogueChoice
                        {
                            labelText = $"选项{index}",
                            condition = new DialogueCondition()
                        });
                        EditorUtility.SetDirty(optNode);
                        view.RebuildChoicePorts();
                        ownerWindow.ForceMenuTreeRebuild();
                        ownerWindow.ResetSelectionSync();
                    });
                    evt.menu.AppendSeparator();
                }
                evt.menu.AppendAction("Add Node Event", _ =>
                {
                    var ev = ScriptableObject.CreateInstance<DialogueEvent>();
                    var so = new SerializedObject(node);
                    var listProp = so.FindProperty("m_NodeEvents");
                    ev.name = $"{node.name}_Event_{listProp.arraySize}";
                    AssetDatabase.AddObjectToAsset(ev, node);
                    AssetDatabase.SaveAssets();


                    listProp.InsertArrayElementAtIndex(listProp.arraySize);
                    listProp.GetArrayElementAtIndex(listProp.arraySize - 1).objectReferenceValue = ev;
                    so.ApplyModifiedPropertiesWithoutUndo();

                    EditorUtility.SetDirty(node);
                    view.RefreshEvents();
                    ownerWindow.ForceMenuTreeRebuild();
                    ownerWindow.ResetSelectionSync();
                });
                evt.menu.AppendSeparator();
                evt.menu.AppendAction("Set as Start Node", _ => SetStartNode(node));
                evt.menu.AppendAction("Focus on Left Tree", _ => ownerWindow.SelectObjectInTree(node));
            });

            return view;
        }

        void SetStartNode(DialogueNodeBase node)
        {
            if (currentGraph == null || node == null)
                return;

            currentGraph.SetStartNode(node);
            EditorUtility.SetDirty(currentGraph);
            HighlightStartNode();
        }

        void HighlightStartNode()
        {
            foreach (var pair in nodeViews)
            {
                if (pair.Key == currentGraph.StartNode)
                    pair.Value.MarkAsStartNode();
            }
        }

        void BuildEdges()
        {
            foreach (var pair in nodeViews)
            {
                var node = pair.Key;
                var sourceView = pair.Value;

                if (node is DialogueOptionNode optNode)
                {
                    sourceView.RebuildChoicePorts();
                    if (optNode.ChoiceList == null)
                        continue;

                    for (int i = 0; i < optNode.ChoiceList.Count; i++)
                    {
                        var choice = optNode.ChoiceList[i];
                        if (choice?.toNode == null)
                            continue;
                        if (!nodeViews.TryGetValue(choice.toNode, out var targetView))
                            continue;

                        var outPort = sourceView.GetOutputPort(i);
                        if (outPort == null)
                            continue;

                        var edge = outPort.ConnectTo(targetView.InputPort);
                        edge.userData = choice;
                        RegisterEdgeClick(edge, this);
                        AddElement(edge);
                    }
                }
                else if (node is DialogueNode seqNode)
                {
                    if (seqNode.LinkList == null)
                        continue;

                    foreach (var link in seqNode.LinkList)
                    {
                        if (link?.toNode == null)
                            continue;
                        if (!nodeViews.TryGetValue(link.toNode, out var targetView))
                            continue;

                        var edge = sourceView.OutputPort.ConnectTo(targetView.InputPort);
                        edge.userData = link;
                        RegisterEdgeClick(edge, this);
                        AddElement(edge);
                    }
                }
            }
        }

        public void SelectNode(DialogueNodeBase node)
        {
            if (node == null || !nodeViews.TryGetValue(node, out var view))
                return;

            RunWithoutSelectionBroadcast(() =>
            {
                ClearSelection();
                AddToSelection(view);
            });
        }

        void RunWithoutSelectionBroadcast(System.Action action)
        {
            suppressSelectionBroadcast = true;
            try
            {
                action();
            }
            finally
            {
                schedule.Execute(() => suppressSelectionBroadcast = false);
            }
        }

        public void FocusNode(DialogueNodeBase node)
        {
            if (node == null || !nodeViews.TryGetValue(node, out var view))
                return;

            SelectNode(node);
            schedule.Execute(() =>
            {
                FrameSelection();
                gridBackground.MarkDirtyRepaint();
            });
        }

        void FocusStartNodeOrGraph()
        {
            if (currentGraph?.StartNode != null && nodeViews.ContainsKey(currentGraph.StartNode))
            {
                FocusNode(currentGraph.StartNode);
                return;
            }

            FrameCurrentGraph();
        }

        public void FrameCurrentGraph()
        {
            if (nodeViews.Count == 0)
                return;

            schedule.Execute(() =>
            {
                FrameAll();
                gridBackground.MarkDirtyRepaint();
            });
        }

        public void RefreshCurrentGraph(bool preserveView)
        {
            if (currentGraph == null)
                return;

            var graph = currentGraph;
            var position = viewTransform.position;
            var scale = viewTransform.scale;

            Populate(graph);

            if (preserveView)
            {
                schedule.Execute(() =>
                {
                    UpdateViewTransform(position, scale);
                    gridBackground.MarkDirtyRepaint();
                });
            }
        }

        public void ApplySelection(object selected)
        {
            if (selected is DialogueGraph graph)
            {
                if (currentGraph != graph)
                    Populate(graph, focusStartNode: true);
                style.display = DisplayStyle.Flex;
                return;
            }

            if (selected is DialogueNodeBase node)
            {
                var graphForNode = ownerWindow.FindGraphForNode(node);
                if (graphForNode == null)
                    return;

                if (currentGraph != graphForNode)
                    Populate(graphForNode, focusStartNode: true);

                SelectNode(node);
                style.display = DisplayStyle.Flex;
                return;
            }

            style.display = DisplayStyle.None;
        }

        public void RefreshNodeTitles()
        {
            foreach (var view in nodeViews.Values)
                view?.RefreshTitle();
        }
    }
}
#endif
