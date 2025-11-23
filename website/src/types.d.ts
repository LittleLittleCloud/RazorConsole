declare module "*.md?raw" {
  const content: string
  export default content
}

// declare module 'razor-console' {
//   export type TerminalComponentName = string

//   export function registerComponent(componentName: TerminalComponentName): Promise<void>
//   export function writeToTerminal(componentName: TerminalComponentName, data: string): Promise<void>
//   export function initTerminal(componentName: TerminalComponentName, options?: Record<string, unknown>): void
//   export function clearTerminal(componentName: TerminalComponentName): void
//   export function disposeTerminal(componentName: TerminalComponentName): void
//   export function attachKeyListener(
//     componentName: TerminalComponentName,
//     helper: { invokeMethodAsync: (methodName: string, ...args: unknown[]) => Promise<unknown> }
//   ): void
//   export function isTerminalAvailable(): boolean
// }

declare module '/wasm/wwwroot/main.js' {
  export * from 'razor-console'
}
