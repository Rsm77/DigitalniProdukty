# font-smoothing - Typography - Tailwind CSS

[](/)v4.1

  1. Typography
  2. font-smoothing

Typography

# font-smoothing

Utilities for controlling the font smoothing of an element.

Class| Styles  
---|---  
`antialiased`| `-webkit-font-smoothing: antialiased; -moz-osx-font-smoothing: grayscale;`  
`subpixel-antialiased`| `-webkit-font-smoothing: auto; -moz-osx-font-smoothing: auto;`  
  
## [Examples](#examples)

### [Grayscale antialiasing](#grayscale-antialiasing)

Use the `antialiased` utility to render text using grayscale antialiasing:

The quick brown fox jumps over the lazy dog.
    
    
    <p class="antialiased ...">The quick brown fox ...</p>

### [Subpixel antialiasing](#subpixel-antialiasing)

Use the `subpixel-antialiased` utility to render text using subpixel antialiasing:

The quick brown fox jumps over the lazy dog.
    
    
    <p class="subpixel-antialiased ...">The quick brown fox ...</p>

### [Responsive design](#responsive-design)

Prefix `-webkit-font-smoothing` and `-moz-osx-font-smoothing` utilities with a breakpoint variant like `md:` to only apply the utility at medium screen sizes and above:
    
    
    <p class="antialiased md:subpixel-antialiased ...">  Lorem ipsum dolor sit amet...</p>

Learn more about using variants in the [variants documentation](/docs/hover-focus-and-other-states).

  * [Examples](#examples)
    * [Grayscale antialiasing](#grayscale-antialiasing)
    * [Subpixel antialiasing](#subpixel-antialiasing)
    * [Responsive design](#responsive-design)
