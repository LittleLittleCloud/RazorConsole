import { Github, Rocket } from "lucide-react"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { showcaseProjects } from "@/data/showcase"

export default function Showcase() {
  const getProjectUrl = (project: typeof showcaseProjects[0]) => {
    if (project.github) return `https://github.com/${project.github}`
    if (project.website) return project.website
    return undefined
  }

  return (
    <div className="min-h-screen bg-gradient-to-b from-slate-50 to-white dark:from-slate-950 dark:to-slate-900">
      <div className="container mx-auto px-4 py-16">
        <div className="text-center mb-12">
          <div className="flex items-center justify-center gap-4 mb-4">
            <h1 className="text-4xl font-bold tracking-tight text-slate-900 dark:text-slate-50">
              Showcase
            </h1>
            <a
              href="https://github.com/RazorConsole/RazorConsole/edit/main/website/src/data/showcase.ts"
              target="_blank"
              rel="noopener noreferrer"
            >
              <Button size="sm" className="gap-2">
                <Github className="w-4 h-4" />
                Add Your Project
              </Button>
            </a>
          </div>
          <p className="text-lg text-slate-600 dark:text-slate-300 max-w-2xl mx-auto">
            Discover projects built with RazorConsole
          </p>
        </div>

        {showcaseProjects.length > 0 ? (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 max-w-5xl mx-auto">
            {showcaseProjects.map((project) => {
              const projectUrl = getProjectUrl(project)
              return (
                <a
                  key={project.name}
                  href={projectUrl}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="block transition-transform hover:scale-[1.02]"
                >
                  <Card className="flex flex-col h-full cursor-pointer hover:shadow-lg transition-shadow">
                    {project.image && (
                      <div className="relative h-48 overflow-hidden rounded-t-lg">
                        <img
                          src={project.image}
                          alt={`${project.name} screenshot`}
                          className="w-full h-full object-cover"
                          loading="lazy"
                        />
                      </div>
                    )}
                    <CardHeader>
                      <CardTitle className="text-xl">{project.name}</CardTitle>
                    </CardHeader>
                    <CardContent className="flex-1 flex flex-col">
                      <CardDescription className="flex-1">
                        {project.description}
                      </CardDescription>
                    </CardContent>
                  </Card>
                </a>
              )
            })}
          </div>
        ) : (
          <div className="text-center py-12">
            <Rocket className="w-16 h-16 mx-auto text-slate-300 dark:text-slate-600 mb-4" />
            <p className="text-lg text-slate-600 dark:text-slate-400 mb-2">
              No projects showcased yet.
            </p>
            <p className="text-sm text-slate-500 dark:text-slate-500">
              Be the first to add your project!
            </p>
          </div>
        )}
      </div>
    </div>
  )
}
