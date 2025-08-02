# TUI Component-Based Architecture

## Core Concepts

### 1. Component System

```javascript
// Base component class (all components inherit from this)
class Component {
    constructor(props = {}) {
        this.props = props;
        this.state = {};
        this.children = [];
        this.parent = null;
        this.isDirty = true;
        this.bounds = { x: 0, y: 0, width: 0, height: 0 };
        this.focused = false;
        this.visible = true;
    }

    // Lifecycle methods
    mount() { }
    unmount() { }
    
    // Update methods
    setState(newState) {
        this.state = { ...this.state, ...newState };
        this.markDirty();
    }
    
    markDirty() {
        this.isDirty = true;
        if (this.parent) {
            this.parent.markDirty();
        }
    }
    
    // Layout methods
    measure(constraints) {
        // Return desired size
        return { width: 0, height: 0 };
    }
    
    layout(bounds) {
        this.bounds = bounds;
        this.layoutChildren();
    }
    
    layoutChildren() {
        // Override in containers
    }
    
    // Rendering
    render(buffer) {
        // Draw to buffer
        if (!this.visible) return;
        
        this.draw(buffer);
        this.renderChildren(buffer);
        this.isDirty = false;
    }
    
    draw(buffer) {
        // Override in concrete components
    }
    
    renderChildren(buffer) {
        for (const child of this.children) {
            if (child.isDirty || this.isDirty) {
                child.render(buffer);
            }
        }
    }
    
    // Child management
    addChild(child) {
        this.children.push(child);
        child.parent = this;
        this.markDirty();
    }
    
    removeChild(child) {
        const index = this.children.indexOf(child);
        if (index !== -1) {
            this.children.splice(index, 1);
            child.parent = null;
            this.markDirty();
        }
    }
    
    // Event handling
    handleKeyPress(key) {
        // Override in interactive components
        return false; // not handled
    }
    
    handleMouseEvent(event) {
        // Override in interactive components
        return false; // not handled
    }
    
    // Focus management
    canFocus() {
        return false; // Override in focusable components
    }
    
    focus() {
        if (this.canFocus()) {
            this.focused = true;
            this.markDirty();
        }
    }
    
    blur() {
        this.focused = false;
        this.markDirty();
    }
}
```

### 2. Screen Buffer System

```javascript
class ScreenBuffer {
    constructor(width, height) {
        this.width = width;
        this.height = height;
        this.cells = [];
        this.dirtyRegions = [];
        
        // Initialize buffer
        for (let y = 0; y < height; y++) {
            this.cells[y] = [];
            for (let x = 0; x < width; x++) {
                this.cells[y][x] = {
                    char: ' ',
                    fg: 'default',
                    bg: 'default',
                    attrs: 0 // bold, italic, underline, etc.
                };
            }
        }
    }
    
    // Drawing operations
    setCell(x, y, char, fg = 'default', bg = 'default', attrs = 0) {
        if (x >= 0 && x < this.width && y >= 0 && y < this.height) {
            const cell = this.cells[y][x];
            if (cell.char !== char || cell.fg !== fg || cell.bg !== bg || cell.attrs !== attrs) {
                cell.char = char;
                cell.fg = fg;
                cell.bg = bg;
                cell.attrs = attrs;
                this.markDirty(x, y, 1, 1);
            }
        }
    }
    
    drawText(x, y, text, fg, bg, attrs) {
        for (let i = 0; i < text.length; i++) {
            this.setCell(x + i, y, text[i], fg, bg, attrs);
        }
    }
    
    fillRect(x, y, width, height, char = ' ', fg = 'default', bg = 'default') {
        for (let dy = 0; dy < height; dy++) {
            for (let dx = 0; dx < width; dx++) {
                this.setCell(x + dx, y + dy, char, fg, bg);
            }
        }
    }
    
    drawBox(x, y, width, height, style = 'single', fg = 'default', bg = 'default') {
        const boxChars = {
            single: '┌─┐│└┘',
            double: '╔═╗║╚╝',
            round: '╭─╮│╰╯',
            bold: '┏━┓┃┗┛'
        };
        
        const chars = boxChars[style] || boxChars.single;
        
        // Top line
        this.setCell(x, y, chars[0], fg, bg);
        for (let i = 1; i < width - 1; i++) {
            this.setCell(x + i, y, chars[1], fg, bg);
        }
        this.setCell(x + width - 1, y, chars[2], fg, bg);
        
        // Sides
        for (let i = 1; i < height - 1; i++) {
            this.setCell(x, y + i, chars[3], fg, bg);
            this.setCell(x + width - 1, y + i, chars[3], fg, bg);
        }
        
        // Bottom line
        this.setCell(x, y + height - 1, chars[4], fg, bg);
        for (let i = 1; i < width - 1; i++) {
            this.setCell(x + i, y + height - 1, chars[1], fg, bg);
        }
        this.setCell(x + width - 1, y + height - 1, chars[5], fg, bg);
    }
    
    // Dirty region tracking
    markDirty(x, y, width, height) {
        this.dirtyRegions.push({ x, y, width, height });
    }
    
    getDirtyRegions() {
        // Merge overlapping regions for efficiency
        return this.optimizeDirtyRegions(this.dirtyRegions);
    }
    
    clearDirtyRegions() {
        this.dirtyRegions = [];
    }
    
    optimizeDirtyRegions(regions) {
        // TODO: Implement region merging algorithm
        return regions;
    }
}
```

