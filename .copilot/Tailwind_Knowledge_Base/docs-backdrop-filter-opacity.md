# backdrop-filter: opacity() - Filters - Tailwind CSS

[](/)v4.1

  1. Filters
  2. opacity

Filters

# backdrop-filter: opacity()

Utilities for applying backdrop opacity filters to an element.

Class| Styles  
---|---  
`backdrop-opacity-<number>`| `backdrop-filter: opacity(<number>%);`  
`backdrop-opacity-(<custom-property>)`| `backdrop-filter: opacity(var(<custom-property>));`  
`backdrop-opacity-[<value>]`| `backdrop-filter: opacity(<value>);`  
  
## [Examples](#examples)

### [Basic example](#basic-example)

Use utilities like `backdrop-opacity-50` and `backdrop-opacity-75` to control the opacity of all the backdrop filters applied to an element:

backdrop-opacity-10

![](https://images.unsplash.com/photo-1554629947-334ff61d85dc?ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&ixlib=rb-1.2.1&auto=format&fit=crop&w=1000&h=1000&q=90)

backdrop-opacity-60

![](https://images.unsplash.com/photo-1554629947-334ff61d85dc?ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&ixlib=rb-1.2.1&auto=format&fit=crop&w=1000&h=1000&q=90)

backdrop-opacity-95

![](https://images.unsplash.com/photo-1554629947-334ff61d85dc?ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&ixlib=rb-1.2.1&auto=format&fit=crop&w=1000&h=1000&q=90)
    
    
    <div class="bg-[url(/img/mountains.jpg)]">  <div class="bg-white/30 backdrop-invert backdrop-opacity-10 ..."></div></div><div class="bg-[url(/img/mountains.jpg)]">  <div class="bg-white/30 backdrop-invert backdrop-opacity-60 ..."></div></div><div class="bg-[url(/img/mountains.jpg)]">  <div class="bg-white/30 backdrop-invert backdrop-opacity-95 ..."></div></div>

### [Using a custom value](#using-a-custom-value)

Use the `backdrop-opacity-[<value>]` syntax to set the backdrop filter opacity based on a completely custom value:
    
    
    <div class="backdrop-opacity-[.15] ...">  <!-- ... --></div>

For CSS variables, you can also use the `backdrop-opacity-(<custom-property>)` syntax:
    
    
    <div class="backdrop-opacity-(--my-backdrop-filter-opacity) ...">  <!-- ... --></div>

This is just a shorthand for `backdrop-opacity-[var(<custom-property>)]` that adds the `var()` function for you automatically.

### [Responsive design](#responsive-design)

Prefix a `backdrop-filter: opacity()` utility with a breakpoint variant like `md:` to only apply the utility at medium screen sizes and above:
    
    
    <div class="backdrop-opacity-100 md:backdrop-opacity-60 ...">  <!-- ... --></div>

Learn more about using variants in the [variants documentation](/docs/hover-focus-and-other-states).

  * [Examples](#examples)
    * [Basic example](#basic-example)
    * [Using a custom value](#using-a-custom-value)
    * [Responsive design](#responsive-design)
