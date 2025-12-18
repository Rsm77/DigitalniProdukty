# text-indent - Typography - Tailwind CSS

[](/)v4.1

  1. Typography
  2. text-indent

Typography

# text-indent

Utilities for controlling the amount of empty space shown before text in a block.

Class| Styles  
---|---  
`indent-<number>`| `text-indent: calc(var(--spacing) * <number>);`  
`-indent-<number>`| `text-indent: calc(var(--spacing) * -<number>);`  
`indent-px`| `text-indent: 1px;`  
`-indent-px`| `text-indent: -1px;`  
`indent-(<custom-property>)`| `text-indent: var(<custom-property>);`  
`indent-[<value>]`| `text-indent: <value>;`  
  
## [Examples](#examples)

### [Basic example](#basic-example)

Use `indent-<number>` utilities like `indent-2` and `indent-8` to set the amount of empty space (indentation) that's shown before text in a block:

So I started to walk into the water. I won't lie to you boys, I was terrified. But I pressed on, and as I made my way past the breakers a strange calm came over me. I don't know if it was divine intervention or the kinship of all living things but I tell you Jerry at that moment, I _was_ a marine biologist.
    
    
    <p class="indent-8">So I started to walk into the water...</p>

### [Using negative values](#using-negative-values)

To use a negative text indent value, prefix the class name with a dash to convert it to a negative value:

So I started to walk into the water. I won't lie to you boys, I was terrified. But I pressed on, and as I made my way past the breakers a strange calm came over me. I don't know if it was divine intervention or the kinship of all living things but I tell you Jerry at that moment, I _was_ a marine biologist.
    
    
    <p class="-indent-8">So I started to walk into the water...</p>

### [Using a custom value](#using-a-custom-value)

Use the `indent-[<value>]` syntax to set the text indentation based on a completely custom value:
    
    
    <p class="indent-[50%] ...">  Lorem ipsum dolor sit amet...</p>

For CSS variables, you can also use the `indent-(<custom-property>)` syntax:
    
    
    <p class="indent-(--my-indentation) ...">  Lorem ipsum dolor sit amet...</p>

This is just a shorthand for `indent-[var(<custom-property>)]` that adds the `var()` function for you automatically.

### [Responsive design](#responsive-design)

Prefix a `text-indent` utility with a breakpoint variant like `md:` to only apply the utility at medium screen sizes and above:
    
    
    <p class="indent-4 md:indent-8 ...">  Lorem ipsum dolor sit amet...</p>

Learn more about using variants in the [variants documentation](/docs/hover-focus-and-other-states).

  * [Examples](#examples)
    * [Basic example](#basic-example)
    * [Using negative values](#using-negative-values)
    * [Using a custom value](#using-a-custom-value)
    * [Responsive design](#responsive-design)
