# box-decoration-break - Layout - Tailwind CSS

[](/)v4.1

  1. Layout
  2. box-decoration-break

Layout

# box-decoration-break

Utilities for controlling how element fragments should be rendered across multiple lines, columns, or pages.

Class| Styles  
---|---  
`box-decoration-clone`| `box-decoration-break: clone;`  
`box-decoration-slice`| `box-decoration-break: slice;`  
  
## [Examples](#examples)

### [Basic example](#basic-example)

Use the `box-decoration-slice` and `box-decoration-clone` utilities to control whether properties like background, border, border-image, box-shadow, clip-path, margin, and padding should be rendered as if the element were one continuous fragment, or distinct blocks:

box-decoration-slice

Hello  
World

box-decoration-clone

Hello  
World
    
    
    <span class="box-decoration-slice bg-linear-to-r from-indigo-600 to-pink-500 px-2 text-white ...">  Hello<br />World</span><span class="box-decoration-clone bg-linear-to-r from-indigo-600 to-pink-500 px-2 text-white ...">  Hello<br />World</span>

### [Responsive design](#responsive-design)

Prefix a `box-decoration-break` utility with a breakpoint variant like `md:` to only apply the utility at medium screen sizes and above:
    
    
    <div class="box-decoration-clone md:box-decoration-slice ...">  <!-- ... --></div>

Learn more about using variants in the [variants documentation](/docs/hover-focus-and-other-states).

  * [Examples](#examples)
    * [Basic example](#basic-example)
    * [Responsive design](#responsive-design)
