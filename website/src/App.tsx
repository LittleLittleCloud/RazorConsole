import { BrowserRouter, Routes, Route } from 'react-router-dom'
import Layout from '@/components/Layout'
import Home from '@/pages/Home'
import QuickStart from '@/pages/QuickStart'
import Components from '@/pages/Components'
import Advanced from '@/pages/Advanced'

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Layout />}>
          <Route index element={<Home />} />
          <Route path="quick-start" element={<QuickStart />} />
          <Route path="components" element={<Components />} />
          <Route path="advanced" element={<Advanced />} />
        </Route>
      </Routes>
    </BrowserRouter>
  )
}

export default App
