import {useEffect, useState} from 'react';
import {codeToHtml, type BundledLanguage} from 'shiki';
import { useTheme } from '@/components/ThemeProvider';
import { CopyButton } from '@/components/CopyButton';

interface CodeBlockProps {
    code: string;
    language?: BundledLanguage;
    showCopy?: boolean;
}


function CodeBlock(
    {
        code,
        language = 'csharp',
        showCopy = true
    }: CodeBlockProps) {
    const [html, setHtml] = useState('');
    const theme = useTheme((s) => s.theme);
    const resolvedTheme = theme === 'system'
        ? window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light'
        : theme;

    useEffect(() => {
        codeToHtml(code, {
            lang: language,
            theme: resolvedTheme === 'dark' ? 'github-dark' : 'github-light',
        }).then((generatedHtml) => {
            // remove inline background styles
            const cleanHtml = generatedHtml
                .replace(/background-color:[^;"]+;?/g, '')
                .replace(/background:[^;"]+;?/g, '');
            setHtml(cleanHtml);
        });
    }, [code, language, resolvedTheme]);


    return (
        <pre className="relative group my-6 overflow-hidden rounded-xl border bg-slate-100 dark:bg-slate-900 p-4 text-sm border-slate-200 dark:border-slate-700 overflow-x-auto">
          {showCopy && (
            <div className="absolute top-3 right-3 z-10 opacity-0 group-hover:opacity-100 transition-opacity">
              <CopyButton content={code} />
            </div>
          )}

          <div
            className="[&_code]:leading-relaxed [&_code]:text-sm"
            dangerouslySetInnerHTML={{ __html: html }}
          />
        </pre>

    );
}

export default CodeBlock;
