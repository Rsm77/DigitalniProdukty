# resize - Interactivity - Tailwind CSS

[](/)v4.1

  1. Interactivity
  2. resize

Interactivity

# resize

Utilities for controlling how an element can be resized.

Class| Styles  
---|---  
`resize-none`| `resize: none;`  
`resize`| `resize: both;`  
`resize-y`| `resize: vertical;`  
`resize-x`| `resize: horizontal;`  
  
## [Examples](#examples)

### [Resizing in all directions](#resizing-in-all-directions)

Use `resize` to make an element horizontally and vertically resizable:

Drag the textarea handle in the demo to see the expected behavior
    
    
    <textarea class="resize rounded-md ..."></textarea>

### [Resizing vertically](#resizing-vertically)

Use `resize-y` to make an element vertically resizable:

Drag the textarea handle in the demo to see the expected behavior
    
    
    <textarea class="resize-y rounded-md ..."></textarea>

### [Resizing horizontally](#resizing-horizontally)

Use `resize-x` to make an element horizontally resizable:

Drag the textarea handle in the demo to see the expected behavior
    
    
    <textarea class="resize-x rounded-md ..."></textarea>

### [Prevent resizing](#prevent-resizing)

Use `resize-none` to prevent an element from being resizable:

Notice that the textarea handle is gone
    
    
    <textarea class="resize-none rounded-md"></textarea>

### [Responsive design](#responsive-design)

Prefix a `resize` utility with a breakpoint variant like `md:` to only apply the utility at medium screen sizes and above:
    
    
    <div class="resize-none md:resize ...">  <!-- ... --></div>

Learn more about using variants in the [variants documentation](/docs/hover-focus-and-other-states).

  * [Examples](#examples)
    * [Resizing in all directions](#resizing-in-all-directions)
    * [Resizing vertically](#resizing-vertically)
    * [Resizing horizontally](#resizing-horizontally)
    * [Prevent resizing](#prevent-resizing)
    * [Responsive design](#responsive-design)
