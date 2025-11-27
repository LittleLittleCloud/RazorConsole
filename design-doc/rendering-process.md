# Rendering Process

This document describes the complete rendering pipeline in RazorConsole, from Razor component to console output.

## Overview

RazorConsole uses a multi-stage rendering pipeline that converts Razor components into console output:

1. **Component Rendering**: Blazor renders Razor components into a `RenderBatch`
2. **VDOM Construction**: `ConsoleRenderer` converts `RenderBatch` into a `VNode` tree
3. **Translation**: `VdomSpectreTranslator` converts `VNode` tree into Spectre.Console `IRenderable`
4. **Diff & Update**: `VdomDiffService` computes differences for efficient updates
5. **Console Output**: Spectre.Console renders to ANSI escape sequences

## Complete Rendering Flow

```mermaid
sequenceDiagram
    participant Component as Razor Component
    participant Blazor as Blazor Renderer
    participant Renderer as ConsoleRenderer
    participant Batch as RenderBatch
    participant VNode as VNode Tree
    participant Translator as VdomSpectreTranslator
    participant Renderable as IRenderable
    participant Diff as VdomDiffService
    participant LiveDisplay as ConsoleLiveDisplayContext
    participant Spectre as Spectre.Console
    participant Output as Console Output

    Note over Component,Output: Initial Render
    Component->>Blazor: Render component
    Blazor->>Batch: Create RenderBatch
    Batch->>Renderer: UpdateDisplayAsync(batch)
    Renderer->>Renderer: ApplyComponentEdits()
    Renderer->>Renderer: BuildSubtree()
    Renderer->>VNode: Create VNode tree
    VNode->>Translator: TryTranslate(vnode)
    Translator->>Translator: TryTranslateElement()
    Translator->>Renderable: Create IRenderable
    Renderable->>Renderer: CreateSnapshot()
    Renderer->>LiveDisplay: OnNext(snapshot)
    LiveDisplay->>Diff: Diff(previous, current)
    Diff->>LiveDisplay: VdomDiffResult
    alt Has changes
        LiveDisplay->>LiveDisplay: TryApplyMutations()
        LiveDisplay->>Spectre: UpdateTarget(renderable)
    else No changes
        LiveDisplay->>LiveDisplay: Skip update
    end
    Spectre->>Output: Write ANSI sequences

    Note over Component,Output: Re-render (State Change)
    Component->>Component: StateChanged()
    Component->>Blazor: RequestRender()
    Blazor->>Batch: Create new RenderBatch
    Batch->>Renderer: UpdateDisplayAsync(batch)
    Renderer->>Renderer: ApplyComponentEdits()
    Note right of Renderer: Only changed components are processed
    Renderer->>VNode: Update VNode tree
    VNode->>Translator: TryTranslate(vnode)
    Translator->>Renderable: Create new IRenderable
    Renderable->>Renderer: CreateSnapshot()
    Renderer->>LiveDisplay: OnNext(snapshot)
    LiveDisplay->>Diff: Diff(previous, current)
    Diff->>LiveDisplay: Mutations list
    LiveDisplay->>LiveDisplay: TryApplyMutations()
    alt Can apply mutations
        LiveDisplay->>Spectre: TryUpdateText/Attributes/ReplaceNode()
    else Full update
        LiveDisplay->>Spectre: UpdateTarget(renderable)
    end
    Spectre->>Output: Write ANSI sequences
```

## Component Lifecycle

```mermaid
stateDiagram-v2
    [*] --> Mount: Component Mount
    
    state Mount {
        [*] --> InstantiateComponent
        InstantiateComponent --> AssignComponentId
        AssignComponentId --> RenderRootComponent
    }
    
    state Render {
        RenderRootComponent --> BuildRenderTree
        BuildRenderTree --> CreateRenderBatch
        CreateRenderBatch --> UpdateDisplayAsync
        UpdateDisplayAsync --> BuildVNodeTree
        BuildVNodeTree --> TranslateToRenderable
        TranslateToRenderable --> CreateSnapshot
    }
    
    state Update {
        CreateSnapshot --> NotifyObservers
        NotifyObservers --> DiffVDOM
        DiffVDOM --> ApplyMutations
        ApplyMutations --> UpdateConsole
    }
    
    state Events {
        UpdateConsole --> WaitForInput
        WaitForInput --> KeyboardEvent
        KeyboardEvent --> DispatchEvent
        DispatchEvent --> StateChange
        StateChange --> Render
    }
    
    Mount --> Render
    Render --> Update
    Update --> Events
    Events --> Render: State change
```

## VDOM Construction Process

```mermaid
flowchart TD
    subgraph Batch["RenderBatch Processing"]
        RenderBatch[RenderBatch]
        Frames[ReferenceFrames]
        Updates[UpdatedComponents]
        Disposed[DisposedComponents]
    end
    
    subgraph Renderer["ConsoleRenderer"]
        Apply[ApplyComponentEdits]
        Build[BuildSubtree]
        Slot[GetOrCreateComponentSlot]
    end
    
    subgraph VNodeTree["VNode Tree"]
        VNode[(VNode)]
        Element[Element]
        Text[Text]
        Comp[Component]
        Region[Region]
    end
    
    RenderBatch --> Apply
    Apply --> Build
    Build --> Build
    Build -->|RenderTreeFrameType.Element| Element
    Build -->|RenderTreeFrameType.Text| Text
    Build -->|RenderTreeFrameType.Component| Comp
    Build -->|RenderTreeFrameType.Region| Region
    
    Element --> Slot
    Comp --> Slot
    Slot --> VNode
    
    note1[Frame types processed:<br/>- Element: Creates VElementNode<br/>- Text: Creates VTextNode<br/>- Markup: Converts HTML to VNode<br/>- Component: Creates placeholder<br/>- Region: Groups children]
    Build -.-> note1
```

