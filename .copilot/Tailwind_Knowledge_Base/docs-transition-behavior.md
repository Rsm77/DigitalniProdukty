# transition-behavior - Transitions & Animation - Tailwind CSS

[](/)v4.1

  1. Transitions & Animation
  2. transition-behavior

Transitions & Animation

# transition-behavior

Utilities to control the behavior of CSS transitions.

Class| Styles  
---|---  
`transition-normal`| `transition-behavior: normal;`  
`transition-discrete`| `transition-behavior: allow-discrete;`  
  
## [Examples](#examples)

### [Basic example](#basic-example)

Use the `transition-discrete` utility to start transitions when changing properties with a discrete set of values, such as elements that change from `hidden` to `block`:

Interact with the checkboxes to see the expected behavior

transition-normalI hide

transition-discreteI fade out
    
    
    <label class="peer ...">  <input type="checkbox" checked /></label><button class="hidden transition-all not-peer-has-checked:opacity-0 peer-has-checked:block ...">  I hide</button><label class="peer ...">  <input type="checkbox" checked /></label><button class="hidden transition-all transition-discrete not-peer-has-checked:opacity-0 peer-has-checked:block ...">  I fade out</button>

### [Responsive design](#responsive-design)

Prefix a `transition-behavior` utility with a breakpoint variant like `md:` to only apply the utility at medium screen sizes and above:
    
    
    <button class="transition-discrete md:transition-normal ...">  <!-- ... --></button>

Learn more about using variants in the [variants documentation](/docs/hover-focus-and-other-states).

  * [Examples](#examples)
    * [Basic example](#basic-example)
    * [Responsive design](#responsive-design)
