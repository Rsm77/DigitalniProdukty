# text-wrap - Typography - Tailwind CSS

[](/)v4.1

  1. Typography
  2. text-wrap

Typography

# text-wrap

Utilities for controlling how text wraps within an element.

Class| Styles  
---|---  
`text-wrap`| `text-wrap: wrap;`  
`text-nowrap`| `text-wrap: nowrap;`  
`text-balance`| `text-wrap: balance;`  
`text-pretty`| `text-wrap: pretty;`  
  
## [Examples](#examples)

### [Allowing text to wrap](#allowing-text-to-wrap)

Use the `text-wrap` utility to wrap overflowing text onto multiple lines at logical points in the text:

Beloved Manhattan soup stand closes

New Yorkers are facing the winter chill with less warmth this year as the city's most revered soup stand unexpectedly shutters, following a series of events that have left the community puzzled.
    
    
    <article class="text-wrap">  <h3>Beloved Manhattan soup stand closes</h3>  <p>New Yorkers are facing the winter chill...</p></article>

### [Preventing text from wrapping](#preventing-text-from-wrapping)

Use the `text-nowrap` utility to prevent text from wrapping, allowing it to overflow if necessary:

Beloved Manhattan soup stand closes

New Yorkers are facing the winter chill with less warmth this year as the city's most revered soup stand unexpectedly shutters, following a series of events that have left the community puzzled.
    
    
    <article class="text-nowrap">  <h3>Beloved Manhattan soup stand closes</h3>  <p>New Yorkers are facing the winter chill...</p></article>

### [Balanced text wrapping](#balanced-text-wrapping)

Use the `text-balance` utility to distribute the text evenly across each line:

Beloved Manhattan soup stand closes

New Yorkers are facing the winter chill with less warmth this year as the city's most revered soup stand unexpectedly shutters, following a series of events that have left the community puzzled.
    
    
    <article>  <h3 class="text-balance">Beloved Manhattan soup stand closes</h3>  <p>New Yorkers are facing the winter chill...</p></article>

For performance reasons browsers limit text balancing to blocks that are ~6 lines or less, making it best suited for headings.

### [Pretty text wrapping](#pretty-text-wrapping)

Use the `text-pretty` utility to prefer better text wrapping and layout at the expense of speed. Behavior varies across browsers but often involves approaches like preventing orphans (a single word on its own line) at the end of a text block:

Beloved Manhattan soup stand closes

New Yorkers are facing the winter chill with less warmth this year as the city's most revered soup stand unexpectedly shutters, following a series of events that have left the community puzzled.
    
    
    <article>  <h3 class="text-pretty">Beloved Manhattan soup stand closes</h3>  <p>New Yorkers are facing the winter chill...</p></article>

### [Responsive design](#responsive-design)

Prefix a `text-wrap` utility with a breakpoint variant like `md:` to only apply the utility at medium screen sizes and above:
    
    
    <h1 class="text-pretty md:text-balance ...">  <!-- ... --></h1>

Learn more about using variants in the [variants documentation](/docs/hover-focus-and-other-states).

  * [Examples](#examples)
    * [Allowing text to wrap](#allowing-text-to-wrap)
    * [Preventing text from wrapping](#preventing-text-from-wrapping)
    * [Balanced text wrapping](#balanced-text-wrapping)
    * [Pretty text wrapping](#pretty-text-wrapping)
    * [Responsive design](#responsive-design)
