import { Github } from "lucide-react"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { collaborators } from "@/data/collaborators"

export default function Collaborators() {
  return (
    <div className="min-h-screen bg-gradient-to-b from-slate-50 to-white dark:from-slate-950 dark:to-slate-900">
      <div className="container mx-auto px-4 py-16">
        <div className="text-center mb-12">
          <h1 className="text-4xl font-bold tracking-tight text-slate-900 dark:text-slate-50 mb-4">
            Collaborators
          </h1>
          <p className="text-lg text-slate-600 dark:text-slate-300 max-w-2xl mx-auto">
            Meet the people who make RazorConsole possible
          </p>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 max-w-5xl mx-auto">
          {collaborators.map((collaborator) => (
            <Card key={collaborator.github} className="flex flex-col">
              <CardHeader className="text-center">
                <div className="mx-auto mb-4">
                  <img
                    src={collaborator.avatar ?? `https://github.com/${collaborator.github}.png`}
                    alt={`${collaborator.name}'s avatar`}
                    className="w-24 h-24 rounded-full border-2 border-slate-200 dark:border-slate-700"
                    loading="lazy"
                    onError={(e) => {
                      e.currentTarget.src = `https://ui-avatars.com/api/?name=${encodeURIComponent(collaborator.name)}&background=random`
                    }}
                  />
                </div>
                <CardTitle className="text-xl">{collaborator.name}</CardTitle>
                <CardDescription className="text-sm font-medium text-blue-600 dark:text-blue-400">
                  {collaborator.role}
                </CardDescription>
              </CardHeader>
              <CardContent className="flex-1 flex flex-col">
                <p className="text-sm text-slate-600 dark:text-slate-400 flex-1 text-center">
                  {collaborator.bio}
                </p>
                <div className="mt-4 flex justify-center">
                  <a
                    href={`https://github.com/${collaborator.github}`}
                    target="_blank"
                    rel="noopener noreferrer"
                  >
                    <Button variant="outline" size="sm" className="gap-2">
                      <Github className="w-4 h-4" />
                      @{collaborator.github}
                    </Button>
                  </a>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>

        <div className="mt-16 text-center">
          <Card className="max-w-2xl mx-auto">
            <CardHeader>
              <CardTitle className="text-xl">Want to Contribute?</CardTitle>
            </CardHeader>
            <CardContent>
              <p className="text-slate-600 dark:text-slate-400 mb-4">
                RazorConsole is an open-source project and we welcome contributions from the community!
              </p>
              <div className="flex gap-4 justify-center flex-wrap">
                <a
                  href="https://github.com/LittleLittleCloud/RazorConsole/blob/main/CONTRIBUTING.md"
                  target="_blank"
                  rel="noopener noreferrer"
                >
                  <Button variant="outline" className="gap-2">
                    Contributing Guide
                  </Button>
                </a>
                <a
                  href="https://discord.gg/DphHAnJxCM"
                  target="_blank"
                  rel="noopener noreferrer"
                >
                  <Button variant="secondary" className="gap-2">
                    Join Discord
                  </Button>
                </a>
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  )
}
