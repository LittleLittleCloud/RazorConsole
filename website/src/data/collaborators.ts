export interface Collaborator {
  name: string
  github: string
  role: string
  bio: string
  avatar?: string
  x?: string
  linkedin?: string
  website?: string
}

export const collaborators: Collaborator[] = [
  {
    name: "Xiaoyun Zhang",
    github: "LittleLittleCloud",
    role: "Creator & Maintainer",
    bio: "Creator of RazorConsole. Passionate about building developer tools and bringing familiar web paradigms to console applications.",
  },
  {
    name: "Skoreyko Misha",
    github: "TeseySTD",
    role: "Collaborator, Component creator, Bug fixer.",
    bio: "Full-stack .NET developer | JS/TS & React frontend | RazorConsole collaborator",
  },
  {
    name: "FallenParadise",
    github: "ParadiseFallen",
    role: "Collaborator, OSS enjoyer.",
    bio: "TeamLead | Full-stack .NET/React dev | RazorConsole collaborator",
  },
]
