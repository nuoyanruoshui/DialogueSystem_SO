# M_DialogSystem — Unity 对话系统

基于 **ScriptableObject + GraphView** 的可视化对话编辑器与运行时对话播放器。编辑器内拖拽节点、连线、配置分支条件与节点事件；运行时通过 `DialogueRunner` 驱动对话流程，并自带一套基于 `Resources` 加载的 UI 面板（`PanelDialogue` / `DialogueOption`）。

- Unity 版本：**2022.3.62f3**
- 命名空间：运行时 `NuoYan.DialogSystem`，编辑器 `NuoYan.DialogSystem.Editor`

---

## 功能特性

- **可视化编辑**：GraphView 画布，支持网格背景、缩放、中键平移、右键菜单快速建节点
- **两种节点**：序列节点（自动推进）+ 选项节点（玩家选择分支）
- **条件系统**：节点出口 / 选项支持挂载布尔条件（`BoolEquals`），基于 `DialogueVariables` 标志变量
- **节点事件**：节点可挂载 `SOEventBase` 事件资产（`DialogueEvent`），进入节点时自动触发 `UnityEvent`
- **JSON 导入 / 导出**：基于 Newtonsoft.Json 的纯 DTO 序列化，不依赖 Unity 资产引用
- **运行时 UI**：`DialogueManager` 单例自动从 `Resources` 加载对话面板与选项按钮
- **图校验**：一键校验空引用、断连、缺失节点
- **三栏编辑器布局**：左侧菜单树 + 中间画布 + 右侧 Inspector，可拖拽调整宽度

---

## 环境要求

