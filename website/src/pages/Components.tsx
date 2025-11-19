import { useState } from "react"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"
import { components, type ComponentInfo } from "@/data/components"
import CodeBlock from "@/components/CodeBlock"
export default function Components() {
  const [selectedCategory, setSelectedCategory] = useState<string>("all")

  const categories = ["all", "Layout", "Input", "Display", "Utilities"]

  const filteredComponents = selectedCategory === "all"
    ? components
    : components.filter(c => c.category === selectedCategory)

  return (
    <div className="min-h-screen bg-gradient-to-b from-slate-50 to-white dark:from-slate-950 dark:to-slate-900">
      <div className="container mx-auto px-4 py-16">
        <div className="mb-8">
          <h1 className="text-4xl font-bold mb-4">Built-in Components</h1>
          <p className="text-slate-600 dark:text-slate-300 text-lg">
            RazorConsole ships with 15+ ready-to-use components that wrap Spectre.Console constructs
          </p>
        </div>

        {/* Category Filter */}
        <Tabs defaultValue="all" className="mb-8" onValueChange={setSelectedCategory}>
          <TabsList>
            {categories.map(cat => (
              <TabsTrigger key={cat} value={cat}>
                {cat === "all" ? "All Components" : cat}
              </TabsTrigger>
            ))}
          </TabsList>
        </Tabs>

        {/* Components Grid */}
        <div className="grid grid-cols-1 gap-6">
          {filteredComponents.map((component) => (
            <ComponentCard key={component.name} component={component} />
          ))}
        </div>
      </div>
    </div>
  )
}

function ComponentCard({ component }: { component: ComponentInfo }) {
  return (
    <Card>
      <CardHeader>
        <div className="flex items-start justify-between">
          <div>
            <CardTitle className="text-2xl">{component.name}</CardTitle>
            <CardDescription className="mt-2">
              {component.description}
            </CardDescription>
          </div>
          <span className="inline-flex items-center rounded-md bg-blue-50 px-2 py-1 text-xs font-medium text-blue-700 ring-1 ring-inset ring-blue-700/10 dark:bg-blue-900/20 dark:text-blue-400 dark:ring-blue-400/30">
            {component.category}
          </span>
        </div>
      </CardHeader>
      <CardContent>
        <Tabs defaultValue="example">
          <TabsList className="mb-4">
            <TabsTrigger value="example">Example</TabsTrigger>
            {component.parameters && <TabsTrigger value="parameters">Parameters</TabsTrigger>}
          </TabsList>

          <TabsContent value="example">
            <div>
              <h4 className="font-semibold mb-2 text-sm">Usage Example</h4>
                <CodeBlock language={"razor"} code={component.example}/>
            </div>
          </TabsContent>

          {component.parameters && (
            <TabsContent value="parameters">
              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="border-b border-slate-200 dark:border-slate-700">
                      <th className="text-left py-2 px-3 font-semibold">Parameter</th>
                      <th className="text-left py-2 px-3 font-semibold">Type</th>
                      <th className="text-left py-2 px-3 font-semibold">Default</th>
                      <th className="text-left py-2 px-3 font-semibold">Description</th>
                    </tr>
                  </thead>
                  <tbody>
                    {component.parameters.map((param, idx) => (
                      <tr key={idx} className="border-b border-slate-100 dark:border-slate-800">
                        <td className="py-2 px-3 font-mono text-xs text-blue-600 dark:text-blue-400">
                          {param.name}
                        </td>
                        <td className="py-2 px-3 font-mono text-xs text-slate-600 dark:text-slate-400">
                          {param.type}
                        </td>
                        <td className="py-2 px-3 font-mono text-xs text-slate-600 dark:text-slate-400">
                          {param.default || "—"}
                        </td>
                        <td className="py-2 px-3 text-slate-700 dark:text-slate-300">
                          {param.description}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </TabsContent>
          )}
        </Tabs>
      </CardContent>
    </Card>
  )
}
