# RazorConsole Documentation Website

This is the documentation website for RazorConsole, built with Vite, React, TypeScript, TailwindCSS, and shadcn/ui components.

## Features

- **Home Page**: Overview of RazorConsole with quick start guide and feature highlights
- **Quick Start**: Step-by-step guide to get started with RazorConsole
- **Components Page**: Comprehensive documentation of all 15+ built-in components with examples and parameter references
- **Advanced Topics**: Deep dive into Hot Reload, Custom Translators, Keyboard Events, and Focus Management

## Development

### Prerequisites

- Node.js 20.x or later
- npm 10.x or later

### Installation

```bash
cd website
npm install
```

### Running the Development Server

```bash
npm run dev
```

The website will be available at `http://localhost:5173`

### Building for Production

```bash
npm run build
```

The production build will be output to the `dist/` directory.

### Preview Production Build

```bash
npm run preview
```

## Technology Stack

- **Vite**: Fast build tool and dev server
- **React**: UI framework
- **TypeScript**: Type-safe JavaScript
- **TailwindCSS**: Utility-first CSS framework
- **shadcn/ui**: High-quality React components built with Radix UI
- **React Router**: Client-side routing
- **Lucide React**: Beautiful icon library

## Project Structure

```
website/
├── src/
│   ├── components/
│   │   ├── ui/          # shadcn/ui components
│   │   └── Layout.tsx   # Main layout with navigation
│   ├── pages/
│   │   ├── Home.tsx         # Homepage
│   │   ├── QuickStart.tsx   # Quick start guide
│   │   ├── Components.tsx   # Component documentation
│   │   └── Advanced.tsx     # Advanced topics
│   ├── data/
│   │   └── components.ts    # Component metadata
│   ├── lib/
│   │   └── utils.ts         # Utility functions
│   ├── App.tsx              # Main app with routing
│   ├── main.tsx             # Entry point
│   └── index.css            # Global styles
├── public/                  # Static assets
├── index.html               # HTML template
├── vite.config.ts           # Vite configuration
├── tailwind.config.js       # Tailwind configuration
├── postcss.config.js        # PostCSS configuration
└── tsconfig.json            # TypeScript configuration
```

## Customization

### Adding New Components

To add documentation for a new component:

1. Add the component info to `src/data/components.ts`
2. Include the component name, description, category, parameters, and example code
3. The component will automatically appear on the Components page

### Updating Styles

- Global styles: Edit `src/index.css`
- Tailwind configuration: Edit `tailwind.config.js`
- Component styles: Use Tailwind utility classes in components

## License

MIT License - see the LICENSE file in the root of the repository.
