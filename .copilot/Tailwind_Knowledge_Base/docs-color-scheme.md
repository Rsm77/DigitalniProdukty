# color-scheme - Interactivity - Tailwind CSS

[](/)v4.1

  1. Interactivity
  2. color-scheme

Interactivity

# color-scheme

Utilities for controlling the color scheme of an element.

Class| Styles  
---|---  
`scheme-normal`| `color-scheme: normal;`  
`scheme-dark`| `color-scheme: dark;`  
`scheme-light`| `color-scheme: light;`  
`scheme-light-dark`| `color-scheme: light dark;`  
`scheme-only-dark`| `color-scheme: only dark;`  
`scheme-only-light`| `color-scheme: only light;`  
  
## [Examples](#examples)

### [Basic example](#basic-example)

Use utilities like `scheme-light` and `scheme-light-dark` to control how element should be rendered:

Try switching your system color scheme to see the difference

scheme-light

scheme-dark

scheme-light-dark
    
    
    <div class="scheme-light ...">  <input type="date" /></div><div class="scheme-dark ...">  <input type="date" /></div><div class="scheme-light-dark ...">  <input type="date" /></div>

### [Applying in dark mode](#applying-in-dark-mode)

Prefix a `color-scheme` utility with a variant like `dark:*` to only apply the utility in that state:
    
    
    <html class="scheme-light dark:scheme-dark ...">  <!-- ... --></html>

Learn more about using variants in the [variants documentation](/docs/hover-focus-and-other-states).

  * [Examples](#examples)
    * [Basic example](#basic-example)
    * [Applying in dark mode](#applying-in-dark-mode)
