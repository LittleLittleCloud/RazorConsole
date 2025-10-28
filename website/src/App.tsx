import { BrowserRouter, Routes, Route } from "react-router-dom"
import Layout from "@/components/Layout"
import Home from "@/pages/Home"
import Docs from "@/pages/Docs"
import QuickStart from "@/pages/QuickStart"
import Components from "@/pages/Components"
import Advanced from "@/pages/Advanced"
import { useThemeEffect } from "@/components/ThemeProvider"

function App() {
  useThemeEffect()
  
  return (
    <BrowserRouter basename={import.meta.env.BASE_URL}>
      <Routes>
        <Route path="/" element={<Layout />}>
          <Route index element={<Home />} />
          <Route path="docs" element={<Docs />} />
          <Route path="quick-start" element={<QuickStart />} />
          <Route path="components" element={<Components />} />
          <Route path="advanced" element={<Advanced />} />
        </Route>
      </Routes>
    </BrowserRouter>
  )
}

export default App
