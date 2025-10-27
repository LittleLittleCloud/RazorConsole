import { useMemo, useState, type HTMLAttributes, type ReactNode } from "react"
import ReactMarkdown from "react-markdown"
import remarkGfm from "remark-gfm"
import { cn } from "@/lib/utils"
import hotReloadDoc from "@/docs/hot-reload.md?raw"
import customTranslatorsDoc from "@/docs/custom-translators.md?raw"
import keyboardEventsDoc from "@/docs/keyboard-events.md?raw"
import focusManagementDoc from "@/docs/focus-management.md?raw"
import componentGalleryDoc from "@/docs/component-gallery.md?raw"

export default function Advanced() {
  const topics = useMemo(
    () => [
      { id: "hot-reload", title: "Hot Reload Support", content: hotReloadDoc },
      { id: "custom-translators", title: "Custom Translators", content: customTranslatorsDoc },
      { id: "keyboard-events", title: "Keyboard Events", content: keyboardEventsDoc },
      { id: "focus-management", title: "Focus Management", content: focusManagementDoc },
      { id: "component-gallery", title: "Component Gallery", content: componentGalleryDoc },
    ],
    []
  )

  const [activeTopicId, setActiveTopicId] = useState(topics[0]?.id ?? "")

  const activeTopic = useMemo(() => {
    return topics.find((topic) => topic.id === activeTopicId) ?? topics[0]
  }, [activeTopicId, topics])

  return (
    <div className="min-h-screen bg-gradient-to-b from-slate-50 to-white dark:from-slate-950 dark:to-slate-900">
      <div className="px-6 py-16 sm:px-10 lg:px-16">
        <header className="mb-12 border-b border-slate-200 pb-8 dark:border-slate-800">
          <p className="mb-3 text-sm font-semibold uppercase tracking-wider text-blue-600 dark:text-blue-400">
            Advanced Guide
          </p>
          <h1 className="text-4xl font-bold sm:text-5xl">Advanced Topics</h1>
          <p className="mt-4 w-full max-w-3xl text-lg text-slate-600 dark:text-slate-300">
            Deep dive into RazorConsole&apos;s power features with targeted walkthroughs, code samples, and best practices.
          </p>
        </header>

        <div className="flex flex-col gap-16 lg:flex-row lg:items-start">
          <aside className="top-32 w-full max-w-sm shrink-0 lg:sticky">
            <h2 className="mb-4 text-xs font-semibold uppercase tracking-[0.2em] text-slate-500 dark:text-slate-400">
              Topics
            </h2>
            <nav className="space-y-1">
              {topics.map((topic) => {
                const isActive = topic.id === activeTopic?.id

                return (
                  <button
                    key={topic.id}
                    type="button"
                    onClick={() => setActiveTopicId(topic.id)}
                    className={cn(
                      "w-full rounded-md px-3 py-2 text-left text-sm font-medium transition focus:outline-none focus-visible:ring-2 focus-visible:ring-blue-500 focus-visible:ring-offset-2 focus-visible:ring-offset-slate-50 dark:focus-visible:ring-offset-slate-900",
                      isActive
                        ? "bg-blue-500/10 text-blue-700 dark:bg-blue-500/20 dark:text-blue-100"
                        : "text-slate-700 hover:bg-slate-100 dark:text-slate-200 dark:hover:bg-slate-800"
                    )}
                    aria-pressed={isActive}
                  >
                    {topic.title}
                  </button>
                )
              })}
            </nav>
          </aside>

          <main className="flex-1">
            {activeTopic && (
              <article key={activeTopic.id} className="prose prose-slate max-w-none dark:prose-invert">
                <h2 className="mb-6 text-3xl font-semibold tracking-tight text-slate-900 dark:text-slate-100">
                  {activeTopic.title}
                </h2>
                <ReactMarkdown
                  remarkPlugins={[remarkGfm]}
                  components={{
                    code({ inline, className, children, ...props }: { inline?: boolean; className?: string; children?: ReactNode } & HTMLAttributes<HTMLElement>) {
                      const match = /language-(\w+)/.exec(className ?? "")
                      if (!inline) {
                        return (
                          <pre className="overflow-x-auto rounded-lg bg-slate-900 p-4 text-sm text-slate-100 shadow-inner">
                            <code className={cn(match ? `language-${match[1]}` : undefined)} {...props}>
                              {children}
                            </code>
                          </pre>
                        )
                      }

                      return (
                        <code
                          className={cn(
                            "rounded-md bg-slate-100 px-1.5 py-0.5 text-sm font-semibold text-slate-800 dark:bg-slate-800 dark:text-slate-200",
                            className
                          )}
                          {...props}
                        >
                          {children}
                        </code>
                      )
                    },
                  }}
                >
                  {activeTopic.content}
                </ReactMarkdown>
              </article>
            )}
          </main>
        </div>
      </div>
    </div>
  )
}