| 依赖 | 说明 |
| --- | --- |
| Unity 2022.3.62f3+ | 项目使用 `2022.3.62f3c1` |
| [Odin Inspector](https://odininspector.com) | 编辑器增强（`OdinMenuEditorWindow`、`PropertyTree` 等） |
| [DOTween Pro](http://dotween.demigiant.com) | 动画库 |
| [Newtonsoft.Json](https://github.com/Unity-Technologies/UnityNuGet/tree/master/com.unity.nuget.newtonsoft-json) | JSON 序列化 |
| TextMesh Pro | 文本渲染（UI 面板依赖） |

> 目前项目未使用 `.asmdef`，所有脚本编译进默认 `Assembly-CSharp`。

---

## 快速开始

### 编辑器

1. 打开菜单 **Tools > Dialog System** 打开编辑器窗口，点工具栏 **新建对话图** 创建图资产（或直接双击已有 `.asset`）；
2. 右键画布空白处 → **Create Sequence Node** 或 **Create Option Node** 创建节点（选项节点会预置 2 个默认选项）；
3. 从节点输出端口拖出连线；选中连线在右侧面板配置条件；
4. 右键节点 → **Set as Start Node** 设置起始节点；
5. 工具栏提供 **校验本图** / **导出 JSON** / **导入 JSON**，播放模式下还有 **播放当前图**（自动查找场景中的 `DialogueRunner` 并 `StartDialogue`）。

> 直接双击 `.asset` Graph 文件也会自动打开编辑器窗口。

### 运行时

1. 在场景中放置 `DialogueRunner` 组件，Inspector 指定 `DialogueGraph`；
2. 通过 `PanelDialogue` UI（挂载 `Button`，调用 `DialogueStep()`）或直接调用 `DialogueRunner` 公开 API 驱动。

```csharp
var runner = GetComponent<DialogueRunner>();
runner.StartDialogue();   // 从 StartNode 开始
runner.DialogueStep();    // 推进到下一个满足条件的节点
runner.SelectOption(0);   // 选项节点上选择第 0 项
runner.StopDialogue();
```

如果使用 UI 面板，也可以直接交给 `DialogueManager`：

```csharp
DialogueManager.Instance.SetRunner(runner); // 自动加载并显示 PanelDialogue
```

---

## 编辑器使用指南

### 窗口布局

`DialogueGraphEditorWindow` 继承 Odin `OdinMenuEditorWindow`，采用自定义 UIToolkit 三栏布局：

| 区域 | 内容 |
| --- | --- |
| **工具栏** | 新建对话图 / 校验本图 / 导出 JSON / 导入 JSON / 播放当前图 |
| **左侧** | OdinMenuTree（Graph / 节点树）+ Parameters 变量编辑器 |
| **中间** | GraphView 画布（网格背景、缩放、中键平移、右键菜单） |
| **右侧** | Odin PropertyTree Inspector（含资产重命名字段）；选中连线时显示 Transition 条件编辑器 |

三栏之间的分隔条可拖拽调整宽度。

### 节点

| 类型 | 颜色 | 用途 |
| --- | --- | --- |
| 序列节点 `DialogueNode` | 蓝 | 常规对话。数据层可含多条带条件的出口（`DialogueLink`），画布上显示单个 `Out` 端口 |
| 选项节点 `DialogueOptionNode` | 橙 | 展示多个选项（`DialogueChoice`），每个选项一个输出端口，玩家选择后跳转 |

- 标题栏显示事件 ⚡ 徽章（节点挂载了事件时出现）
- 起止节点带绿色边框
- 节点字段：`nodeId`、`speakType`（Node/Player/NPC）、`speakerName`、`dialogText`、节点事件列表

### 右键菜单

- **画布空白处**：`Create Sequence Node` / `Create Option Node`
- **节点上**：`Add Choice`（仅选项节点）/ `Add Node Event` / `Set as Start Node` / `Focus on Left Tree`
- 新建选项节点会预置 2 个默认选项（`选项1` / `选项2`），连线时若端口不足会自动补足

### 条件系统

1. 在左侧 **Parameters** 区域编辑 `DialogueVariables` 标志变量（key-value bool 对）：`+` 按钮添加、行内改名、切换布尔值、`×` 删除；
2. 选中一条连线，右侧显示 **Transition** 编辑器：`E_Condition`（None / BoolEquals）、`Key` 下拉选择变量、`Bool` 目标值；选项连线还可编辑 `Label` 文案；
3. 运行时 `DialogueLink.CanPass(vars)` / `DialogueChoice.CanPass(vars)` 按条件决定分支走向。

### 节点事件

1. 右键节点 → **Add Node Event** 创建 `DialogueEvent`（ScriptableObject，作为节点的 sub-asset 存储）；
2. 在 `DialogueEvent` 资产的 Inspector 中配置 `UnityEvent onNodeEnter`；
3. 运行时进入节点时调用 `InvokeNodeEvents()`，触发事件并广播给所有监听者（`SOEventListener`）；
4. 节点展开区会显示事件行（⚡ 徽章计数），点击事件行可 Ping 并选中事件资产，`×` 可删除事件。

### 校验与导入导出

- **校验本图**：检查节点空引用、连线断连、缺失节点、startNode 与选项空列表等（`DialogueGraphValidator`）；
- **导出 JSON**：通过纯 DTO（`DialogueGraphJsonModels`）与 Newtonsoft.Json 序列化，导出目录为 `Assets/DialogSystem/Export`，包含节点（布局 / 文本 / 发言类型）、连线、选项、条件、节点事件与 graphId；
- **导入 JSON**：可导入到已有图或直接新建图。导入到已有图时按 `nodeId` 合并 —— 同 id 更新、缺失 id 新建、JSON 中不存在的旧节点从图中移除（资产保留）

---

## 运行时 API

### DialogueRunner（MonoBehaviour）

驱动对话流程的核心组件。

**公开方法**

| 方法 | 说明 |
| --- | --- |
| `StartDialogue(DialogueGraph graph = null)` | 开始对话，从 `StartNode` 进入 |
| `StopDialogue()` | 结束对话 |
| `DialogueStep()` | 推进一步；若当前是选项节点，则生成选项 UI |
| `Advance()` | 沿首个满足条件的 `DialogueLink` 推进 |
| `SelectOption(int index)` | 选择第 index 个可用选项 |
| `RefreshAvailableChoices()` | 刷新可用选项列表 |

**公开属性**

| 属性 | 说明 |
| --- | --- |
| `CurrentNode` | 当前节点 |
| `Variables` | 运行时变量（从 Graph 复制而来） |
| `IsDialogueActive` | 是否正在对话中 |
| `AvailableChoices` | 当前可用选项列表 |

**事件**

| 事件 | 触发时机 |
| --- | --- |
| `OnDialogueStart` | 对话开始 |
| `OnDialogueEnd` | 对话结束 |
| `OnNodeChanged(DialogueNodeBase)` | 进入新节点 |
| `OnChoicesUpdated(IReadOnlyList<DialogueChoice>)` | 可用选项刷新 |

> 注：`Advance()`、`Space` 推进、`1~9` 选择等旧逻辑已注释，当前通过 `DialogueStep()` 与 UI 按钮驱动，可按需恢复。

### DialogueManager（静态单例）

非 MonoBehaviour 单例，负责加载并管理对话 UI。

| 方法 | 说明 |
| --- | --- |
| `Instance` | 获取单例，首次访问时从 `Resources` 实例化 `PanelDialogue` |
| `SetRunner(DialogueRunner)` | 设置当前 Runner 并显示对话面板 |
| `CreateDialogueOptions(index, choice, onOptionSelected)` | 创建选项按钮 |
| `ClearDialogueOptions()` | 清空所有选项按钮 |

### 运行时 UI（Resources 自动加载）

- **PanelDialogue.prefab**：对话面板。`TXT/Tmp_Content`（正文）、`TXT/Tmp_Name`（说话人）、`Options`（选项父节点）、主 `Button`（下一步）
- **DialogueOption.prefab**：选项按钮，`Txt_Option` 文本 + `Button`

> 两个 Prefab 必须放在 `Assets/DialogSystem/Resources/` 下（`Resources.Load` 依赖）。

### 变量 / 条件 / 事件

- `DialogueVariables`：`GetBool(key)` / `SetBool(key, value)` / `CopyFrom` / `GetAllKeys`
- `DialogueCondition`：`E_Condition`（`None` / `BoolEquals`），`MeetCondition(vars)`
- `DialogueLink`：节点出口，`toNode` + `condition`，`CanPass(vars)`
- `DialogueChoice`：选项，`labelText` + `toNode` + `condition`，`CanPass(vars)`
- `SOEventBase`：事件资产基类，`RegisterListener` / `UnregisterListener` / `RaiseEvent`
- `DialogueEvent`：`UnityEvent onNodeEnter`，进入节点时触发
- `SOEventListener`：MonoBehaviour，监听 `SOEventBase`，触发自身 `UnityEvent<object?>`

---

## 项目结构

```
Assets/DialogSystem/
├── Scene/DialogSystemScene.unity          # 主场景
├── NodeSo/                                # Graph .asset（节点作为 sub-asset）
├── Export/                                # JSON 导出目录
├── Resources/                             # 运行时 UI 预制体（Resources.Load）
│   ├── PanelDialogue.prefab
│   └── DialogueOption.prefab
├── Front/                                 # UI 资源（字体等）
└── Scripts/
    ├── Data/
    │   ├── DialogueNodeBase.cs            # 节点基类（nodeId / speakType / speakerName / dialogText / 节点事件）
    │   ├── DialogueNode.cs                # 序列节点（DialogueLink 出口列表）
    │   ├── DialogueOptionNode.cs          # 选项节点（DialogueChoice 选项列表）
    │   ├── DialogueGraph.cs               # 图资产（节点列表、布局、起止、变量）
    │   ├── DialogueGraphJsonModels.cs     # JSON 导入导出 DTO
    │   ├── DialogueEvent.cs               # 节点事件资产（UnityEvent onNodeEnter）
    │   └── SOEventBase.cs                 # 事件资产基类 + Editor 运行时监听器显示
    ├── Rumtime/                           # 注：目录名为 "Rumtime"（拼写错误），应为 Runtime
    │   ├── DialogueRunner.cs              # 运行时对话引擎
    │   ├── DialogueManager.cs             # 运行时 UI 管理器（单例）
    │   ├── PanelDialogue.cs               # 对话面板
    │   ├── DialogueOption.cs              # 选项按钮
    │   ├── DialogueSpeakEnums.cs          # 发言类型枚举
    │   ├── DialogueVariables.cs           # 布尔标志变量系统
    │   ├── DialogueCondition.cs           # 条件 / 连线 / 选项
    │   └── SOEventListener.cs             # 事件监听器
    └── Editor/
        ├── DialogueGraphEditorWindow.cs   # 三栏主窗口（UIToolkit + 可拖拽分隔条）
        ├── DialogueGraphView.cs           # GraphView 画布
        ├── DialogueEditorContext.cs       # 当前编辑上下文
        ├── DialogueEditorPaths.cs         # 资产路径常量
        ├── DialogueEditorPanelStyles.cs   # UI 主题样式
        ├── DialogueGraphJsonIO.cs         # JSON 序列化 / 反序列化
        └── DialogueGraphValidator.cs      # 图校验
```

---

## 已知问题

- `Assets/DialogSystem/Scripts/Rumtime/` 目录名拼写应为 `Runtime`
- `Assets/DialogSystem/Scripts/Test.cs` 是空的测试占位文件，可移除
- 无 `.asmdef`，所有脚本编译到默认 `Assembly-CSharp`
- 无单元测试

---

## 第三方依赖

- **Odin Inspector**（Sirenix）
- **DOTween Pro**（Demigiant）
- **Newtonsoft.Json**（com.unity.nuget.newtonsoft-json）
- **TextMesh Pro**

---