### 3. Renderer

```javascript
class Renderer {
    constructor() {
        this.primaryBuffer = null;
        this.secondaryBuffer = null;
        this.currentBuffer = 'primary';
    }
    
    initialize(width, height) {
        this.primaryBuffer = new ScreenBuffer(width, height);
        this.secondaryBuffer = new ScreenBuffer(width, height);
    }
    
    render(rootComponent) {
        const buffer = this.currentBuffer === 'primary' 
            ? this.primaryBuffer 
            : this.secondaryBuffer;
        
        // Clear dirty regions
        const dirtyRegions = buffer.getDirtyRegions();
        for (const region of dirtyRegions) {
            buffer.fillRect(
                region.x, 
                region.y, 
                region.width, 
                region.height
            );
        }
        
        // Render component tree
        rootComponent.render(buffer);
        
        // Get only changed regions
        const changes = this.diff(
            this.primaryBuffer, 
            this.secondaryBuffer
        );
        
        // Apply changes to terminal
        this.applyChanges(changes);
        
        // Swap buffers
        this.swapBuffers();
        buffer.clearDirtyRegions();
    }
    
    diff(buffer1, buffer2) {
        const changes = [];
        const regions = buffer2.getDirtyRegions();
        
        for (const region of regions) {
            for (let y = region.y; y < region.y + region.height; y++) {
                for (let x = region.x; x < region.x + region.width; x++) {
                    const cell1 = buffer1.cells[y][x];
                    const cell2 = buffer2.cells[y][x];
                    
                    if (cell1.char !== cell2.char ||
                        cell1.fg !== cell2.fg ||
                        cell1.bg !== cell2.bg ||
                        cell1.attrs !== cell2.attrs) {
                        changes.push({ x, y, cell: cell2 });
                    }
                }
            }
        }
        
        return changes;
    }
    
    applyChanges(changes) {
        // Group changes by row for efficiency
        const byRow = {};
        for (const change of changes) {
            if (!byRow[change.y]) {
                byRow[change.y] = [];
            }
            byRow[change.y].push(change);
        }
        
        // Apply changes
        for (const [y, rowChanges] of Object.entries(byRow)) {
            // Sort by x coordinate
            rowChanges.sort((a, b) => a.x - b.x);
            
            // Move cursor and apply changes
            // This would call the actual terminal output methods
            this.moveCursor(rowChanges[0].x, parseInt(y));
            
            for (const change of rowChanges) {
                this.outputCell(change.cell);
            }
        }
    }
    
    swapBuffers() {
        this.currentBuffer = this.currentBuffer === 'primary' 
            ? 'secondary' 
            : 'primary';
    }
}
```

