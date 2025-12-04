import { useState } from "react"
import { ChevronLeft, ChevronRight, Rocket } from "lucide-react"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { showcaseProjects } from "@/data/showcase"

function ImageBanner({ imageUrls, alt }: { imageUrls: string[], alt: string }) {
  const [currentIndex, setCurrentIndex] = useState(0)

  if (imageUrls.length === 0) return null

  const goToPrevious = (e: React.MouseEvent) => {
    e.preventDefault()
    e.stopPropagation()
    setCurrentIndex((prev) => (prev === 0 ? imageUrls.length - 1 : prev - 1))
  }

  const goToNext = (e: React.MouseEvent) => {
    e.preventDefault()
    e.stopPropagation()
    setCurrentIndex((prev) => (prev === imageUrls.length - 1 ? 0 : prev + 1))
  }

  return (
    <div className="relative h-72 overflow-hidden rounded-t-lg group">
      <img
        src={imageUrls[currentIndex]}
        alt={`${alt} screenshot ${currentIndex + 1}`}
        className="w-full h-full object-cover"
        loading="lazy"
      />
      {imageUrls.length > 1 && (
        <>
          <button
            onClick={goToPrevious}
            className="absolute left-2 top-1/2 -translate-y-1/2 bg-black/50 hover:bg-black/70 text-white p-1 rounded-full opacity-0 group-hover:opacity-100 transition-opacity"
            aria-label="Previous image"
          >
            <ChevronLeft className="w-4 h-4" />
          </button>
          <button
            onClick={goToNext}
            className="absolute right-2 top-1/2 -translate-y-1/2 bg-black/50 hover:bg-black/70 text-white p-1 rounded-full opacity-0 group-hover:opacity-100 transition-opacity"
            aria-label="Next image"
          >
            <ChevronRight className="w-4 h-4" />
          </button>
          <div className="absolute bottom-2 left-1/2 -translate-x-1/2 flex gap-1">
            {imageUrls.map((_, index) => (
              <button
                key={index}
                onClick={(e) => {
                  e.preventDefault()
                  e.stopPropagation()
                  setCurrentIndex(index)
                }}
                className={`w-2 h-2 rounded-full transition-colors ${
                  index === currentIndex ? "bg-white" : "bg-white/50"
                }`}
                aria-label={`Go to image ${index + 1}`}
              />
            ))}
          </div>
        </>
      )}
    </div>
  )
}

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
          <h1 className="text-4xl font-bold tracking-tight text-slate-900 dark:text-slate-50 mb-4">
            Showcase
          </h1>
          <p className="text-lg text-slate-600 dark:text-slate-300 max-w-2xl mx-auto">
            Discover projects built with RazorConsole
          </p>
        </div>

        {showcaseProjects.length > 0 ? (
          <div className="grid grid-cols-1 md:grid-cols-2 gap-8 max-w-6xl mx-auto">
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
                    {project.imageUrls && project.imageUrls.length > 0 && (
                      <ImageBanner imageUrls={project.imageUrls} alt={project.name} />
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
