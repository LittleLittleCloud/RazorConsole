import { useEffect, useRef, useState } from 'react'
import { disposeTerminal, isTerminalAvailable } from '@/lib/terminalInterop'
import { registerComponent } from 'razor-console'
interface XTermPreviewProps {
  componentName: string
  className?: string
}

export default function XTermPreview({ componentName, className = '' }: XTermPreviewProps) {
  const terminalRef = useRef<HTMLDivElement>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const elementId = `terminal-${componentName.toLowerCase()}`

  useEffect(() => {
    let cancelled = false
    let disposePreview: (() => void) | undefined

    async function startPreview() {
      setError(null)
      setIsLoading(true)
      try {
        if (!isTerminalAvailable()) {
          throw new Error('xterm.js not loaded')
        }

        console.log('Registering component:', componentName);

        await registerComponent(componentName);

        if (!cancelled) {
          setIsLoading(false)
        }
      } catch (err) {
        if (!cancelled) {
          const message = err instanceof Error ? err.message : 'Failed to initialize preview'
          setError(message)
          setIsLoading(false)
        }

        if (disposePreview) {
          disposePreview()
          disposePreview = undefined
        }
      }
    }

    startPreview()

    return () => {
      cancelled = true
      if (disposePreview) {
        disposePreview()
      }
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
