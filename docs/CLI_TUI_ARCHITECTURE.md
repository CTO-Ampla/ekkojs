# CLI and TUI Architecture for EkkoJS

## Overview

Two distinct modules for terminal interaction:
- **ekko:cli** - Basic CLI operations (colors, input, clear screen, simple menus)
- **ekko:tui** - Full Terminal UI with layouts, controls, and event handling

## CLI Module (ekko:cli)

### Purpose
Provide simple, synchronous terminal operations for basic CLI applications.

### Core Features

#### 1. Output Formatting
```javascript
import cli from 'ekko:cli';

// Text colors
cli.red('Error message');
cli.green('Success!');
cli.yellow('Warning');
cli.blue('Info');
cli.magenta('Special');
cli.cyan('Debug');
cli.white('Normal');
cli.gray('Muted');

// Background colors
cli.bgRed('Error');
cli.bgGreen('Success');
// ... etc

// Text styles
cli.bold('Important');
cli.italic('Emphasis');
cli.underline('Link');
cli.strikethrough('Deprecated');
cli.dim('Secondary');

// Combinations
cli.bold.red('Critical Error!');
cli.bgYellow.black('Highlight');
```

#### 2. Screen Control
```javascript
// Clear operations
cli.clear();           // Clear entire screen
cli.clearLine();       // Clear current line
cli.clearDown();       // Clear from cursor down
cli.clearUp();         // Clear from cursor up

// Cursor control
cli.moveTo(x, y);      // Move cursor to position
cli.moveUp(n);         // Move cursor up n lines
cli.moveDown(n);       // Move cursor down n lines
cli.moveLeft(n);       // Move cursor left n columns
cli.moveRight(n);      // Move cursor right n columns
cli.saveCursor();      // Save cursor position
cli.restoreCursor();   // Restore saved position
cli.hideCursor();      // Hide cursor
cli.showCursor();      // Show cursor

// Screen info
const { width, height } = cli.getTerminalSize();
```

#### 3. Input Operations
```javascript
// Basic input
const name = await cli.input('Enter your name: ');
const password = await cli.password('Password: '); // Hidden input
const confirmed = await cli.confirm('Continue? (y/n) ');

// Choices
const choice = await cli.select({
    message: 'Choose an option:',
    choices: [
        { label: 'Option 1', value: 'opt1' },
        { label: 'Option 2', value: 'opt2' },
        { label: 'Option 3', value: 'opt3' }
    ],
    default: 'opt1'
});

// Multi-select
const selected = await cli.multiSelect({
    message: 'Select features:',
    choices: [
        { label: 'Feature A', value: 'a', checked: true },
        { label: 'Feature B', value: 'b' },
        { label: 'Feature C', value: 'c' }
    ]
});
```

#### 4. Progress & Spinners
```javascript
// Progress bar
const progress = cli.createProgressBar({
    total: 100,
    width: 40,
    format: 'Progress: [{bar}] {percent}% | {current}/{total}'
});

for (let i = 0; i <= 100; i++) {
    progress.update(i);
    await sleep(50);
}
progress.complete();

// Spinner
const spinner = cli.createSpinner('Loading...');
spinner.start();
// ... do work
spinner.succeed('Done!');
// or spinner.fail('Error!');
// or spinner.warn('Warning!');
// or spinner.info('Info');
```

#### 5. Tables & Formatting
```javascript
// Simple table
cli.table([
    ['Name', 'Age', 'City'],
    ['John', '30', 'New York'],
    ['Jane', '25', 'London']
]);

// Box drawing
cli.box('This is in a box');
cli.box('Custom box', { 
    padding: 2, 
    borderStyle: 'double',
    borderColor: 'blue' 
});
```

### Implementation Approach
- Use ANSI escape codes for colors and cursor control
- Leverage .NET's Console class capabilities
- Provide both synchronous and async methods where appropriate
- Chain-able API for color/style combinations

---

## TUI Module (ekko:tui)

### Purpose
Build full-featured terminal applications with layouts, widgets, and event handling.

