import { useState } from "react"
import { components, type ComponentInfo } from "@/data/components"
import { cn } from "@/lib/utils"
import { ComponentPreview } from "@/components/ComponentPreview"

export default function Components() {
  const [activeComponentName, setActiveComponentName] = useState<string | null>(null)

  const categories = ["Layout", "Input", "Display", "Utilities"]
  const groupedComponents = categories.reduce((acc, category) => {
    acc[category] = components.filter(c => c.category === category)
    return acc
  }, {} as Record<string, ComponentInfo[]>)

  const activeComponent = activeComponentName 
    ? components.find(c => c.name === activeComponentName) 
    : null

  return (
    <div className="container mx-auto px-4 py-8 md:py-12 lg:py-16">
      <div className="flex flex-col md:grid md:grid-cols-[220px_1fr] lg:grid-cols-[240px_1fr] gap-8">
        
        {/* Sidebar */}
        <aside className="hidden md:block w-full shrink-0">
            <div className="sticky top-24 h-[calc(100vh-8rem)] overflow-y-auto pr-4">
                <div className="pb-4">
                    <h4 className="mb-1 rounded-md px-2 py-1 text-sm font-semibold">
                        Getting Started
                    </h4>
                    <div className="grid grid-flow-row auto-rows-max text-sm">
                        <button
                            onClick={() => setActiveComponentName(null)}
                            className={cn(
                                "group flex w-full items-center rounded-md border border-transparent px-2 py-1 hover:underline text-muted-foreground",
                                !activeComponentName ? "font-medium text-foreground text-blue-600 dark:text-blue-400" : ""
                            )}
                        >
                            Overview
                        </button>
                    </div>
                </div>
                {categories.map(category => (
                    <div key={category} className="pb-4">
                        <h4 className="mb-1 rounded-md px-2 py-1 text-sm font-semibold">
                            {category}
                        </h4>
                        <div className="grid grid-flow-row auto-rows-max text-sm">
                            {groupedComponents[category]?.map(component => (
                                <button
                                    key={component.name}
                                    onClick={() => setActiveComponentName(component.name)}
                                    className={cn(
                                        "group flex w-full items-center rounded-md border border-transparent px-2 py-1 hover:underline text-muted-foreground text-left",
                                        activeComponentName === component.name ? "font-medium text-blue-600 dark:text-blue-400" : ""
                                    )}
                                >
                                    {component.name}
                                </button>
                            ))}
                        </div>
                    </div>
                ))}
            </div>
        </aside>

        {/* Main Content */}
        <main className="relative min-w-0">
            {activeComponent ? (
                <ComponentDetail component={activeComponent} />
            ) : (
                <ComponentsOverview onSelect={setActiveComponentName} />
            )}
        </main>
      </div>
    </div>
  )
}

function ComponentsOverview({ onSelect }: { onSelect: (name: string) => void }) {
    return (
        <div className="space-y-8">
            <div>
                <h1 className="text-4xl font-bold mb-4">Built-in Components</h1>
                <p className="text-slate-600 dark:text-slate-300 text-lg">
                    RazorConsole ships with 15+ ready-to-use components that wrap Spectre.Console constructs.
                </p>
            </div>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                {components.map(component => (
                    <div 
                        key={component.name} 
                        className="group relative rounded-lg border p-6 hover:border-slate-400 dark:hover:border-slate-600 transition-colors cursor-pointer"
                        onClick={() => onSelect(component.name)}
                    >
                        <h3 className="font-bold text-xl mb-2">{component.name}</h3>
                        <p className="text-sm text-slate-500 dark:text-slate-400 mb-4">{component.description}</p>
                        <span className="inline-flex items-center rounded-md bg-slate-100 px-2 py-1 text-xs font-medium text-slate-600 ring-1 ring-inset ring-slate-500/10 dark:bg-slate-800 dark:text-slate-400 dark:ring-slate-400/20">
                            {component.category}
                        </span>
                    </div>
                ))}
            </div>
        </div>
    )
}

function ComponentDetail({ component }: { component: ComponentInfo }) {
    return (
        <div className="space-y-8 animate-in fade-in slide-in-from-bottom-4 duration-500">
            <div>
                <div className="flex items-center space-x-4 mb-2">
                    <h1 className="text-3xl font-bold">{component.name}</h1>
                    <span className="inline-flex items-center rounded-md bg-blue-50 px-2 py-1 text-xs font-medium text-blue-700 ring-1 ring-inset ring-blue-700/10 dark:bg-blue-900/20 dark:text-blue-400 dark:ring-blue-400/30">
                        {component.category}
                    </span>
                </div>
                <p className="text-lg text-slate-600 dark:text-slate-300">
                    {component.description}
                </p>
            </div>

            <ComponentPreview component={component} />

            {component.parameters && (
                <div className="space-y-4">
                    <h3 className="text-xl font-semibold">Parameters</h3>
                    <div className="rounded-md border">
                        <table className="w-full text-sm">
                            <thead>
                                <tr className="border-b bg-slate-50 dark:bg-slate-900">
                                    <th className="text-left py-3 px-4 font-semibold">Prop</th>
                                    <th className="text-left py-3 px-4 font-semibold">Type</th>
                                    <th className="text-left py-3 px-4 font-semibold">Default</th>
                                    <th className="text-left py-3 px-4 font-semibold">Description</th>
                                </tr>
                            </thead>
                            <tbody>
                                {component.parameters.map((param, idx) => (
                                    <tr key={idx} className="border-b last:border-0">
                                        <td className="py-3 px-4 font-mono text-xs text-blue-600 dark:text-blue-400">
                                            {param.name}
                                        </td>
                                        <td className="py-3 px-4 font-mono text-xs text-slate-600 dark:text-slate-400">
                                            {param.type}
                                        </td>
                                        <td className="py-3 px-4 font-mono text-xs text-slate-600 dark:text-slate-400">
                                            {param.default || "—"}
                                        </td>
                                        <td className="py-3 px-4 text-slate-700 dark:text-slate-300">
                                            {param.description}
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    </div>
                </div>
            )}
        </div>
    )
}
