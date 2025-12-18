# text-decoration-line - Typography - Tailwind CSS

[](/)v4.1

  1. Typography
  2. text-decoration-line

Typography

# text-decoration-line

Utilities for controlling the decoration of text.

Class| Styles  
---|---  
`underline`| `text-decoration-line: underline;`  
`overline`| `text-decoration-line: overline;`  
`line-through`| `text-decoration-line: line-through;`  
`no-underline`| `text-decoration-line: none;`  
  
## [Examples](#examples)

### [Underling text](#underling-text)

Use the `underline` utility to add an underline to the text of an element:

The quick brown fox jumps over the lazy dog.
    
    
    <p class="underline">The quick brown fox...</p>

### [Adding an overline to text](#adding-an-overline-to-text)

Use the `overline` utility to add an overline to the text of an element:

The quick brown fox jumps over the lazy dog.
    
    
    <p class="overline">The quick brown fox...</p>

### [Adding a line through text](#adding-a-line-through-text)

Use the `line-through` utility to add a line through the text of an element:

The quick brown fox jumps over the lazy dog.
    
    
    <p class="line-through">The quick brown fox...</p>

### [Removing a line from text](#removing-a-line-from-text)

Use the `no-underline` utility to remove a line from the text of an element:

The quick brown fox jumps over the lazy dog.
    
    
    <p class="no-underline">The quick brown fox...</p>

### [Applying on hover](#applying-on-hover)

Prefix a `text-decoration-line` utility with a variant like `hover:*` to only apply the utility in that state:

Hover over the text to see the expected behavior

The [quick brown fox](https://en.wikipedia.org/wiki/The_quick_brown_fox_jumps_over_the_lazy_dog) jumps over the lazy dog.
    
    
    <p>The <a href="..." class="no-underline hover:underline ...">quick brown fox</a> jumps over the lazy dog.</p>

Learn more about using variants in the [variants documentation](/docs/hover-focus-and-other-states).

### [Responsive design](#responsive-design)

Prefix a `text-decoration-line` utility with a breakpoint variant like `md:` to only apply the utility at medium screen sizes and above:
    
    
    <a class="no-underline md:underline ..." href="...">  <!-- ... --></a>

Learn more about using variants in the [variants documentation](/docs/hover-focus-and-other-states).

  * [Examples](#examples)
    * [Underling text](#underling-text)
    * [Adding an overline to text](#adding-an-overline-to-text)
    * [Adding a line through text](#adding-a-line-through-text)
    * [Removing a line from text](#removing-a-line-from-text)
    * [Applying on hover](#applying-on-hover)
    * [Responsive design](#responsive-design)