## Translation Process

```mermaid
flowchart TD
    VNode[VNode Tree] --> Translate[TryTranslate]
    Translate -->|Switch by VNodeKind| Translate
    
    Translate -->|VNodeKind.Text| TextPath[Create Text]
    TextPath --> Renderable[IRenderable]
    
    Translate -->|VNodeKind.Element| TranslateElement[TryTranslateElement]
    TranslateElement --> Interface[IVdomElementTranslator]
    Interface -->|Priority order| Button[ButtonElementTranslator]
    Interface -->|Priority order| Panel[PanelElementTranslator]
    Interface -->|Priority order| TextTranslator[TextElementTranslator]
    Interface -->|Priority order| Custom[Custom Translators]
    
    Button -->|TryTranslate| Match{Translator matches?}
    Panel -->|TryTranslate| Match
    TextTranslator -->|TryTranslate| Match
    Custom -->|TryTranslate| Match
    
    Match -->|Yes| Context[TranslationContext]
    Context -->|Recursive call| Translate
    Context --> SpectreComp[Create Spectre component]
    SpectreComp --> Renderable
    
    Match -->|No| Fail[null - fail]
    
    Translate -->|VNodeKind.Component| Context2[TranslationContext]
    Context2 --> BlockInline[BlockInlineRenderable]
    BlockInline --> Renderable
    
    Translate --> Animated[Collect animated renderables]
    Animated --> Renderable
    
    note1[Translators are ordered by Priority.<br/>First matching translator wins.]
    Interface -.-> note1
```

## Diff and Update Process

```mermaid
sequenceDiagram
    participant Renderer as ConsoleRenderer
    participant Snapshot as RenderSnapshot
    participant LiveDisplay as ConsoleLiveDisplayContext
    participant DiffService as VdomDiffService
    participant DiffResult as VdomDiffResult
    participant Canvas as LiveDisplayCanvas
    participant Spectre as Spectre.Console

    Renderer->>Snapshot: CreateSnapshot()
    Snapshot->>LiveDisplay: OnNext(snapshot)
    LiveDisplay->>LiveDisplay: UpdateView(view)

    alt First render
        LiveDisplay->>Canvas: UpdateTarget(renderable)
        Canvas->>Spectre: Write(renderable)
    else Subsequent render
        LiveDisplay->>DiffService: Diff(previousRoot, currentRoot)
        DiffService->>DiffResult: Compute mutations
        
        alt Has changes
            DiffResult->>LiveDisplay: Mutations list
            LiveDisplay->>LiveDisplay: TryApplyMutations()
            
            loop For each mutation
                alt UpdateText
                    LiveDisplay->>Canvas: TryUpdateText(path, text)
                else UpdateAttributes
                    LiveDisplay->>Canvas: TryUpdateAttributes(path, attrs)
                else ReplaceNode
                    LiveDisplay->>LiveDisplay: TryReplaceNode()
                    LiveDisplay->>Canvas: TryReplaceNode(path, renderable)
                end
            end
            
            alt Mutations applied
                Canvas->>Spectre: Partial update
            else Fallback to full update
                Canvas->>Spectre: UpdateTarget(renderable)
            end
        else No changes
            LiveDisplay->>LiveDisplay: Skip update
        end
    end

    Spectre->>Spectre: Render to ANSI

    Note right of DiffService: Diff algorithm:<br/>1. Compare VNode trees recursively<br/>2. Detect text changes<br/>3. Detect attribute changes<br/>4. Detect structural changes<br/>5. Generate mutation list
```

## Event Handling and Re-render

```mermaid
sequenceDiagram
    participant User as User Input
    participant KbdMgr as KeyboardEventManager
    participant Focus as FocusManager
    participant Renderer as ConsoleRenderer
    participant Component as Component
    participant Callback as EventCallback

    User->>KbdMgr: HandleKeyAsync(keyInfo)
    KbdMgr->>Focus: GetCurrentFocus()
    Focus->>Focus: Get focus snapshot
    Focus->>KbdMgr: Focus target + handlers

    KbdMgr->>KbdMgr: Process key event
    alt Printable character
        KbdMgr->>KbdMgr: Update text buffer
        KbdMgr->>Renderer: DispatchEventAsync(handlerId, oninput)
    end

    KbdMgr->>Renderer: DispatchEventAsync(handlerId, onkeydown)
    Renderer->>Callback: Invoke event handler
    Callback->>Component: Update state
    Component->>Component: StateHasChanged()
    Component->>Renderer: Trigger re-render

    KbdMgr->>Renderer: DispatchEventAsync(handlerId, onkeyup)

    Renderer->>Renderer: UpdateDisplayAsync()
    Note right of Renderer: Re-render cycle starts
```

