import { useEffect, useRef, useState } from 'react'
import { isTerminalAvailable, initTerminal, registerComponent, attachKeyListener, clearTerminal, disposeTerminal, handleKeyboardEvent } from 'razor-console'
import { Terminal } from 'xterm'
interface XTermPreviewProps {
  componentName: string
  className?: string
}

export default function XTermPreview({ componentName, className = '' }: XTermPreviewProps) {
  const terminalRef = useRef<HTMLDivElement>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const elementId = componentName

  useEffect(() => {
    let cancelled = false
    async function startPreview() {
      setError(null)
      setIsLoading(true)
      try {
        if (window.Terminal === undefined) {
          console.error('xterm.js is not loaded')
          window.Terminal = Terminal
        }
        initTerminal(elementId)
        if (!isTerminalAvailable()) {
          throw new Error('xterm.js not loaded')
        }

        if (!terminalRef.current) {
          console.error('Terminal host element was not found')
          throw new Error('Terminal host element was not found')
        }
        await registerComponent(elementId)

        attachKeyListener(elementId, {
          invokeMethodAsync: async (methodName: string, ...args: unknown[]) => {
            console.debug(`Key event forwarded from preview via ${methodName}`, args)
            await handleKeyboardEvent(...(args as [string, string, string, boolean, boolean, boolean]))
            return null
          }
        })


        if (!cancelled) {
          setIsLoading(false)
        }
      } catch (err) {
        if (!cancelled) {
          const message = err instanceof Error ? err.message : 'Failed to initialize preview'
          setError(message)
          setIsLoading(false)
        }
      }
    }

    startPreview()

    return () => {
      cancelled = true
      clearTerminal(elementId)
      disposeTerminal(elementId)
    }
  }, [componentName, elementId])

  if (error) {
    return (
      <div className="p-4 bg-red-50 dark:bg-red-900/20 text-red-600 dark:text-red-400 rounded">
        Error: {error}
      </div>
    )
  }

  return (
    <div className={`relative ${className}`}>
      {isLoading && (
        <div className="absolute inset-0 flex items-center justify-center bg-slate-900/50 text-white">
          Loading preview...
        </div>
      )}
      <div
        ref={terminalRef}
        id={elementId}
        className="w-full h-full bg-[#1e1e1e] rounded"
      />
    </div>
  )
}