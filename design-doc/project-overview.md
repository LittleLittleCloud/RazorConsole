# RazorConsole Project Overview

## Architecture

```mermaid
flowchart TB
    subgraph RazorComponents["Razor Components"]
        Razor[Razor .razor files]
        Blazor[Blazor Components]
    end
    
    subgraph Core["RazorConsole.Core"]
        subgraph Rendering["Rendering"]
            Renderer[ConsoleRenderer]
            LiveDisplay[ConsoleLiveDisplayContext]
            Batch[RenderBatch Processing]
        end
        
        subgraph VDOM["VDOM"]
            VNode[VNode Tree]
            Translator[VdomSpectreTranslator]
            Diff[VdomDiffService]
        end
        
        subgraph Input["Input"]
            Keyboard[KeyboardEventManager]
            Focus[FocusManager]
        end
        
        subgraph Components["Components"]
            BuiltIn[Built-in Components]
            Custom[Custom Translators]
        end
    end
    
    subgraph Spectre["Spectre.Console"]
        Renderable[IRenderable]
        ANSI[ANSI Output]
    end
    
    subgraph Output["Output"]
        Console[Console Application]
        Browser[Browser WASM]
    end
    
    Razor -->|Component definition| Blazor
    Blazor -->|Render component| Renderer
    Renderer -->|Process RenderBatch| Batch
    Batch -->|Build VNode tree| VNode
    VNode -->|Translate to IRenderable| Translator
    Translator -->|Use built-in translators| BuiltIn
    Translator -->|Use custom translators| Custom
    Translator -->|Create Spectre renderable| Renderable
    Renderable -->|Generate ANSI sequences| ANSI
    
    Renderer -->|RenderSnapshot| LiveDisplay
    LiveDisplay -->|Diff VNode trees| Diff
    Diff -->|Apply mutations| LiveDisplay
    LiveDisplay -->|Update display| Renderable
    
    User[User] -->|Keyboard input| Keyboard
    Keyboard -->|Get focus target| Focus
    Focus -->|Dispatch event| Renderer
    Renderer -->|EventCallback| Blazor
    Blazor -->|State change → re-render| Renderer
    
    ANSI -->|Native console output| Console
    ANSI -->|WASM interop output| Browser
    
    Note1[VNode tree:<br/>- Immutable structure<br/>- Component roots stored separately<br/>- Events captured in descriptors<br/>- Keys for efficient diffing]
    VNode -.-> Note1
    
    Note2[Translation chain:<br/>1. VNode tree<br/>2. Element translators by priority<br/>3. Spectre.Console IRenderable<br/>4. ANSI escape sequences]
    Translator -.-> Note2
```

## Project Structure

The project should contains the following files structure:

.github/
  workflows/ - CI/CD workflows
design-doc/
  project-overview.md - This document
  rendering-process.md - Complete rendering pipeline documentation
  vdom design.md - Virtual DOM architecture
  keyboard-events.md - Keyboard event handling
  wasm-js-interop.md - Browser/WASM interop
  syntax-highlighter.md - Syntax highlighter component design
src/
  RazorConsole.Core - Core library with rendering logic and Razor components
  RazorConsole.Gallery - Sample RazorConsole app
  RazorConsole.Tests - Unit tests for core library

Directory.Build.targets - Centralized build targets
Directory.Build.props - Centralized build properties
Directory.Packages.props - Centralized package versions

global.json - .NET SDK version
README.md - Project overview and instructions
LICENSE - License file
RazorConsole.slnx - Solution file

.gitignore - Git ignore file

## Key Components

### ConsoleRenderer
- Extends Blazor's `Renderer` base class
- Converts `RenderBatch` to `VNode` tree
- Manages component lifecycle
- Handles event dispatching

### VNode Tree
- Immutable virtual DOM representation
- Stores elements, text, components, and regions
- Captures events and attributes
- Enables efficient diffing

### VdomSpectreTranslator
- Translates VNode tree to Spectre.Console renderables
- Uses priority-based translator chain
- Supports custom translators
- Handles animated renderables

### KeyboardEventManager
- Processes console keyboard input
- Manages text buffers for input elements
- Dispatches events to focused components
- Integrates with FocusManager

### FocusManager
- Tracks focusable elements
- Manages focus traversal (Tab/Shift+Tab)
- Provides focus snapshots for event dispatch
- Handles focus change events
