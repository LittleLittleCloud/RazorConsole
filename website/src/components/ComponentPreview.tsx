import CodeBlock from "@/components/CodeBlock"
import XTermPreview from "@/components/XTermPreview"
import type { ComponentInfo } from "@/data/components"

export function ComponentPreview({ component }: { component: ComponentInfo }) {
  return (
    <div className="group relative my-4 flex flex-col space-y-4">
      <div className="relative rounded-md border border-slate-200 dark:border-slate-700">
        <div className="preview flex min-h-[350px] w-full justify-center p-4 items-center bg-slate-50 dark:bg-slate-900/50">
          <div className="w-full h-96 bg-slate-950 rounded-md overflow-hidden border border-slate-800 shadow-sm">
            <XTermPreview elementId={component.name} className="h-full" />
          </div>
        </div>
      </div>
      
      <div className="flex flex-col space-y-4">
        <div className="w-full rounded-md [&_pre]:my-0 [&_pre]:max-h-[350px] [&_pre]:overflow-auto">
          <CodeBlock code={component.example} language="razor" />
        </div>
      </div>
    </div>
  )
}