### 4. Layout System

```javascript
// Layout components
class FlexLayout extends Component {
    constructor(props) {
        super(props);
        this.direction = props.direction || 'horizontal'; // or 'vertical'
        this.gap = props.gap || 0;
        this.padding = props.padding || 0;
    }
    
    layoutChildren() {
        const { x, y, width, height } = this.bounds;
        const innerBounds = {
            x: x + this.padding,
            y: y + this.padding,
            width: width - 2 * this.padding,
            height: height - 2 * this.padding
        };
        
        // Calculate flex sizes
        let totalFlex = 0;
        let fixedSize = 0;
        
        for (const child of this.children) {
            if (child.props.flex) {
                totalFlex += child.props.flex;
            } else if (child.props.size) {
                fixedSize += child.props.size;
            }
        }
        
        const totalGap = this.gap * (this.children.length - 1);
        const availableSize = (this.direction === 'horizontal' 
            ? innerBounds.width 
            : innerBounds.height) - fixedSize - totalGap;
        
        const flexUnit = totalFlex > 0 ? availableSize / totalFlex : 0;
        
        // Layout children
        let offset = 0;
        for (const child of this.children) {
            let childSize;
            if (child.props.flex) {
                childSize = flexUnit * child.props.flex;
            } else if (child.props.size) {
                childSize = child.props.size;
            } else {
                childSize = 0;
            }
            
            const childBounds = this.direction === 'horizontal'
                ? {
                    x: innerBounds.x + offset,
                    y: innerBounds.y,
                    width: childSize,
                    height: innerBounds.height
                }
                : {
                    x: innerBounds.x,
                    y: innerBounds.y + offset,
                    width: innerBounds.width,
                    height: childSize
                };
            
            child.layout(childBounds);
            offset += childSize + this.gap;
        }
    }
}

class GridLayout extends Component {
    constructor(props) {
        super(props);
        this.rows = props.rows || 1;
        this.cols = props.cols || 1;
        this.gap = props.gap || 0;
    }
    
    layoutChildren() {
        const { x, y, width, height } = this.bounds;
        
        const cellWidth = (width - (this.cols - 1) * this.gap) / this.cols;
        const cellHeight = (height - (this.rows - 1) * this.gap) / this.rows;
        
        let index = 0;
        for (let row = 0; row < this.rows && index < this.children.length; row++) {
            for (let col = 0; col < this.cols && index < this.children.length; col++) {
                const child = this.children[index++];
                child.layout({
                    x: x + col * (cellWidth + this.gap),
                    y: y + row * (cellHeight + this.gap),
                    width: cellWidth,
                    height: cellHeight
                });
            }
        }
    }
}
```

### 5. Focus Management

```javascript
class FocusManager {
    constructor() {
        this.focusableComponents = [];
        this.currentFocusIndex = -1;
    }
    
    register(component) {
        if (component.canFocus()) {
            this.focusableComponents.push(component);
        }
    }
    
    unregister(component) {
        const index = this.focusableComponents.indexOf(component);
        if (index !== -1) {
            this.focusableComponents.splice(index, 1);
            if (this.currentFocusIndex >= index) {
                this.currentFocusIndex--;
            }
        }
    }
    
    focusNext() {
        if (this.focusableComponents.length === 0) return;
        
        // Blur current
        if (this.currentFocusIndex >= 0) {
            this.focusableComponents[this.currentFocusIndex].blur();
        }
        
        // Focus next
        this.currentFocusIndex = 
            (this.currentFocusIndex + 1) % this.focusableComponents.length;
        this.focusableComponents[this.currentFocusIndex].focus();
    }
    
    focusPrevious() {
        if (this.focusableComponents.length === 0) return;
        
        // Blur current
        if (this.currentFocusIndex >= 0) {
            this.focusableComponents[this.currentFocusIndex].blur();
        }
        
        // Focus previous
        this.currentFocusIndex = this.currentFocusIndex - 1;
        if (this.currentFocusIndex < 0) {
            this.currentFocusIndex = this.focusableComponents.length - 1;
        }
        this.focusableComponents[this.currentFocusIndex].focus();
    }
    
    getCurrentFocus() {
        if (this.currentFocusIndex >= 0) {
            return this.focusableComponents[this.currentFocusIndex];
        }
        return null;
    }
}
```

