import { create } from "zustand"
import { useEffect } from "react"

type Theme = "light" | "dark" | "system"

type ThemeStore = {
    theme: Theme
    setTheme: (theme: Theme) => void
}

export const useTheme = create<ThemeStore>((set) => ({
    theme: (localStorage.getItem("theme") as Theme) || "system",
    setTheme: (theme: Theme) => {
        localStorage.setItem("theme", theme)
        set({ theme })
    },
}))

// Hook to apply theme changes to the DOM
export function useThemeEffect() {
    const theme = useTheme((state) => state.theme)

    useEffect(() => {
        const root = window.document.documentElement

        root.classList.remove("light", "dark")

        if (theme === "system") {
            const systemTheme = window.matchMedia("(prefers-color-scheme: dark)")
                .matches
                ? "dark"
                : "light"

            root.classList.add(systemTheme)
            return
        }

        root.classList.add(theme)
    }, [theme])
}
