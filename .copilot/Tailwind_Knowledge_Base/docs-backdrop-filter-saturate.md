# backdrop-filter: saturate() - Filters - Tailwind CSS

[](/)v4.1

  1. Filters
  2. saturate

Filters

# backdrop-filter: saturate()

Utilities for applying backdrop saturation filters to an element.

Class| Styles  
---|---  
`backdrop-saturate-<number>`| `backdrop-filter: saturate(<number>%);`  
`backdrop-saturate-(<custom-property>)`| `backdrop-filter: saturate(var(<custom-property>));`  
`backdrop-saturate-[<value>]`| `backdrop-filter: saturate(<value>);`  
  
## [Examples](#examples)

### [Basic example](#basic-example)

Use utilities like `backdrop-saturate-50` and `backdrop-saturate-100` utilities to control the saturation of an element's backdrop:

backdrop-saturate-50

![](https://images.unsplash.com/photo-1554629947-334ff61d85dc?ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&ixlib=rb-1.2.1&auto=format&fit=crop&w=1000&h=1000&q=90)

backdrop-saturate-125

![](https://images.unsplash.com/photo-1554629947-334ff61d85dc?ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&ixlib=rb-1.2.1&auto=format&fit=crop&w=1000&h=1000&q=90)

backdrop-saturate-200

![](https://images.unsplash.com/photo-1554629947-334ff61d85dc?ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&ixlib=rb-1.2.1&auto=format&fit=crop&w=1000&h=1000&q=90)
    
    
    <div class="bg-[url(/img/mountains.jpg)]">  <div class="bg-white/30 backdrop-saturate-50 ..."></div></div><div class="bg-[url(/img/mountains.jpg)]">  <div class="bg-white/30 backdrop-saturate-125 ..."></div></div><div class="bg-[url(/img/mountains.jpg)]">  <div class="bg-white/30 backdrop-saturate-200 ..."></div></div>

### [Using a custom value](#using-a-custom-value)

Use the `backdrop-saturate-[<value>]` syntax to set the backdrop saturation based on a completely custom value:
    
    
    <div class="backdrop-saturate-[.25] ...">  <!-- ... --></div>

For CSS variables, you can also use the `backdrop-saturate-(<custom-property>)` syntax:
    
    
    <div class="backdrop-saturate-(--my-backdrop-saturation) ...">  <!-- ... --></div>

This is just a shorthand for `backdrop-saturate-[var(<custom-property>)]` that adds the `var()` function for you automatically.

### [Responsive design](#responsive-design)

Prefix a `backdrop-filter: saturate()` utility with a breakpoint variant like `md:` to only apply the utility at medium screen sizes and above:
    
    
    <div class="backdrop-saturate-50 md:backdrop-saturate-150 ...">  <!-- ... --></div>

Learn more about using variants in the [variants documentation](/docs/hover-focus-and-other-states).

  * [Examples](#examples)
    * [Basic example](#basic-example)
    * [Using a custom value](#using-a-custom-value)
    * [Responsive design](#responsive-design)
