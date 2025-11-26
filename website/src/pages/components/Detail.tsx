import {useParams, Navigate} from "react-router-dom"
import {components} from "@/data/components"
import {ComponentPreview} from "@/components/ComponentPreview"

export default function ComponentDetail() {
    const {name} = useParams()
    const component = components.find(c => c.name.toLowerCase() === name?.toLowerCase())

    if (!component) {
        return <Navigate to="/components" replace/>
    }

    return (
        <div className="space-y-8 animate-in fade-in slide-in-from-bottom-4 duration-500">
            <div>
                <div className="flex items-center space-x-4 mb-2">
                    <h1 className="text-3xl font-bold">{component.name}</h1>
                    <span
                        className="inline-flex items-center rounded-md bg-blue-50 px-2 py-1 text-xs font-medium text-blue-700 ring-1 ring-inset ring-blue-700/10 dark:bg-blue-900/20 dark:text-blue-400 dark:ring-blue-400/30">
                        {component.category}
                    </span>
                </div>
                <p className="text-lg text-slate-600 dark:text-slate-300">
                    {component.description}
                </p>
            </div>

            <ComponentPreview component={component}/>

            {component.parameters && (
                <div className="space-y-4">
                    <h3 className="text-xl font-semibold">Parameters</h3>

                    <div className="overflow-x-auto rounded-md border bg-card">
                        <table
                            className="w-full min-w-[600px] text-sm">
                            <thead>
                            <tr className="border-b bg-muted/50">
                                <th className="text-left py-3 px-4 font-semibold">Prop</th>
                                <th className="text-left py-3 px-4 font-semibold">Type</th>
                                <th className="text-left py-3 px-4 font-semibold">Default</th>
                                <th className="text-left py-3 px-4 font-semibold">Description</th>
                            </tr>
                            </thead>
                            <tbody>
                            {component.parameters.map((param, idx) => (
                                <tr key={idx} className="border-b last:border-0 hover:bg-muted/30 transition-colors">
                                    <td className="py-3 px-4 font-mono text-xs text-blue-600 dark:text-blue-400 break-all">
                                        {param.name}
                                    </td>
                                    <td className="py-3 px-4 font-mono text-xs text-slate-600 dark:text-slate-400 break-all">
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
