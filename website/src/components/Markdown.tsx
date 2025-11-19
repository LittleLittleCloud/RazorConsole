import React from "react";
import ReactMarkdown from "react-markdown";
import remarkGfm from "remark-gfm";
import CodeBlock from "./CodeBlock";
import type {BundledLanguage} from "shiki";

interface MarkdownRendererProps {
  content: string;
  className?: string;
}

export const MarkdownRenderer: React.FC<MarkdownRendererProps> = ({
  content,
  className = "",
}) => {
  const remarkPlugins: any[] = [remarkGfm];


  return (
    <div className={`markdown-renderer ${className}`}>
      <ReactMarkdown
        remarkPlugins={remarkPlugins}
        rehypePlugins={[]}
        components={{
          // Customize code blocks
          code: ({ className, children, ...props }: any) => {
            const match = /language-(\w+)/.exec(className || "");
            const language =   match ? match[1] as BundledLanguage  : undefined;
            const inline = !className?.includes("language-");
            const codeContent = String(children).replace(/\n$/, "");

            if (inline || !match) {
              return (
                <code
                  className="bg-gray-100 dark:bg-gray-800 px-1 py-0.5 rounded text-sm font-mono text-gray-800 dark:text-gray-200"
                  {...props}
                >
                  {children}
                </code>
              );
            }

            return (
              <CodeBlock
                code={codeContent}
                language={language }
                showCopy={true}
              />
            );
          },


          // Customize tables
          table: ({ children }) => (
            <div className="overflow-x-auto my-4">
              <table className="min-w-full border-collapse border border-gray-300 dark:border-gray-600">
                {children}
              </table>
            </div>
          ),

          th: ({ children }) => (
            <th className="border border-gray-300 dark:border-gray-600 bg-gray-50 dark:bg-gray-800 px-4 py-2 text-left font-semibold">
              {children}
            </th>
          ),

          td: ({ children }) => (
            <td className="border border-gray-300 dark:border-gray-600 px-4 py-2">
              {children}
            </td>
          ),

          // Customize headings
          h1: ({ children }) => (
            <h1 className="text-2xl font-bold mb-4 mt-6 first:mt-0 border-b border-gray-200 dark:border-gray-700 pb-2">
              {children}
            </h1>
          ),

          h2: ({ children }) => (
            <h2 className="text-xl font-bold mb-3 mt-5 first:mt-0">
              {children}
            </h2>
          ),

          h3: ({ children }) => (
            <h3 className="text-lg font-bold mb-2 mt-4 first:mt-0">
              {children}
            </h3>
          ),

          h4: ({ children }) => (
            <h4 className="text-base font-bold mb-2 mt-3 first:mt-0">
              {children}
            </h4>
          ),

          // Customize paragraphs
          p: ({ children }) => (
            <p className="mb-4 last:mb-0 leading-relaxed">{children}</p>
          ),

          // Customize lists
          ul: ({ children }) => (
            <ul className="list-disc list-inside mb-4 space-y-1">{children}</ul>
          ),

          ol: ({ children }) => (
            <ol className="list-decimal list-inside mb-4 space-y-1">
              {children}
            </ol>
          ),

          li: ({ children }) => <li className="leading-relaxed">{children}</li>,

          // Customize blockquotes
          blockquote: ({ children }) => (
            <blockquote className="border-l-4 border-gray-300 dark:border-gray-600 pl-4 italic my-4 text-gray-700 dark:text-gray-300">
              {children}
            </blockquote>
          ),

          // Customize links
          a: ({ href, children }) => (
            <a
              href={href}
              className="text-blue-600 dark:text-blue-400 hover:text-blue-800 dark:hover:text-blue-300 underline"
              target="_blank"
              rel="noopener noreferrer"
            >
              {children}
            </a>
          ),

          // Customize horizontal rules
          hr: () => (
            <hr className="my-6 border-gray-300 dark:border-gray-600" />
          ),
        }}
      >
        {content}
      </ReactMarkdown>
    </div>
  );
};