### Core Concepts

#### 1. Application & Screen
```javascript
import tui from 'ekko:tui';

const app = tui.createApp({
    title: 'My TUI App',
    theme: 'dark', // or 'light', 'custom'
    mouse: true,   // Enable mouse support
    fullscreen: true
});

// Main screen
const screen = app.screen;
```

#### 2. Layout System
```javascript
// Flexible layouts
const layout = tui.createLayout({
    type: 'flex',
    direction: 'vertical', // or 'horizontal'
    children: [
        { size: '30%', minSize: 10, maxSize: 50 },
        { size: '70%', grow: true }
    ]
});

// Grid layout
const grid = tui.createGrid({
    rows: 3,
    cols: 3,
    gap: 1
});

// Absolute positioning
const absolute = tui.createContainer({
    top: 5,
    left: 10,
    width: 40,
    height: 20
});
```

#### 3. Core Widgets

##### Text & Display
```javascript
// Label
const label = tui.createLabel({
    text: 'Hello World',
    align: 'center', // left, center, right
    wrap: true
});

// Text area (multiline, scrollable)
const textArea = tui.createTextArea({
    value: 'Initial text...',
    readonly: false,
    scrollable: true
});

// Log viewer
const log = tui.createLog({
    maxLines: 1000,
    follow: true // Auto-scroll to bottom
});
log.append('New log entry');
```

##### Input Widgets
```javascript
// Text input
const input = tui.createInput({
    label: 'Name:',
    value: '',
    placeholder: 'Enter name...',
    validation: /^[a-zA-Z]+$/
});

// Button
const button = tui.createButton({
    label: 'Submit',
    style: 'primary', // primary, secondary, danger
    onClick: () => console.log('Clicked!')
});

// Checkbox
const checkbox = tui.createCheckbox({
    label: 'Enable feature',
    checked: false,
    onChange: (checked) => console.log('Checked:', checked)
});

// Radio group
const radio = tui.createRadioGroup({
    options: [
        { label: 'Option A', value: 'a' },
        { label: 'Option B', value: 'b' }
    ],
    selected: 'a'
});

// Select/Dropdown
const select = tui.createSelect({
    options: ['Red', 'Green', 'Blue'],
    selected: 'Red',
    onChange: (value) => console.log('Selected:', value)
});
```

##### Lists & Tables
```javascript
// List
const list = tui.createList({
    items: ['Item 1', 'Item 2', 'Item 3'],
    selectable: true,
    multiSelect: false,
    onSelect: (item, index) => console.log('Selected:', item)
});

// Table
const table = tui.createTable({
    headers: ['Name', 'Age', 'City'],
    data: [
        ['John', '30', 'NYC'],
        ['Jane', '25', 'LA']
    ],
    selectable: true,
    sortable: true
});
```

##### Containers & Windows
```javascript
// Panel/Box
const panel = tui.createPanel({
    title: 'Settings',
    border: true,
    shadow: true
});

// Modal dialog
const modal = tui.createModal({
    title: 'Confirm',
    content: 'Are you sure?',
    buttons: ['OK', 'Cancel'],
    onClose: (button) => console.log('Closed with:', button)
});

// Tabs
const tabs = tui.createTabs({
    tabs: [
        { label: 'Tab 1', content: panel1 },
        { label: 'Tab 2', content: panel2 }
    ],
    activeTab: 0
});
```

##### Status & Feedback
```javascript
// Status bar
const statusBar = tui.createStatusBar({
    items: [
        { text: 'Ready', align: 'left' },
        { text: 'Line 1, Col 1', align: 'center' },
        { text: '100%', align: 'right' }
    ]
});

// Progress bar
const progress = tui.createProgress({
    value: 0,
    max: 100,
    showPercent: true
});

// Gauge
const gauge = tui.createGauge({
    value: 75,
    max: 100,
    label: 'CPU Usage'
});
```

