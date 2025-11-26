import { useEffect, useMemo, useState } from "react";
import { useLocation } from "react-router-dom";
import { cn } from "@/lib/utils";
import { ChevronDown, ChevronRight } from "lucide-react";
import quickStartDoc from "@/docs/quick-start.md?raw";
import builtInComponentsDoc from "@/docs/built-in-components.md?raw";
import hotReloadDoc from "@/docs/hot-reload.md?raw";
import customTranslatorsDoc from "@/docs/custom-translators.md?raw";
import keyboardEventsDoc from "@/docs/keyboard-events.md?raw";
import focusManagementDoc from "@/docs/focus-management.md?raw";
import vdomDebuggingDoc from "@/docs/vdom-debugging.md?raw";
import routingDoc from "@/docs/routing.md?raw";
import componentGalleryDoc from "@/docs/component-gallery.md?raw";
import v0_1_1ReleaseNotes from "../../../release-notes/v0.1.1.md?raw";
import v0_2_0ReleaseNotes from "../../../release-notes/v0.2.0.md?raw";
import { MarkdownRenderer } from "@/components/Markdown";

interface TopicItem {
    id: string;
    title: string;
    content: string;
    filePath: string;
}

function getFilePathForTopic(
    topicId: string,
    topics: TopicItem[],
    releaseNotes: TopicItem[]
): string {
    // Find the topic or release note and return its filePath
    const topic =
        topics.find((t) => t.id === topicId) ||
        releaseNotes.find((r) => r.id === topicId);
    return topic?.filePath || "";
}

