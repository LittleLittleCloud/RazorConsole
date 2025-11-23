import { Link } from "react-router-dom"
import { components } from "@/data/components"

export default function ComponentsOverview() {
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
                    <Link 
                        key={component.name} 
                        to={`/components/${component.name.toLowerCase()}`}
                        className="group relative rounded-lg border p-6 hover:border-slate-400 dark:hover:border-slate-600 transition-colors cursor-pointer block"
                    >
                        <h3 className="font-bold text-xl mb-2">{component.name}</h3>
                        <p className="text-sm text-slate-500 dark:text-slate-400 mb-4">{component.description}</p>
                        <span className="inline-flex items-center rounded-md bg-slate-100 px-2 py-1 text-xs font-medium text-slate-600 ring-1 ring-inset ring-slate-500/10 dark:bg-slate-800 dark:text-slate-400 dark:ring-slate-400/20">
                            {component.category}
                        </span>
                    </Link>
                ))}
            </div>
        </div>
    )
}