#### 4. Event System
```javascript
// Global keyboard events
app.on('keypress', (key) => {
    if (key === 'q' || key === 'ctrl+c') {
        app.exit();
    }
});

// Widget-specific events
input.on('change', (value) => { });
input.on('submit', (value) => { });
button.on('click', () => { });
list.on('select', (item) => { });

// Mouse events (if enabled)
widget.on('mousedown', (x, y) => { });
widget.on('mouseup', (x, y) => { });
widget.on('mousemove', (x, y) => { });
widget.on('wheel', (direction) => { });

// Focus events
widget.on('focus', () => { });
widget.on('blur', () => { });
```

#### 5. Styling & Themes
```javascript
// Widget styling
const styledButton = tui.createButton({
    label: 'Styled',
    style: {
        fg: 'white',
        bg: 'blue',
        border: {
            fg: 'cyan',
            type: 'double' // single, double, round, bold
        },
        hover: {
            fg: 'blue',
            bg: 'white'
        },
        focus: {
            border: { fg: 'yellow' }
        }
    }
});

// Global theme
app.setTheme({
    colors: {
        primary: 'blue',
        secondary: 'gray',
        success: 'green',
        danger: 'red',
        warning: 'yellow',
        info: 'cyan'
    },
    widgets: {
        button: {
            primary: { fg: 'white', bg: 'blue' },
            secondary: { fg: 'black', bg: 'gray' }
        }
    }
});
```

#### 6. Application Lifecycle
```javascript
// Initialize
app.on('ready', () => {
    console.log('App ready');
});

// Run the app
app.run();

// Cleanup
app.on('exit', () => {
    console.log('Cleaning up...');
});
```

### Implementation Approach

#### For CLI Module:
1. Use System.Console for basic operations
2. ANSI escape sequences for colors/formatting
3. Simple state management for spinners/progress
4. Minimal dependencies

#### For TUI Module:
1. Consider wrapping a robust .NET TUI library like:
   - Terminal.Gui
   - Spectre.Console
   - Or build on top of lower-level libraries
2. Virtual DOM-like approach for efficient rendering
3. Event loop integration with EkkoJS
4. Layout engine for responsive designs

### Key Differences

| Feature | CLI | TUI |
|---------|-----|-----|
| Complexity | Simple, procedural | Complex, event-driven |
| Use Case | Scripts, simple tools | Full applications |
| Rendering | Direct console writes | Buffered, optimized |
| Layout | Manual positioning | Automatic layouts |
| Events | Basic input only | Full event system |
| State | Minimal | Full state management |

### Example Usage Comparison

#### CLI Example - Simple Menu
```javascript
import cli from 'ekko:cli';

async function main() {
    cli.clear();
    cli.bold.blue('Welcome to My App\n');
    
    const choice = await cli.select({
        message: 'What would you like to do?',
        choices: [
            { label: 'View Stats', value: 'stats' },
            { label: 'Settings', value: 'settings' },
            { label: 'Exit', value: 'exit' }
        ]
    });
    
    if (choice === 'stats') {
        cli.green('Here are your stats...');
    }
}
```

#### TUI Example - Full App
```javascript
import tui from 'ekko:tui';

const app = tui.createApp({ title: 'My App' });

const layout = tui.createLayout({
    type: 'flex',
    direction: 'vertical'
});

const menu = tui.createList({
    items: ['View Stats', 'Settings', 'Exit'],
    onSelect: (item) => {
        if (item === 'View Stats') {
            showStats();
        }
    }
});

const content = tui.createPanel({
    title: 'Content'
});

layout.add(menu, { size: '30%' });
layout.add(content, { size: '70%' });

app.screen.add(layout);
app.run();
```

## Questions for Consideration

1. **Dependency Strategy**: Should we wrap existing .NET libraries or build from scratch?
2. **Async Model**: How should async operations integrate with the event loop?
3. **Platform Differences**: How to handle Windows vs Unix terminal capabilities?
4. **Performance**: Should TUI use virtual DOM or direct rendering?
5. **Accessibility**: Should we include screen reader support?