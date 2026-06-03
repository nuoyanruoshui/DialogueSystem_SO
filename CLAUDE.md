# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Unity 2022.3.62f3 对话系统项目。核心是一个基于 ScriptableObject 的可视化对话编辑器 + 运行时对话播放器。

## 构建与运行

- **编辑器**：在 Unity Editor 中通过 `Tools > Dialog System` 打开对话编辑器窗口，或直接双击 `.asset` Graph 文件自动打开；场景为 `Assets/DialogSystem/Scene/DialogSystemScene.unity`
- **运行时**：场景中挂载 `DialogueRunner` 组件，通过 Inspector 指定 `DialogueGraph`，运行时按 Space 推进对话，选项节点按 1~9 选择
- **导出/导入 JSON**：编辑器工具栏提供 "导出 JSON" / "导入 JSON" 按钮
- **校验**：工具栏 "校验本图" 按钮检查节点连接完整性

## 项目结构

```
Assets/DialogSystem/
├── Scene/DialogSystemScene.unity          # 主场景
├── NodeSo/                                # ScriptableObject 资产目录（Graph .asset，节点作为 sub-asset）
├── Export/                                # JSON 导出目录
└── Scripts/
    ├── Data/
    │   ├── DialogueNodeBase.cs            # 节点基类 ScriptableObject（nodeId, speakType, speakerName, dialogText, 节点事件列表）
    │   ├── DialogueNode.cs                # 序列节点：可包含多个带条件的出口（DialogueLink 列表）
    │   ├── DialogueOptionNode.cs          # 选项节点：可包含多个带条件的选项（DialogueChoice 列表）
    │   ├── DialogueGraph.cs               # 对话图 ScriptableObject（节点列表、布局、起止、变量）
    │   ├── DialogueGraphJsonModels.cs     # JSON 导入导出的纯数据模型 DTO
    │   ├── DialogueEvent.cs               # ScriptableObject 事件（继承 SOEventBase，封装 UnityEvent）
    │   └── SOEventBase.cs                 # ScriptableObject 事件基类 + Editor 运行时监听器显示
    ├── Rumtime/                           # 目录名拼写为 "Rumtime" 而非 "Runtime"
    │   ├── DialogueRunner.cs              # 运行时对话引擎（推进/选择/条件判定/事件触发）
    │   ├── DialogueSpeakEnums.cs          # 发言类型枚举 (Node/Player/NPC)
    │   ├── DialogueVariables.cs           # 布尔标志变量系统（条件系统的数据源）
    │   ├── DialogueCondition.cs           # 条件系统（DialogueCondition、DialogueLink、DialogueChoice）
    │   └── SOEventListener.cs            # MonoBehaviour 事件监听器（监听 SOEventBase）
    └── Editor/
        ├── DialogueGraphEditorWindow.cs   # 三栏编辑器主窗口（自定义 UIToolkit 布局 + 可拖拽分隔条）
        ├── DialogueGraphView.cs           # GraphView 画布（DialogueNodeView + 网格/放大/中键平移/右键菜单）
        ├── DialogueEditorContext.cs        # 当前编辑上下文静态类
        ├── DialogueEditorPaths.cs          # 资产路径常量与目录创建
        ├── DialogueEditorPanelStyles.cs    # UI 主题样式（IMGUI + OdinMenuStyle）
        ├── DialogueGraphJsonIO.cs          # JSON 序列化/反序列化（含事件）
        └── DialogueGraphValidator.cs       # 图校验（空引用、断连、缺失节点）
```

## 架构要点

### 数据层 (ScriptableObject)
- `DialogueNodeBase`（抽象基类）→ `DialogueNode`（序列节点）/ `DialogueOptionNode`（选项节点）。节点作为 **sub-asset** 存储在 Graph 的 `.asset` 文件中（通过 `AssetDatabase.AddObjectToAsset`），而非独立文件
- `DialogueGraph`：Graph 资产，持有 `startNode`、`nodeList`、节点布局 `NodeLayoutEntry` 列表、`DialogueVariables` 变量系统
- 节点事件系统：`DialogueNodeBase` 包含 `m_NodeEvents` ScriptableObject 引用列表（`SOEventBase`），可在编辑器中通过右键菜单 "Add Node Event" 添加 `DialogueEvent`，运行时在进入节点时调用 `InvokeNodeEvents()`
- `SOEventBase`：ScriptableObject 事件基类，支持 UnityAction 注册/注销；`DialogueEvent` 子类添加了 `UnityEvent onNodeEnter` 供非代码用户使用
- `SOEventListener`（MonoBehaviour）：运行时组件，注册到 `SOEventBase` 资产，收到事件后触发自身的 `UnityEvent<object?>`

### 运行时
- `DialogueRunner`（MonoBehaviour）驱动对话流程：`Advance()` 沿首条满足条件的 `DialogueLink` 推进，`SelectOption(index)` 选择选项，进入节点时调用 `PlayNode()` 和 `InvokeNodeEvents()`
- `DialogueVariables` 管理 `FlagData` 列表（key-value bool 对），编辑器中通过左侧面板的 Parameters 区域编辑
- `DialogueCondition` 判定 `BoolEquals` 条件是否满足，挂载在 `DialogueLink` / `DialogueChoice` 上控制分支
- 支持通过脚本注入条件式：`DialogueRunner` 事件 `OnDialogueStart/OnDialogueEnd/OnNodeChanged/OnChoicesUpdated`

### 编辑器
- `DialogueGraphEditorWindow` 继承 `OdinMenuEditorWindow`，使用 **自定义 UIToolkit 三栏布局**（非默认 Odin 布局）：
  - 左侧：OdinMenuTree（Graph/节点树）+ Parameters 变量编辑器
  - 中间：GraphView 画布（自定义 IMGUI 网格背景 + 缩放/中键平移/右键菜单）
  - 右侧：Odin PropertyTree Inspector + 行内重命名（选中连线时显示条件编辑器）
- `DialogueNodeView`（GraphView 节点）：两种样式 —— TALK（蓝色，序列节点）和 OPTION（橙色，选项节点），标题栏包含事件⚡徽章，起止节点有绿色边框
- 右键菜单（GraphView 空白处）："Create Sequence Node" / "Create Option Node"
- 右键菜单（节点上）："Add Choice"（仅选项节点）/ "Add Node Event" / "Set as Start Node"
- 连线点击：右侧面板显示条件编辑器（E_Condition 枚举、Key 下拉、Bool 值），支持从已有参数列表中选择
- JSON 导入/导出使用 Newtonsoft.Json，通过纯数据 DTO 模型避免依赖 Unity 资产引用

### 命名空间
- 运行时与数据模型：`NuoYan.DialogSystem`
- 编辑器代码：`NuoYan.DialogSystem.Editor`

## 第三方依赖
- **Odin Inspector**（Sirenix）：编辑器增强（`OdinMenuEditorWindow`、`PropertyTree`、`ShowIf` 等）
- **DOTween Pro**（Demigiant）：动画库
- **Newtonsoft.Json**（com.unity.nuget.newtonsoft-json）：JSON 序列化
- **TextMesh Pro**：文本渲染

## 已知问题
- `Assets/DialogSystem/Scripts/Rumtime/` 目录名应为 `Runtime`（拼写错误）
- `Assets/DialogSystem/Scripts/Test.cs` 是空的测试占位文件，可以移除
- 无 `.asmdef` 程序集定义文件，所有脚本编译到默认 `Assembly-CSharp`
- 无单元测试
