export interface Collaborator {
  name: string
  github: string
  role: string
  bio: string
  avatar?: string
}

export const collaborators: Collaborator[] = [
  {
    name: "Xiaoyun Zhang",
    github: "LittleLittleCloud",
    role: "Creator & Maintainer",
    bio: "Creator of RazorConsole. Passionate about building developer tools and bringing familiar web paradigms to console applications.",
  },
]
