# object-position - Layout - Tailwind CSS

[](/)v4.1

  1. Layout
  2. object-position

Layout

# object-position

Utilities for controlling how a replaced element's content should be positioned within its container.

Class| Styles  
---|---  
`object-top-left`| `object-position: top left;`  
`object-top`| `object-position: top;`  
`object-top-right`| `object-position: top right;`  
`object-left`| `object-position: left;`  
`object-center`| `object-position: center;`  
`object-right`| `object-position: right;`  
`object-bottom-left`| `object-position: bottom left;`  
`object-bottom`| `object-position: bottom;`  
`object-bottom-right`| `object-position: bottom right;`  
`object-(<custom-property>)`| `object-position: var(<custom-property>);`  
`object-[<value>]`| `object-position: <value>;`  
  
## [Examples](#examples)

### [Basic example](#basic-example)

Use utilities like `object-left` and `object-bottom-right` to specify how a replaced element's content should be positioned within its container:

Hover over examples to see the full image

object-top-left

![](https://images.unsplash.com/photo-1554629947-334ff61d85dc?ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&ixlib=rb-1.2.1&auto=format&fit=crop&w=1000&h=1000&q=90)

object-top

![](https://images.unsplash.com/photo-1554629947-334ff61d85dc?ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&ixlib=rb-1.2.1&auto=format&fit=crop&w=1000&h=1000&q=90)

object-top-right

![](https://images.unsplash.com/photo-1554629947-334ff61d85dc?ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&ixlib=rb-1.2.1&auto=format&fit=crop&w=1000&h=1000&q=90)

object-left

![](https://images.unsplash.com/photo-1554629947-334ff61d85dc?ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&ixlib=rb-1.2.1&auto=format&fit=crop&w=1000&h=1000&q=90)

object-center

![](https://images.unsplash.com/photo-1554629947-334ff61d85dc?ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&ixlib=rb-1.2.1&auto=format&fit=crop&w=1000&h=1000&q=90)

object-right

![](https://images.unsplash.com/photo-1554629947-334ff61d85dc?ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&ixlib=rb-1.2.1&auto=format&fit=crop&w=1000&h=1000&q=90)

object-bottom-left

![](https://images.unsplash.com/photo-1554629947-334ff61d85dc?ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&ixlib=rb-1.2.1&auto=format&fit=crop&w=1000&h=1000&q=90)

object-bottom

![](https://images.unsplash.com/photo-1554629947-334ff61d85dc?ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&ixlib=rb-1.2.1&auto=format&fit=crop&w=1000&h=1000&q=90)

object-bottom-right

![](https://images.unsplash.com/photo-1554629947-334ff61d85dc?ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&ixlib=rb-1.2.1&auto=format&fit=crop&w=1000&h=1000&q=90)
    
    
    <img class="size-24 object-top-left ..." src="/img/mountains.jpg" /><img class="size-24 object-top ..." src="/img/mountains.jpg" /><img class="size-24 object-top-right ..." src="/img/mountains.jpg" /><img class="size-24 object-left ..." src="/img/mountains.jpg" /><img class="size-24 object-center ..." src="/img/mountains.jpg" /><img class="size-24 object-right ..." src="/img/mountains.jpg" /><img class="size-24 object-bottom-left ..." src="/img/mountains.jpg" /><img class="size-24 object-bottom ..." src="/img/mountains.jpg" /><img class="size-24 object-bottom-right ..." src="/img/mountains.jpg" />

### [Using a custom value](#using-a-custom-value)

Use the `object-[<value>]` syntax to set the object position based on a completely custom value:
    
    
    <img class="object-[25%_75%] ..." src="/img/mountains.jpg" />

For CSS variables, you can also use the `object-(<custom-property>)` syntax:
    
    
    <img class="object-(--my-object) ..." src="/img/mountains.jpg" />

This is just a shorthand for `object-[var(<custom-property>)]` that adds the `var()` function for you automatically.

### [Responsive design](#responsive-design)

Prefix an `object-position` utility with a breakpoint variant like `md:` to only apply the utility at medium screen sizes and above:
    
    
    <img class="object-center md:object-top ..." src="/img/mountains.jpg" />

Learn more about using variants in the [variants documentation](/docs/hover-focus-and-other-states).

  * [Examples](#examples)
    * [Basic example](#basic-example)
    * [Using a custom value](#using-a-custom-value)
    * [Responsive design](#responsive-design)
