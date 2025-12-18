# background-clip - Backgrounds - Tailwind CSS

[](/)v4.1

  1. Backgrounds
  2. background-clip

Backgrounds

# background-clip

Utilities for controlling the bounding box of an element's background.

Class| Styles  
---|---  
`bg-clip-border`| `background-clip: border-box;`  
`bg-clip-padding`| `background-clip: padding-box;`  
`bg-clip-content`| `background-clip: content-box;`  
`bg-clip-text`| `background-clip: text;`  
  
## [Examples](#examples)

### [Basic example](#basic-example)

Use the `bg-clip-border`, `bg-clip-padding`, and `bg-clip-content` utilities to control the bounding box of an element's background:

bg-clip-border

bg-clip-padding

bg-clip-content
    
    
    <div class="border-4 bg-indigo-500 bg-clip-border p-3"></div><div class="border-4 bg-indigo-500 bg-clip-padding p-3"></div><div class="border-4 bg-indigo-500 bg-clip-content p-3"></div>

### [Cropping to text](#cropping-to-text)

Use the `bg-clip-text` utility to crop an element's background to match the shape of the text:

Hello world
    
    
    <p class="bg-linear-to-r from-pink-500 to-violet-500 bg-clip-text text-5xl font-extrabold text-transparent ...">  Hello world</p>

### [Responsive design](#responsive-design)

Prefix a `background-clip` utility with a breakpoint variant like `md:` to only apply the utility at medium screen sizes and above:
    
    
    <div class="bg-clip-border md:bg-clip-padding ...">  <!-- ... --></div>

Learn more about using variants in the [variants documentation](/docs/hover-focus-and-other-states).

  * [Examples](#examples)
    * [Basic example](#basic-example)
    * [Cropping to text](#cropping-to-text)
    * [Responsive design](#responsive-design)
