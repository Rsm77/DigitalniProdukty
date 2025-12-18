# white-space - Typography - Tailwind CSS

[](/)v4.1

  1. Typography
  2. white-space

Typography

# white-space

Utilities for controlling an element's white-space property.

Class| Styles  
---|---  
`whitespace-normal`| `white-space: normal;`  
`whitespace-nowrap`| `white-space: nowrap;`  
`whitespace-pre`| `white-space: pre;`  
`whitespace-pre-line`| `white-space: pre-line;`  
`whitespace-pre-wrap`| `white-space: pre-wrap;`  
`whitespace-break-spaces`| `white-space: break-spaces;`  
  
## [Examples](#examples)

### [Normal](#normal)

Use the `whitespace-normal` utility to cause text to wrap normally within an element. Newlines and spaces will be collapsed:

Hey everyone! It’s almost 2022 and we still don’t know if there are aliens living among us, or do we? Maybe the person writing this is an alien. You will never know.
    
    
    <p class="whitespace-normal">Hey everyone!It's almost 2022       and we still don't know if there             are aliens living among us, or do we? Maybe the person writing this is an alien.You will never know.</p>

### [No Wrap](#no-wrap)

Use the `whitespace-nowrap` utility to prevent text from wrapping within an element. Newlines and spaces will be collapsed:

Hey everyone! It’s almost 2022 and we still don’t know if there are aliens living among us, or do we? Maybe the person writing this is an alien. You will never know.
    
    
    <p class="overflow-auto whitespace-nowrap">Hey everyone!It's almost 2022       and we still don't know if there             are aliens living among us, or do we? Maybe the person writing this is an alien.You will never know.</p>

### [Pre](#pre)

Use the `whitespace-pre` utility to preserve newlines and spaces within an element. Text will not be wrapped:

Hey everyone! It’s almost 2022 and we still don’t know if there are aliens living among us, or do we? Maybe the person writing this is an alien. You will never know.
    
    
    <p class="overflow-auto whitespace-pre">Hey everyone!It's almost 2022       and we still don't know if there             are aliens living among us, or do we? Maybe the person writing this is an alien.You will never know.</p>

### [Pre Line](#pre-line)

Use the `whitespace-pre-line` utility to preserve newlines but not spaces within an element. Text will be wrapped normally:

Hey everyone! It’s almost 2022 and we still don’t know if there are aliens living among us, or do we? Maybe the person writing this is an alien. You will never know.
    
    
    <p class="whitespace-pre-line">Hey everyone!It's almost 2022       and we still don't know if there             are aliens living among us, or do we? Maybe the person writing this is an alien.You will never know.</p>

### [Pre Wrap](#pre-wrap)

Use the `whitespace-pre-wrap` utility to preserve newlines and spaces within an element. Text will be wrapped normally:

Hey everyone! It’s almost 2022 and we still don’t know if there are aliens living among us, or do we? Maybe the person writing this is an alien. You will never know.
    
    
    <p class="whitespace-pre-wrap">Hey everyone!It's almost 2022       and we still don't know if there             are aliens living among us, or do we? Maybe the person writing this is an alien.You will never know.</p>

### [Break Spaces](#break-spaces)

Use the `whitespace-break-spaces` utility to preserve newlines and spaces within an element. White space at the end of lines will not hang, but will wrap to the next line:

Hey everyone! It’s almost 2022 and we still don’t know if there are aliens living among us, or do we? Maybe the person writing this is an alien. You will never know.
    
    
    <p class="whitespace-break-spaces">Hey everyone!It's almost 2022       and we still don't know if there             are aliens living among us, or do we? Maybe the person writing this is an alien.You will never know.</p>

### [Responsive design](#responsive-design)

Prefix a `white-space` utility with a breakpoint variant like `md:` to only apply the utility at medium screen sizes and above:
    
    
    <p class="whitespace-pre md:whitespace-normal ...">  Lorem ipsum dolor sit amet...</p>

Learn more about using variants in the [variants documentation](/docs/hover-focus-and-other-states).

  * [Examples](#examples)
    * [Normal](#normal)
    * [No Wrap](#no-wrap)
    * [Pre](#pre)
    * [Pre Line](#pre-line)
    * [Pre Wrap](#pre-wrap)
    * [Break Spaces](#break-spaces)
    * [Responsive design](#responsive-design)
