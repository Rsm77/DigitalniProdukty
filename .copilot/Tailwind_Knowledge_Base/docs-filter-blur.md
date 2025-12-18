# filter: blur() - Filters - Tailwind CSS

[](/)v4.1

  1. Filters
  2. blur

Filters

# filter: blur()

Utilities for applying blur filters to an element.

Class| Styles  
---|---  
`blur-xs`| `filter: blur(var(--blur-xs)); /* 4px */`  
`blur-sm`| `filter: blur(var(--blur-sm)); /* 8px */`  
`blur-md`| `filter: blur(var(--blur-md)); /* 12px */`  
`blur-lg`| `filter: blur(var(--blur-lg)); /* 16px */`  
`blur-xl`| `filter: blur(var(--blur-xl)); /* 24px */`  
`blur-2xl`| `filter: blur(var(--blur-2xl)); /* 40px */`  
`blur-3xl`| `filter: blur(var(--blur-3xl)); /* 64px */`  
`blur-none`| `filter: ;`  
`blur-(<custom-property>)`| `filter: blur(var(<custom-property>));`  
`blur-[<value>]`| `filter: blur(<value>);`  
  
## [Examples](#examples)

### [Basic example](#basic-example)

Use utilities like `blur-sm` and `blur-lg` to blur an element:

blur-none

![](https://images.unsplash.com/photo-1554629947-334ff61d85dc?ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&ixlib=rb-1.2.1&auto=format&fit=crop&w=1000&h=1000&q=90)

blur-sm

![](https://images.unsplash.com/photo-1554629947-334ff61d85dc?ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&ixlib=rb-1.2.1&auto=format&fit=crop&w=1000&h=1000&q=90)

blur-lg

![](https://images.unsplash.com/photo-1554629947-334ff61d85dc?ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&ixlib=rb-1.2.1&auto=format&fit=crop&w=1000&h=1000&q=90)

blur-2xl

![](https://images.unsplash.com/photo-1554629947-334ff61d85dc?ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&ixlib=rb-1.2.1&auto=format&fit=crop&w=1000&h=1000&q=90)
    
    
    <img class="blur-none" src="/img/mountains.jpg" /><img class="blur-sm" src="/img/mountains.jpg" /><img class="blur-lg" src="/img/mountains.jpg" /><img class="blur-2xl" src="/img/mountains.jpg" />

### [Using a custom value](#using-a-custom-value)

Use the `blur-[<value>]` syntax to set the blur based on a completely custom value:
    
    
    <img class="blur-[2px] ..." src="/img/mountains.jpg" />

For CSS variables, you can also use the `blur-(<custom-property>)` syntax:
    
    
    <img class="blur-(--my-blur) ..." src="/img/mountains.jpg" />

This is just a shorthand for `blur-[var(<custom-property>)]` that adds the `var()` function for you automatically.

### [Responsive design](#responsive-design)

Prefix a `filter: blur()` utility with a breakpoint variant like `md:` to only apply the utility at medium screen sizes and above:
    
    
    <img class="blur-none md:blur-lg ..." src="/img/mountains.jpg" />

Learn more about using variants in the [variants documentation](/docs/hover-focus-and-other-states).

## [Customizing your theme](#customizing-your-theme)

Use the `--blur-*` theme variables to customize the blur utilities in your project:
    
    
    @theme {  --blur-2xs: 2px; }

Now the `blur-2xs` utility can be used in your markup:
    
    
    <img class="blur-2xs" src="/img/mountains.jpg" />

Learn more about customizing your theme in the [theme documentation](/docs/theme#customizing-your-theme).

  * [Examples](#examples)
    * [Basic example](#basic-example)
    * [Using a custom value](#using-a-custom-value)
    * [Responsive design](#responsive-design)
  * [Customizing your theme](#customizing-your-theme)