export default function Docs() {
    const topics = useMemo(
        () => [
            {
                id: "quick-start",
                title: "Quick Start",
                content: quickStartDoc,
                filePath: "website/src/docs/quick-start.md",
            },
            {
                id: "built-in-components",
                title: "Built-in Components",
                content: builtInComponentsDoc,
                filePath: "website/src/docs/built-in-components.md",
            },
            {
                id: "hot-reload",
                title: "Hot Reload",
                content: hotReloadDoc,
                filePath: "website/src/docs/hot-reload.md",
            },
            {
                id: "custom-translators",
                title: "Custom Translators",
                content: customTranslatorsDoc,
                filePath: "website/src/docs/custom-translators.md",
            },
            {
                id: "keyboard-events",
                title: "Keyboard Events",
                content: keyboardEventsDoc,
                filePath: "website/src/docs/keyboard-events.md",
            },
            {
                id: "focus-management",
                title: "Focus Management",
                content: focusManagementDoc,
                filePath: "website/src/docs/focus-management.md",
            },
            {
                id: "vdom-debugging",
                title: "VDom Tree Debugging",
                content: vdomDebuggingDoc,
                filePath: "website/src/docs/vdom-debugging.md",
            },
            {
                id: "cli-routing",
                title: "Routing",
                content: routingDoc,
                filePath: "website/src/docs/routing.md",
            },
            {
                id: "component-gallery",
                title: "Component Gallery",
                content: componentGalleryDoc,
                filePath: "website/src/docs/component-gallery.md",
            },
        ],
        []
    );

    const releaseNotes = useMemo(
        () => [
            {
                id: "v0.2.0",
                title: "v0.2.0",
                content: v0_2_0ReleaseNotes,
                filePath: "release-notes/v0.2.0.md",
            },
            {
                id: "v0.1.1",
                title: "v0.1.1",
                content: v0_1_1ReleaseNotes,
                filePath: "release-notes/v0.1.1.md",
            },
        ],
        []
    );

    const location = useLocation();
    const [activeTopicId, setActiveTopicId] = useState(topics[0]?.id ?? "");
    const [releaseNotesExpanded, setReleaseNotesExpanded] = useState(false);

    useEffect(() => {
        const hash = location.hash.replace("#", "");
        if (hash && topics.some((topic) => topic.id === hash)) {
            setActiveTopicId(hash);
        } else if (hash && releaseNotes.some((note) => note.id === hash)) {
            setActiveTopicId(hash);
            setReleaseNotesExpanded(true);
        } else if (!hash && topics[0]) {
            setActiveTopicId(topics[0].id);
        }
    }, [location, topics, releaseNotes]);

    const activeTopic = useMemo(() => {
        return (
            topics.find((topic) => topic.id === activeTopicId) ??
            releaseNotes.find((note) => note.id === activeTopicId) ??
            topics[0]
        );
    }, [activeTopicId, topics, releaseNotes]);

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

                        {/* Release Notes Section */}
                        <div>
                            <button
                                type="button"
                                onClick={() =>
                                    setReleaseNotesExpanded(
                                        !releaseNotesExpanded
                                    )
                                }
                                className="mb-2 flex w-full items-center justify-between text-xs font-semibold uppercase tracking-[0.2em] text-slate-500 hover:text-slate-700 dark:text-slate-400 dark:hover:text-slate-200 transition-colors"
                            >
                                <span>Release Notes</span>
                                {releaseNotesExpanded ? (
                                    <ChevronDown className="h-3 w-3" />
                                ) : (
                                    <ChevronRight className="h-3 w-3" />
                                )}
                            </button>
                            {releaseNotesExpanded && (
                                <nav className="space-y-1">
                                    {releaseNotes.map((note) => {
                                        const isActive =
                                            note.id === activeTopic?.id;

                                        return (
                                            <button
                                                key={note.id}
                                                type="button"
                                                onClick={() =>
                                                    handleTopicSelect(note.id)
                                                }
                                                className={cn(
                                                    "w-full rounded-md px-3 py-2 text-left text-sm font-medium transition focus:outline-none focus-visible:ring-2 focus-visible:ring-blue-500 focus-visible:ring-offset-2 focus-visible:ring-offset-slate-50 dark:focus-visible:ring-offset-slate-900",
                                                    isActive
                                                        ? "bg-blue-500/10 text-blue-700 dark:bg-blue-500/20 dark:text-blue-100"
                                                        : "text-slate-700 hover:bg-slate-100 dark:text-slate-200 dark:hover:bg-slate-800"
                                                )}
                                                aria-pressed={isActive}
                                            >
                                                {note.title}
                                            </button>
                                        );
                                    })}
                                </nav>
                            )}
                        </div>
                    </aside>

                    <main className="flex-1">
                        {activeTopic && (
                            <article
                                key={activeTopic.id}
                                className="prose prose-slate max-w-none dark:prose-invert"
                            >
                                <MarkdownRenderer
                                    content={activeTopic.content}
                                />
                                {(() => {
                                    const filePath = getFilePathForTopic(
                                        activeTopic.id,
                                        topics,
                                        releaseNotes
                                    );
                                    return filePath != null && (
                                        <div className="mt-8 pt-6 border-t border-slate-200 dark:border-slate-700 not-prose">
                                            <a
                                                href={`https://github.com/LittleLittleCloud/RazorConsole/edit/main/${filePath}`}
                                                target="_blank"
                                                rel="noopener noreferrer"
                                                className="flex items-center gap-2 text-sm text-slate-600 hover:text-slate-900 dark:text-slate-400 dark:hover:text-slate-100 transition-colors"
                                            >
                                                <svg
                                                    className="w-4 h-4"
                                                    fill="currentColor"
                                                    viewBox="0 0 20 20"
                                                >
                                                    <path d="M13.586 3.586a2 2 0 112.828 2.828l-.793.793-2.828-2.828.793-.793zM11.379 5.793L3 14.172V17h2.828l8.38-8.379-2.83-2.828z" />
                                                </svg>
                                                Edit on GitHub
                                            </a>
                                        </div>
                                    );
                                })()}
                            </article>
                        )}
                    </main>
                </div>
            </div>
        </div>
    );
}
