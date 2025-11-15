# Todo List Manager Example

A simple yet comprehensive task management application demonstrating RazorConsole's interactive components and data binding capabilities.

## Features

- **Add Tasks**: Create new tasks with descriptive text using TextInput
- **Filter Tasks**: View All, Active, or Completed tasks using Select dropdown
- **Task Management**: Complete, reopen, or delete tasks with TextButton controls
- **Live Statistics**: Real-time count of total, active, and completed tasks
- **Dynamic Table**: Tasks displayed in a responsive table with status indicators
- **Two-way Data Binding**: Automatic UI updates when data changes

## Components Used

- **TextInput**: For entering new task descriptions
- **Select**: For filtering tasks by status (All/Active/Completed)
- **TextButton**: For interactive actions (Add, Complete, Delete, etc.)
- **Table** (SpectreTable): For displaying tasks in a structured format
- **Panel**: For organizing UI sections
- **Markup**: For styled text and status indicators

## Running the Example

```bash
cd examples/TodoList
dotnet run
```

## Key Concepts Demonstrated

1. **Two-way Data Binding**: The `@bind-Value` directive on TextInput and Select components enables automatic synchronization between UI and data
2. **Collection Rendering**: The `@foreach` loop dynamically renders table rows based on filtered task list
3. **Dynamic UI Updates**: Changes to task status or filters immediately update the displayed content
4. **Event Handling**: Button click events trigger methods that modify the task collection
5. **Computed Properties**: The `GetFilteredTasks()` method demonstrates reactive filtering based on current selection

## Controls

- **Tab**: Navigate between interactive elements
- **Enter**: Activate buttons or select options
- **Type**: Enter text in the TextInput field
- **Ctrl+C**: Exit the application