### 6. Event System

```javascript
class EventDispatcher {
    constructor() {
        this.keyHandlers = [];
        this.mouseHandlers = [];
    }
    
    dispatchKeyEvent(key) {
        // First try focused component
        const focused = this.focusManager.getCurrentFocus();
        if (focused && focused.handleKeyPress(key)) {
            return;
        }
        
        // Then bubble up from focused component
        let component = focused?.parent;
        while (component) {
            if (component.handleKeyPress(key)) {
                return;
            }
            component = component.parent;
        }
        
        // Finally, global handlers
        for (const handler of this.keyHandlers) {
            if (handler(key)) {
                return;
            }
        }
    }
    
    dispatchMouseEvent(event) {
        // Find component at coordinates
        const component = this.findComponentAt(event.x, event.y);
        if (component && component.handleMouseEvent(event)) {
            return;
        }
        
        // Bubble up
        let parent = component?.parent;
        while (parent) {
            if (parent.handleMouseEvent(event)) {
                return;
            }
            parent = parent.parent;
        }
    }
    
    findComponentAt(x, y, root) {
        // Depth-first search for deepest component at position
        for (let i = root.children.length - 1; i >= 0; i--) {
            const child = root.children[i];
            if (child.visible && 
                x >= child.bounds.x && 
                x < child.bounds.x + child.bounds.width &&
                y >= child.bounds.y && 
                y < child.bounds.y + child.bounds.height) {
                
                const found = this.findComponentAt(x, y, child);
                return found || child;
            }
        }
        return null;
    }
}
```

### 7. Example Components

```javascript
// Button component
class Button extends Component {
    constructor(props) {
        super(props);
        this.label = props.label || 'Button';
        this.onClick = props.onClick;
    }
    
    canFocus() {
        return true;
    }
    
    draw(buffer) {
        const { x, y, width, height } = this.bounds;
        const fg = this.focused ? 'black' : 'white';
        const bg = this.focused ? 'white' : 'blue';
        
        // Draw button background
        buffer.fillRect(x, y, width, height, ' ', fg, bg);
        
        // Draw label centered
        const labelX = x + Math.floor((width - this.label.length) / 2);
        const labelY = y + Math.floor(height / 2);
        buffer.drawText(labelX, labelY, this.label, fg, bg);
        
        // Draw border if focused
        if (this.focused) {
            buffer.drawBox(x, y, width, height, 'single', 'yellow');
        }
    }
    
    handleKeyPress(key) {
        if (key === 'enter' || key === ' ') {
            this.onClick?.();
            return true;
        }
        return false;
    }
    
    handleMouseEvent(event) {
        if (event.type === 'click') {
            this.onClick?.();
            return true;
        }
        return false;
    }
}

// Input component
class TextInput extends Component {
    constructor(props) {
        super(props);
        this.value = props.value || '';
        this.placeholder = props.placeholder || '';
        this.cursorPos = this.value.length;
        this.scrollOffset = 0;
    }
    
    canFocus() {
        return true;
    }
    
    draw(buffer) {
        const { x, y, width, height } = this.bounds;
        const fg = this.focused ? 'white' : 'gray';
        const bg = this.focused ? 'darkblue' : 'black';
        
        // Draw background
        buffer.fillRect(x, y, width, height, ' ', fg, bg);
        
        // Draw text or placeholder
        const text = this.value || this.placeholder;
        const textFg = this.value ? fg : 'darkgray';
        
        // Handle scrolling for long text
        const visibleText = text.substring(
            this.scrollOffset, 
            this.scrollOffset + width - 2
        );
        buffer.drawText(x + 1, y, visibleText, textFg, bg);
        
        // Draw cursor if focused
        if (this.focused) {
            const cursorX = x + 1 + this.cursorPos - this.scrollOffset;
            if (cursorX >= x + 1 && cursorX < x + width - 1) {
                buffer.setCell(cursorX, y, '│', 'white', bg);
            }
        }
        
        // Draw border
        buffer.drawBox(x, y, width, height, 'single', fg, bg);
    }
    
    handleKeyPress(key) {
        if (key.length === 1) {
            // Insert character
            this.value = 
                this.value.slice(0, this.cursorPos) + 
                key + 
                this.value.slice(this.cursorPos);
            this.cursorPos++;
            this.markDirty();
            return true;
        }
        
        switch (key) {
            case 'backspace':
                if (this.cursorPos > 0) {
                    this.value = 
                        this.value.slice(0, this.cursorPos - 1) + 
                        this.value.slice(this.cursorPos);
                    this.cursorPos--;
                    this.markDirty();
                }
                return true;
                
            case 'delete':
                if (this.cursorPos < this.value.length) {
                    this.value = 
                        this.value.slice(0, this.cursorPos) + 
                        this.value.slice(this.cursorPos + 1);
                    this.markDirty();
                }
                return true;
                
            case 'left':
                if (this.cursorPos > 0) {
                    this.cursorPos--;
                    this.markDirty();
                }
                return true;
                
            case 'right':
                if (this.cursorPos < this.value.length) {
                    this.cursorPos++;
                    this.markDirty();
                }
                return true;
                
            case 'home':
                this.cursorPos = 0;
                this.markDirty();
                return true;
                
            case 'end':
                this.cursorPos = this.value.length;
                this.markDirty();
                return true;
        }
        
        return false;
    }
}
```

