export interface ShowcaseProject {
  name: string
  description: string
  github?: string
  website?: string
  image?: string
}

export const showcaseProjects: ShowcaseProject[] = [
  // Add your project here! Submit a PR to be featured.
  {
    name: "Waves",
    description: "GitHub Game Off 2025 entry - A console game built with RazorConsole.",
    github: "Skuzzle-UK/Waves",
    image: "https://raw.githubusercontent.com/Skuzzle-UK/Waves/main/coverimage.png",
  },
]
