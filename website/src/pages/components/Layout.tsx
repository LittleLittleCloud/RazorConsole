import { Outlet, NavLink } from "react-router-dom"
import { components, type ComponentInfo } from "@/data/components"
import { cn } from "@/lib/utils"

export default function ComponentsLayout() {
  const categories = ["Layout", "Input", "Display", "Utilities"]
  const groupedComponents = categories.reduce((acc, category) => {
    acc[category] = components.filter(c => c.category === category)
    return acc
  }, {} as Record<string, ComponentInfo[]>)

  return (
    <div className="container mx-auto px-4 py-8 md:py-12 lg:py-16">
      <div className="flex flex-col md:grid md:grid-cols-[220px_1fr] lg:grid-cols-[240px_1fr] gap-8">

        {/* Sidebar */}
        <aside className="hidden md:block w-full shrink-0">
            <div className="sticky top-24 h-[calc(90vh-3rem)] overflow-y-auto pr-4">
                <div className="pb-4">
                    <h4 className="mb-1 rounded-md px-2 py-1 text-sm font-semibold">
                        Getting Started
                    </h4>
                    <div className="grid grid-flow-row auto-rows-max text-sm">
                        <NavLink
                            to="/components"
                            end
                            className={({ isActive }) => cn(
                                "group flex w-full items-center rounded-md border border-transparent px-2 py-1 hover:underline text-muted-foreground",
                                isActive ? "font-medium text-foreground text-blue-600 dark:text-blue-400" : ""
                            )}
                        >
                            Overview
                        </NavLink>
                    </div>
                </div>
                {categories.map(category => (
                    <div key={category} className="pb-4">
                        <h4 className="mb-1 rounded-md px-2 py-1 text-sm font-semibold">
                            {category}
                        </h4>
                        <div className="grid grid-flow-row auto-rows-max text-sm">
                            {groupedComponents[category]?.map(component => (
                                <NavLink
                                    key={component.name}
                                    to={`/components/${component.name.toLowerCase()}`}
                                    className={({ isActive }) => cn(
                                        "group flex w-full items-center rounded-md border border-transparent px-2 py-1 hover:underline text-muted-foreground text-left",
                                        isActive ? "font-medium text-blue-600 dark:text-blue-400" : ""
                                    )}
                                >
                                    {component.name}
                                </NavLink>
                            ))}
                        </div>
                    </div>
                ))}
            </div>
        </aside>

        {/* Main Content */}
        <main className="relative min-w-0">
            <Outlet />
        </main>
      </div>
    </div>
  )
}