### 8. Application Structure

```javascript
class TUIApplication {
    constructor(options = {}) {
        this.title = options.title || 'TUI App';
        this.width = options.width || process.stdout.columns;
        this.height = options.height || process.stdout.rows;
        
        this.rootComponent = new Component();
        this.renderer = new Renderer();
        this.focusManager = new FocusManager();
        this.eventDispatcher = new EventDispatcher();
        
        this.running = false;
        this.frameRate = options.frameRate || 60;
        this.lastRenderTime = 0;
    }
    
    mount(component) {
        this.rootComponent.addChild(component);
        component.mount();
    }
    
    run() {
        this.running = true;
        
        // Initialize
        this.renderer.initialize(this.width, this.height);
        this.setupEventHandlers();
        
        // Initial layout
        this.rootComponent.layout({
            x: 0,
            y: 0,
            width: this.width,
            height: this.height
        });
        
        // Start render loop
        this.renderLoop();
    }
    
    renderLoop() {
        if (!this.running) return;
        
        const now = Date.now();
        const deltaTime = now - this.lastRenderTime;
        
        if (deltaTime >= 1000 / this.frameRate) {
            this.renderer.render(this.rootComponent);
            this.lastRenderTime = now;
        }
        
        // Schedule next frame
        setImmediate(() => this.renderLoop());
    }
    
    stop() {
        this.running = false;
        this.cleanup();
    }
}
```

## Key Benefits of This Architecture

1. **Efficient Rendering**: Only redraws changed regions
2. **Component Reusability**: Build complex UIs from simple components
3. **Proper State Management**: Components manage their own state
4. **Layout Flexibility**: Multiple layout strategies (flex, grid, absolute)
5. **Event Handling**: Proper event bubbling and focus management
6. **Performance**: Double buffering prevents flicker
7. **Extensibility**: Easy to add new components and layouts

## Implementation Priorities

1. Core component system and lifecycle
2. Basic rendering with dirty region tracking
3. Flex layout system
4. Focus management
5. Basic components (Button, TextInput, Label)
6. Event handling system
7. More complex components (List, Table, etc.)
8. Advanced layouts (Grid, Absolute)
9. Styling and theming
10. Animation support