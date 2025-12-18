# isolation - Layout - Tailwind CSS

[](/)v4.1

  1. Layout
  2. isolation

Layout

# isolation

Utilities for controlling whether an element should explicitly create a new stacking context.

Class| Styles  
---|---  
`isolate`| `isolation: isolate;`  
`isolation-auto`| `isolation: auto;`  
  
## [Examples](#examples)

### [Basic example](#basic-example)

Use the `isolate` and `isolation-auto` utilities to control whether an element should explicitly create a new stacking context:
    
    
    <div class="isolate ...">  <!-- ... --></div>

### [Responsive design](#responsive-design)

Prefix an `isolation` utility with a breakpoint variant like `md:` to only apply the utility at medium screen sizes and above:
    
    
    <div class="isolate md:isolation-auto ...">  <!-- ... --></div>

Learn more about using variants in the [variants documentation](/docs/hover-focus-and-other-states).

  * [Examples](#examples)
    * [Basic example](#basic-example)
    * [Responsive design](#responsive-design)
