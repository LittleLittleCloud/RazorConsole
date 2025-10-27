import { useEffect, useMemo, useState } from "react";
import { useLocation } from "react-router-dom";
import { cn } from "@/lib/utils";
import quickStartDoc from "@/docs/quick-start.md?raw";
import builtInComponentsDoc from "@/docs/built-in-components.md?raw";
import hotReloadDoc from "@/docs/hot-reload.md?raw";
import customTranslatorsDoc from "@/docs/custom-translators.md?raw";
import keyboardEventsDoc from "@/docs/keyboard-events.md?raw";
import focusManagementDoc from "@/docs/focus-management.md?raw";
import componentGalleryDoc from "@/docs/component-gallery.md?raw";
import { MarkdownRenderer } from "@/components/Markdown";

export default function Docs() {
    const topics = useMemo(
        () => [
            { id: "quick-start", title: "Quick Start", content: quickStartDoc },
            {
                id: "built-in-components",
                title: "Built-in Components",
                content: builtInComponentsDoc,
            },
            { id: "hot-reload", title: "Hot Reload", content: hotReloadDoc },
            {
                id: "custom-translators",
                title: "Custom Translators",
                content: customTranslatorsDoc,
            },
            {
                id: "keyboard-events",
                title: "Keyboard Events",
                content: keyboardEventsDoc,
            },
            {
                id: "focus-management",
                title: "Focus Management",
                content: focusManagementDoc,
            },
            {
                id: "component-gallery",
                title: "Component Gallery",
                content: componentGalleryDoc,
            },
        ],
        []
    );

    const location = useLocation();
    const [activeTopicId, setActiveTopicId] = useState(topics[0]?.id ?? "");

    useEffect(() => {
        const hash = location.hash.replace("#", "");
        if (hash && topics.some((topic) => topic.id === hash)) {
            setActiveTopicId(hash);
        } else if (!hash && topics[0]) {
            setActiveTopicId(topics[0].id);
        }
    }, [location, topics]);

    const activeTopic = useMemo(() => {
        return topics.find((topic) => topic.id === activeTopicId) ?? topics[0];
    }, [activeTopicId, topics]);

    const handleTopicSelect = (topicId: string) => {
        setActiveTopicId(topicId);
        if (typeof window !== "undefined") {
            window.history.replaceState(null, "", `#${topicId}`);
        }
    };

    return (
        <div className="min-h-screen bg-gradient-to-b from-slate-50 to-white dark:from-slate-950 dark:to-slate-900">
            <div className="px-6 py-16 sm:px-10 lg:px-16">
                <div className="flex flex-col gap-16 lg:flex-row lg:items-start">
                    <aside className="top-32 w-full max-w-sm shrink-0 space-y-4 lg:sticky">
                        <div>
                            <h2 className="mb-4 text-xs font-semibold uppercase tracking-[0.2em] text-slate-500 dark:text-slate-400">
                                Topics
                            </h2>
                            <nav className="space-y-1">
                                {topics.map((topic) => {
                                    const isActive =
                                        topic.id === activeTopic?.id;

                                    return (
                                        <button
                                            key={topic.id}
                                            type="button"
                                            onClick={() =>
                                                handleTopicSelect(topic.id)
                                            }
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
                                    );
                                })}
                            </nav>
                        </div>
                    </aside>

                    <main className="flex-1">
                        {activeTopic && (
                            <article
                                key={activeTopic.id}
                                className="prose prose-slate max-w-none dark:prose-invert"
                            >
                                <h2 className="mb-6 text-3xl font-semibold tracking-tight text-slate-900 dark:text-slate-100">
                                    {activeTopic.title}
                                </h2>
                                <MarkdownRenderer content={activeTopic.content} />
                            </article>
                        )}
                    </main>
                </div>
            </div>
        </div>
    );
}
