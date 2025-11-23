import { useState, useEffect } from 'react'

interface GitHubRepo {
  stargazers_count: number
}

export function useGitHubStars(owner: string, repo: string) {
  const [stars, setStars] = useState<number | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<Error | null>(null)

  useEffect(() => {
    const fetchStars = async () => {
      try {
        const response = await fetch(`https://api.github.com/repos/${owner}/${repo}`)
        if (!response.ok) {
          throw new Error('Failed to fetch repository data')
        }
        const data: GitHubRepo = await response.json()
        setStars(data.stargazers_count)
        setError(null)
      } catch (err) {
        setError(err instanceof Error ? err : new Error('Unknown error'))
      } finally {
        setLoading(false)
      }
    }

    fetchStars()
  }, [owner, repo])

  return { stars, loading, error }
}
