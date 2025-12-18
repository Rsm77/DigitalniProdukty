# HTMX Knowledge Base

A comprehensive guide for applying HTMX in web applications. HTMX is a library that allows you to access modern browser features directly from HTML, rather than using JavaScript.

---

## Table of Contents

1. [Core Concept](#core-concept)
2. [Installation](#installation)
3. [AJAX Attributes](#ajax-attributes)
4. [Triggering Requests](#triggering-requests)
5. [Request Indicators](#request-indicators)
6. [Targets](#targets)
7. [Swapping Content](#swapping-content)
8. [CSS Transitions](#css-transitions)
9. [Out of Band Swaps](#out-of-band-swaps)
10. [Parameters](#parameters)
11. [Request Confirmation](#request-confirmation)
12. [Attribute Inheritance](#attribute-inheritance)
13. [Boosting](#boosting)
14. [History Support](#history-support)
15. [Requests & Responses](#requests--responses)
16. [Synchronization](#synchronization)
17. [Validation](#validation)
18. [Extensions](#extensions)
19. [Events & Logging](#events--logging)
20. [Scripting Integration](#scripting-integration)
21. [Security](#security)
22. [Configuration](#configuration)

---

## Core Concept

HTMX extends HTML's capabilities by allowing:

- **Any element** (not just anchors and forms) to issue HTTP requests
- **Any event** (not just clicks or form submissions) to trigger requests
- **Any HTTP verb** (GET, POST, PUT, PATCH, DELETE) to be used
- **Any element** (not just the entire window) to be the target for updates

> **Important**: When using htmx, the server typically responds with **HTML**, not JSON. This keeps you within the original web programming model (HATEOAS - Hypertext As The Engine Of Application State).

### Basic Example

Traditional anchor tag:
```html
<a href="/blog">Blog</a>
```

HTMX-powered button:
```html
<button hx-post="/clicked"
    hx-trigger="click"
    hx-target="#parent-div"
    hx-swap="outerHTML">
    Click Me!
</button>
```

This tells htmx: "When a user clicks on this button, issue an HTTP POST request to '/clicked' and use the content from the response to replace the element with the id `parent-div` in the DOM."

### Data Prefix Alternative

You can use the `data-` prefix for HTML5 compliance:
```html
<a data-hx-post="/click">Click Me!</a>
```

---

## Installation

HTMX is a dependency-free, browser-oriented JavaScript library.

### Via CDN (Recommended for Quick Start)

```html
<script src="https://cdn.jsdelivr.net/npm/htmx.org@2.0.8/dist/htmx.min.js" 
        integrity="sha384-/TgkGk7p307TH7EXJDuUlgG3Ce1UVolAOFopFekQkkXihi5u/6OCvVKyz1W+idaz" 
        crossorigin="anonymous"></script>
```

Unminified version:
```html
<script src="https://cdn.jsdelivr.net/npm/htmx.org@2.0.8/dist/htmx.js" 
        integrity="sha384-ezjq8118wdwdRMj+nX4bevEi+cDLTbhLAeFF688VK8tPDGeLUe0WoY2MZtSla72F" 
        crossorigin="anonymous"></script>
```

### Download and Self-Host

```html
<script src="/path/to/htmx.min.js"></script>
```

### Via npm

```bash
npm install htmx.org@2.0.8
```

### Webpack Integration

1. Install htmx via npm or yarn
2. Add import to `index.js`:
```javascript
import 'htmx.org';
```

3. To use the global `htmx` variable, create a custom JS file:
```javascript
window.htmx = require('htmx.org');
```

4. Import in `index.js`:
```javascript
import 'path/to/my_custom.js';
```

---

## AJAX Attributes

Core attributes for issuing AJAX requests:

| Attribute | Description |
|-----------|-------------|
| `hx-get` | Issues a GET request to the given URL |
| `hx-post` | Issues a POST request to the given URL |
| `hx-put` | Issues a PUT request to the given URL |
| `hx-patch` | Issues a PATCH request to the given URL |
| `hx-delete` | Issues a DELETE request to the given URL |

### Example

```html
<button hx-put="/messages">
    Put To Messages
</button>
```

This issues a PUT request to `/messages` when clicked, and loads the response into the button.

---

## Triggering Requests

### Default Triggers

By default, AJAX requests are triggered by the "natural" event of an element:

| Element Type | Default Trigger |
|--------------|-----------------|
| `input`, `textarea`, `select` | `change` event |
| `form` | `submit` event |
| Everything else | `click` event |

### Custom Triggers with hx-trigger

```html
<div hx-post="/mouse_entered" hx-trigger="mouseenter">
    [Here Mouse, Mouse!]
</div>
```

### Trigger Modifiers

| Modifier | Description |
|----------|-------------|
| `once` | Request happens only once |
| `changed` | Only issue request if value has changed |
| `delay:<time>` | Wait before issuing request (e.g., `delay:500ms`) |
| `throttle:<time>` | Throttle requests (e.g., `throttle:1s`) |
| `from:<selector>` | Listen for event on a different element |

### Active Search Pattern Example

```html
<input type="text" name="q"
    hx-get="/trigger_delay"
    hx-trigger="keyup changed delay:500ms"
    hx-target="#search-results"
    placeholder="Search...">
<div id="search-results"></div>
```

### Trigger Filters

Use square brackets with JavaScript expressions:

```html
<div hx-get="/clicked" hx-trigger="click[ctrlKey]">
    Control Click Me
</div>
```

### Special Events

| Event | Description |
|-------|-------------|
| `load` | Fires once when element is first loaded |
| `revealed` | Fires once when element scrolls into viewport |
| `intersect` | Fires when element intersects viewport |

Intersect options:
- `root:<selector>` - CSS selector of root element
- `threshold:<float>` - Intersection amount (0.0 to 1.0)

### Polling

```html
<div hx-get="/news" hx-trigger="every 2s"></div>
```

Stop polling by returning HTTP status code **286**.

### Load Polling

```html
<div hx-get="/messages"
    hx-trigger="load delay:1s"
    hx-swap="outerHTML">
</div>
```

Useful for progress bars that terminate when complete.

---

## Request Indicators

Show loading state during AJAX requests using the `htmx-indicator` class.

### Basic Usage

```html
<button hx-get="/click">
    Click Me!
    <img class="htmx-indicator" src="/spinner.gif" alt="Loading...">
</button>
```

### Custom CSS for Indicators

```css
.htmx-indicator {
    display: none;
}
.htmx-request .htmx-indicator {
    display: inline;
}
.htmx-request.htmx-indicator {
    display: inline;
}
```

### External Indicator

```html
<div>
    <button hx-get="/click" hx-indicator="#indicator">
        Click Me!
    </button>
    <img id="indicator" class="htmx-indicator" src="/spinner.gif" alt="Loading..."/>
</div>
```

### Disable Elements During Request

Use `hx-disabled-elt` attribute to add the `disabled` attribute during requests.

---

## Targets

Use `hx-target` to specify where the response should be loaded.

### Basic Usage

```html
<input type="text" name="q"
    hx-get="/trigger_delay"
    hx-trigger="keyup delay:500ms changed"
    hx-target="#search-results"
    placeholder="Search...">
<div id="search-results"></div>
```

### Extended CSS Selectors

| Syntax | Description |
|--------|-------------|
| `this` | The element itself |
| `closest <selector>` | Closest ancestor matching selector |
| `next <selector>` | Next element matching selector |
| `previous <selector>` | Previous element matching selector |
| `find <selector>` | First child descendant matching selector |

### Examples

```html
<!-- Target closest table row -->
<button hx-get="/data" hx-target="closest tr">Update Row</button>

<!-- Target next sibling div -->
<button hx-get="/data" hx-target="next div">Load Next</button>

<!-- Target using hyperscript-style syntax -->
<button hx-get="/data" hx-target="<#myElement/>">Load</button>
```

---

## Swapping Content

The `hx-swap` attribute controls how content is inserted into the DOM.

### Swap Strategies

| Value | Description |
|-------|-------------|
| `innerHTML` | Default - puts content inside target element |
| `outerHTML` | Replaces entire target element |
| `afterbegin` | Prepends before first child |
| `beforebegin` | Prepends before target in parent |
| `beforeend` | Appends after last child |
| `afterend` | Appends after target in parent |
| `delete` | Deletes target element |
| `none` | No content swap (OOB swaps still processed) |

### Swap Modifiers

| Option | Description |
|--------|-------------|
| `transition` | Use View Transition API (`true`/`false`) |
| `swap` | Swap delay (e.g., `swap:100ms`) |
| `settle` | Settle delay (e.g., `settle:100ms`) |
| `ignoreTitle` | Ignore title tags in response |
| `scroll` | Scroll target to `top` or `bottom` |
| `show` | Scroll target's top/bottom into view |

### Example with Modifiers

```html
<button hx-post="/like" hx-swap="outerHTML ignoreTitle:true">Like</button>
```

### Morph Swaps (via Extensions)

- **Idiomorph** - Morphing algorithm by htmx developers
- **Morphdom Swap** - Based on morphdom library
- **Alpine-morph** - Works well with Alpine.js

### View Transitions

Enable globally:
```javascript
htmx.config.globalViewTransitions = true;
```

Or per-element:
```html
<button hx-get="/page" hx-swap="innerHTML transition:true">Navigate</button>
```

---

## CSS Transitions

HTMX enables CSS transitions by preserving element IDs across swaps.

### How It Works

Original content:
```html
<div id="div1">Original Content</div>
```

New content from server:
```html
<div id="div1" class="red">New Content</div>
```

CSS transition:
```css
.red {
    color: red;
    transition: all ease-in 1s;
}
```

**Key**: Keep the element's `id` stable across requests to enable CSS transitions.

### Swap & Settle Model

1. Before swap, existing content is examined for matching `id` attributes
2. Old attributes are copied to new elements
3. Content is swapped with old attribute values
4. After settle delay (20ms default), new attribute values are applied
5. CSS transitions animate the change

---

## Out of Band Swaps

Swap content directly into DOM elements by ID using `hx-swap-oob`.

### Basic Usage

Server response:
```html
<div id="message" hx-swap-oob="true">Swap me directly!</div>
<!-- Additional content swapped into target normally -->
```

### Tables (Using Template)

```html
<template>
    <tr id="message" hx-swap-oob="true"><td>Joe</td><td>Smith</td></tr>
</template>
```

### Selecting Content

```html
<!-- Select subset from response -->
<button hx-get="/data" hx-select="#content">Load</button>

<!-- Select specific elements for OOB swap -->
<button hx-get="/data" hx-select-oob="#notification,#counter">Load</button>
```

### Preserving Content

```html
<video id="player" hx-preserve>
    <!-- Video continues playing during swaps -->
</video>
```

---

## Parameters

### Default Behavior

- Elements include their value if they have one
- Forms include all input values
- The `name` attribute is used as parameter name
- Non-GET requests include values from the associated form

### Including Additional Elements

```html
<input type="text" id="extra-field" name="extra">
<button hx-post="/submit" hx-include="#extra-field">Submit</button>
```

### Filtering Parameters

```html
<form hx-post="/submit" hx-params="*">...</form>      <!-- All parameters -->
<form hx-post="/submit" hx-params="none">...</form>   <!-- No parameters -->
<form hx-post="/submit" hx-params="field1,field2">...</form>  <!-- Specific -->
<form hx-post="/submit" hx-params="not field3">...</form>     <!-- Exclude -->
```

### File Upload

```html
<form hx-post="/upload" hx-encoding="multipart/form-data">
    <input type="file" name="document">
    <button type="submit">Upload</button>
</form>
```

Monitor progress with `htmx:xhr:progress` event.

### Extra Values

JSON format with `hx-vals`:
```html
<button hx-post="/action" hx-vals='{"key": "value"}'>Submit</button>
```

Dynamic with `hx-vars`:
```html
<button hx-post="/action" hx-vars="timestamp:Date.now()">Submit</button>
```

---

## Request Confirmation

### Simple Confirmation Dialog

```html
<button hx-delete="/account" hx-confirm="Are you sure you wish to delete your account?">
    Delete My Account
</button>
```

### Custom Confirmation with Events

```javascript
document.body.addEventListener('htmx:confirm', function(evt) {
    if (evt.target.matches("[confirm-with-sweet-alert='true']")) {
        evt.preventDefault();
        swal({
            title: "Are you sure?",
            text: "Are you sure you are sure?",
            icon: "warning",
            buttons: true,
            dangerMode: true,
        }).then((confirmed) => {
            if (confirmed) {
                evt.detail.issueRequest();
            }
        });
    }
});
```

Usage:
```html
<button hx-delete="/account" confirm-with-sweet-alert="true">Delete</button>
```

---

## Attribute Inheritance

Most htmx attributes are inherited by child elements.

### Hoisting Attributes

Before (duplicated):
```html
<button hx-delete="/account" hx-confirm="Are you sure?">Delete</button>
<button hx-put="/account" hx-confirm="Are you sure?">Update</button>
```

After (hoisted):
```html
<div hx-confirm="Are you sure?">
    <button hx-delete="/account">Delete My Account</button>
    <button hx-put="/account">Update My Account</button>
</div>
```

### Unsetting Inherited Attributes

```html
<div hx-confirm="Are you sure?">
    <button hx-delete="/account">Delete My Account</button>
    <button hx-put="/account">Update My Account</button>
    <button hx-confirm="unset" hx-get="/">Cancel</button>
</div>
```

### Disabling Inheritance

Per-element:
```html
<div hx-disinherit="hx-confirm">...</div>
```

Globally:
```javascript
htmx.config.disableInheritance = true;
```

Then explicitly enable with `hx-inherit`:
```html
<div hx-inherit="hx-target">...</div>
```

---

## Boosting

Convert regular links and forms to AJAX requests with `hx-boost`.

### Basic Usage

```html
<div hx-boost="true">
    <a href="/blog">Blog</a>
    <form action="/search" method="POST">...</form>
</div>
```

### Progressive Enhancement Pattern

```html
<form action="/search" method="POST">
    <input class="form-control" type="search"
        name="search" 
        placeholder="Begin typing to search users..."
        hx-post="/search"
        hx-trigger="keyup changed delay:500ms, search"
        hx-target="#search-results"
        hx-indicator=".htmx-indicator">
    <button type="submit">Search</button>
</form>
```

Benefits:
- JavaScript-enabled: Active search UX
- JavaScript-disabled: Standard form submission works

Check `HX-Request` header on server to differentiate requests.

### Accessibility Recommendations

- Use semantic HTML
- Ensure visible focus states
- Associate labels with form fields
- Maintain readability with appropriate fonts and contrast

---

## History Support

### Push URL to History

```html
<a hx-get="/blog" hx-push-url="true">Blog</a>
```

### How It Works

1. htmx snapshots current DOM before request
2. Makes request and swaps content
3. Pushes new location to history stack
4. Back button restores from cache or makes new request

### Custom History Element

```html
<div hx-history-elt>
    <!-- Only this element is snapshotted -->
</div>
```

### Cleaning Up 3rd Party Library Mutations

Initialize on load:
```javascript
htmx.onLoad(function(target) {
    target.querySelectorAll(".tomselect")
        .forEach(elt => new TomSelect(elt));
});
```

Clean up before history save:
```javascript
htmx.on('htmx:beforeHistorySave', function() {
    document.querySelectorAll('.tomSelect')
        .forEach(elt => elt.tomselect.destroy());
});
```

### Disable History Snapshots

```html
<div hx-history="false">
    <!-- Sensitive data not cached -->
</div>
```

---

## Requests & Responses

### Expected Response Format

htmx expects **HTML** responses (fragments or full documents).

### Special Response Codes

- **204 No Content**: htmx ignores response content
- **286**: Stops polling

### Response Handling Configuration

```html
<meta name="htmx-config" content='{
    "responseHandling":[
        {"code":"204", "swap": false},
        {"code":"[23]..", "swap": true},
        {"code":"422", "swap": true},
        {"code":"[45]..", "swap": false, "error":true},
        {"code":"...", "swap": true}
    ]
}'/>
```

### Request Headers

| Header | Description |
|--------|-------------|
| `HX-Boosted` | Request is via `hx-boost` element |
| `HX-Current-URL` | Current browser URL |
| `HX-History-Restore-Request` | History restoration request |
| `HX-Prompt` | User response to `hx-prompt` |
| `HX-Request` | Always "true" for htmx requests |
| `HX-Target` | ID of target element |
| `HX-Trigger-Name` | Name of triggered element |
| `HX-Trigger` | ID of triggered element |

### Response Headers

| Header | Description |
|--------|-------------|
| `HX-Location` | Client-side redirect without full reload |
| `HX-Push-Url` | Push new URL to history |
| `HX-Redirect` | Full client-side redirect |
| `HX-Refresh` | Full page refresh if "true" |
| `HX-Replace-Url` | Replace current URL |
| `HX-Reswap` | Override swap strategy |
| `HX-Retarget` | Change target element |
| `HX-Reselect` | Select part of response |
| `HX-Trigger` | Trigger client-side events |
| `HX-Trigger-After-Settle` | Trigger events after settle |
| `HX-Trigger-After-Swap` | Trigger events after swap |

### Request Order of Operations

1. Element triggered, request begins
2. Values gathered for request
3. `htmx-request` class applied
4. Request issued asynchronously
5. Response received, `htmx-swapping` class applied
6. Optional swap delay
7. Content swap done
8. `htmx-swapping` removed, `htmx-added` added
9. `htmx-settling` applied
10. Settle delay (default 20ms)
11. DOM settled
12. `htmx-settling` and `htmx-added` removed

---

## Synchronization

Coordinate requests between elements with `hx-sync`.

### Race Condition Example

Problem:
```html
<form hx-post="/store">
    <input id="title" name="title" type="text"
        hx-post="/validate"
        hx-trigger="change">
    <button type="submit">Submit</button>
</form>
```

Solution:
```html
<form hx-post="/store">
    <input id="title" name="title" type="text"
        hx-post="/validate"
        hx-trigger="change"
        hx-sync="closest form:abort">
    <button type="submit">Submit</button>
</form>
```

### Programmatic Request Cancellation

```html
<button id="request-button" hx-post="/example">
    Issue Request
</button>
<button onclick="htmx.trigger('#request-button', 'htmx:abort')">
    Cancel Request
</button>
```

---

## Validation

htmx integrates with HTML5 Validation API.

### Validation Events

| Event | Description |
|-------|-------------|
| `htmx:validation:validate` | Before `checkValidity()` is called |
| `htmx:validation:failed` | When `checkValidity()` returns false |
| `htmx:validation:halted` | Request not issued due to validation errors |

### Enable Validation on Non-Form Elements

```html
<div hx-get="/data" hx-validate="true">...</div>
```

### Custom Validation Example

```html
<form id="example-form" hx-post="/test">
    <input name="example"
           onkeyup="this.setCustomValidity('')"
           hx-on:htmx:validation:validate="if(this.value != 'foo') {
               this.setCustomValidity('Please enter the value foo');
               htmx.find('#example-form').reportValidity();
           }">
</form>
```

### Enable Browser Validation Reporting

```javascript
htmx.config.reportValidityOfForms = true;
```

---

## Extensions

### Core Extensions

| Extension | Description |
|-----------|-------------|
| `head-support` | Merge head tag information |
| `htmx-1-compat` | Restore htmx 1.x behavior |
| `idiomorph` | Morph swap strategy |
| `preload` | Preload content for performance |
| `response-targets` | Target elements by response code |
| `sse` | Server Sent Events support |
| `ws` | WebSocket support |

### Installing Extensions (CDN)

```html
<head>
    <script src="https://cdn.jsdelivr.net/npm/htmx.org@2.0.8/dist/htmx.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/htmx-ext-response-targets@2.0.4"></script>
</head>
<body hx-ext="response-targets">
    ...
</body>
```

### Installing Extensions (npm)

```bash
npm install htmx-ext-response-targets
```

```javascript
import 'htmx.org';
import 'htmx-ext-response-targets';
```

### Enabling Extensions

```html
<body hx-ext="response-targets">
    <button hx-post="/register" 
            hx-target="#response-div" 
            hx-target-404="#not-found">
        Register!
    </button>
    <div id="response-div"></div>
    <div id="not-found"></div>
</body>
```

---

## Events & Logging

### Listening to Events

```javascript
// Standard addEventListener
document.body.addEventListener('htmx:load', function(evt) {
    myJavascriptLib.init(evt.detail.elt);
});

// htmx helper
htmx.on("htmx:load", function(evt) {
    myJavascriptLib.init(evt.detail.elt);
});

// Simplified onLoad helper
htmx.onLoad(function(target) {
    myJavascriptLib.init(target);
});
```

### Common Event Uses

#### Initialize 3rd Party Libraries

```javascript
htmx.onLoad(function(target) {
    myJavascriptLib.init(target);
});
```

#### Configure Requests

```javascript
document.body.addEventListener('htmx:configRequest', function(evt) {
    evt.detail.parameters['auth_token'] = getAuthToken();
    evt.detail.headers['Authentication-Token'] = getAuthToken();
});
```

#### Modify Swap Behavior

```javascript
document.body.addEventListener('htmx:beforeSwap', function(evt) {
    if(evt.detail.xhr.status === 404) {
        alert("Error: Could Not Find Resource");
    } else if(evt.detail.xhr.status === 422) {
        evt.detail.shouldSwap = true;
        evt.detail.isError = false;
    } else if(evt.detail.xhr.status === 418) {
        evt.detail.shouldSwap = true;
        evt.detail.target = htmx.find("#teapot");
    }
});
```

### Event Naming

Events fire in both formats:
- Camel Case: `htmx:afterSwap`
- Kebab Case: `htmx:after-swap`

### Logging

```javascript
htmx.logger = function(elt, event, data) {
    if(console) {
        console.log(event, elt, data);
    }
};
```

### Debugging

```javascript
// Log all htmx events
htmx.logAll();

// Monitor all events on an element (console only)
monitorEvents(htmx.find("#theElement"));
```

---

## Scripting Integration

### The hx-on* Attributes

Respond to any event with inline scripting:

```html
<button hx-on:click="alert('You clicked me!')">
    Click Me!
</button>
```

### Handling htmx Events

```html
<button hx-post="/example"
        hx-on:htmx:config-request="event.detail.parameters.example = 'Hello Scripting!'">
    Post Me!
</button>
```

### 3rd Party Library Integration (SortableJS Example)

HTML:
```html
<form class="sortable" hx-post="/items" hx-trigger="end">
    <div class="htmx-indicator">Updating...</div>
    <div><input type='hidden' name='item' value='1'/>Item 1</div>
    <div><input type='hidden' name='item' value='2'/>Item 2</div>
    <div><input type='hidden' name='item' value='3'/>Item 3</div>
</form>
```

JavaScript:
```javascript
htmx.onLoad(function(content) {
    var sortables = content.querySelectorAll(".sortable");
    for (var i = 0; i < sortables.length; i++) {
        var sortable = sortables[i];
        new Sortable(sortable, {
            animation: 150,
            ghostClass: 'blue-background-class'
        });
    }
});
```

### Process Dynamically Added Content

```javascript
let myDiv = document.getElementById('my-div');
fetch('http://example.com/data')
    .then(response => response.text())
    .then(data => { 
        myDiv.innerHTML = data; 
        htmx.process(myDiv);  // Initialize htmx attributes
    });
```

### Alpine.js Integration

```html
<div x-data="{show_new: false}"
    x-init="$watch('show_new', value => {
        if (show_new) {
            htmx.process(document.querySelector('#new_content'))
        }
    })">
    <button @click="show_new = !show_new">Toggle New Content</button>
    <template x-if="show_new">
        <div id="new_content">
            <a hx-get="/server/newstuff" href="#">New Clickable</a>
        </div>
    </template>
</div>
```

### Recommended Scripting Libraries

- **VanillaJS** - Lightweight, built-in JavaScript
- **Alpine.js** - Lightweight reactive framework
- **jQuery** - Good for legacy codebases
- **hyperscript** - Designed to pair with htmx

---

## Security

### Rule 1: Escape All User Content

Always escape untrusted content to prevent XSS attacks. Most templating languages support automatic escaping.

If injecting raw HTML, whitelist allowed attributes and tags rather than blacklisting.

### hx-disable

Prevent htmx processing on untrusted content:

```html
<div hx-disable>
    <%= raw(user_content) %>
</div>
```

### hx-history

Prevent sensitive data from being cached:

```html
<div hx-history="false">
    <!-- Sensitive content -->
</div>
```

### Security Configuration Options

| Option | Description |
|--------|-------------|
| `htmx.config.selfRequestsOnly` | Only allow same-domain requests (default: `true`) |
| `htmx.config.allowScriptTags` | Process script tags (default: `true`) |
| `htmx.config.historyCacheSize` | Set to `0` to disable caching |
| `htmx.config.allowEval` | Disable eval-based features (default: `true`) |

### URL Validation Event

```javascript
document.body.addEventListener('htmx:validateUrl', function(evt) {
    if (!evt.detail.sameHost && evt.detail.url.hostname !== "myserver.com") {
        evt.preventDefault();
    }
});
```

### Content Security Policy

```html
<meta http-equiv="Content-Security-Policy" content="default-src 'self';">
```

### CSRF Prevention

```html
<html lang="en" hx-headers='{"X-CSRF-TOKEN": "CSRF_TOKEN_INSERTED_HERE"}'>
    ...
</html>
```

Or on body:
```html
<body hx-headers='{"X-CSRF-TOKEN": "CSRF_TOKEN_INSERTED_HERE"}'>
    ...
</body>
```

---

## Configuration

### Setting Configuration

Via JavaScript:
```javascript
htmx.config.defaultSwapStyle = "outerHTML";
```

Via meta tag:
```html
<meta name="htmx-config" content='{"defaultSwapStyle":"outerHTML"}'>
```

### Configuration Options Reference

| Variable | Default | Description |
|----------|---------|-------------|
| `historyEnabled` | `true` | Enable history feature |
| `historyCacheSize` | `10` | Number of pages to cache |
| `refreshOnHistoryMiss` | `false` | Full refresh on cache miss |
| `defaultSwapStyle` | `innerHTML` | Default swap strategy |
| `defaultSwapDelay` | `0` | Delay before swap |
| `defaultSettleDelay` | `20` | Delay for settling |
| `includeIndicatorStyles` | `true` | Include indicator CSS |
| `indicatorClass` | `htmx-indicator` | Indicator class name |
| `requestClass` | `htmx-request` | Request class name |
| `addedClass` | `htmx-added` | Added content class |
| `settlingClass` | `htmx-settling` | Settling class name |
| `swappingClass` | `htmx-swapping` | Swapping class name |
| `allowEval` | `true` | Allow eval for filters |
| `allowScriptTags` | `true` | Process script tags |
| `inlineScriptNonce` | `''` | Nonce for inline scripts |
| `inlineStyleNonce` | `''` | Nonce for inline styles |
| `attributesToSettle` | `["class", "style", "width", "height"]` | Attributes to settle |
| `useTemplateFragments` | `false` | Use template tags for parsing |
| `wsReconnectDelay` | `full-jitter` | WebSocket reconnect delay |
| `wsBinaryType` | `blob` | WebSocket binary type |
| `disableSelector` | `[hx-disable], [data-hx-disable]` | Disable htmx selector |
| `withCredentials` | `false` | Send credentials cross-site |
| `timeout` | `0` | Request timeout (ms) |
| `scrollBehavior` | `instant` | Scroll behavior |
| `defaultFocusScroll` | `false` | Scroll focused element |
| `getCacheBusterParam` | `false` | Add cache-buster param |
| `globalViewTransitions` | `false` | Use View Transitions API |
| `methodsThatUseUrlParams` | `["get", "delete"]` | Methods using URL params |
| `selfRequestsOnly` | `true` | Same-domain requests only |
| `ignoreTitle` | `false` | Ignore title tags |
| `disableInheritance` | `false` | Disable attribute inheritance |
| `scrollIntoViewOnBoost` | `true` | Scroll on boost |
| `triggerSpecsCache` | `null` | Cache for trigger specs |
| `allowNestedOobSwaps` | `true` | Process nested OOB swaps |
| `historyRestoreAsHxRequest` | `true` | History restore as HX-Request |

---

## Creating Demos

For testing and bug reports, use the htmx demo environment:

```html
<!-- Load demo environment -->
<script src="https://demo.htmx.org"></script>

<!-- Your htmx code -->
<button hx-post="/foo" hx-target="#result">
    Count Up
</button>
<output id="result"></output>

<!-- Mock response -->
<script>
    globalInt = 0;
</script>
<template url="/foo" delay="500">
    ${globalInt++}
</template>
```

---

## Quick Reference - Common Patterns

### Active Search

```html
<input type="text" name="q"
    hx-get="/search"
    hx-trigger="keyup changed delay:500ms"
    hx-target="#results">
<div id="results"></div>
```

### Infinite Scroll

```html
<div hx-get="/items?page=2"
     hx-trigger="revealed"
     hx-swap="afterend">
    Loading more...
</div>
```

### Click to Edit

```html
<div hx-get="/edit" hx-trigger="click" hx-swap="outerHTML">
    Click to edit
</div>
```

### Delete with Confirmation

```html
<button hx-delete="/item/1" 
        hx-confirm="Are you sure?"
        hx-target="closest tr"
        hx-swap="outerHTML">
    Delete
</button>
```

### Modal Dialog

```html
<button hx-get="/modal" hx-target="#modal-container">
    Open Modal
</button>
<div id="modal-container"></div>
```

### Lazy Loading

```html
<div hx-get="/content" hx-trigger="load">
    <img class="htmx-indicator" src="/spinner.gif">
</div>
```

### Progress Bar (Load Polling)

```html
<div hx-get="/progress"
     hx-trigger="load delay:500ms"
     hx-swap="outerHTML">
    <progress value="0" max="100"></progress>
</div>
```

### Tabs

```html
<div class="tabs">
    <button hx-get="/tab1" hx-target="#tab-content" class="active">Tab 1</button>
    <button hx-get="/tab2" hx-target="#tab-content">Tab 2</button>
    <button hx-get="/tab3" hx-target="#tab-content">Tab 3</button>
</div>
<div id="tab-content">
    <!-- Tab content loaded here -->
</div>
```

### Inline Editing

```html
<span hx-get="/edit/1" hx-trigger="dblclick" hx-swap="outerHTML">
    Double-click to edit
</span>
```

### Bulk Delete

```html
<form hx-delete="/items" hx-confirm="Delete selected items?">
    <input type="checkbox" name="ids" value="1">
    <input type="checkbox" name="ids" value="2">
    <input type="checkbox" name="ids" value="3">
    <button type="submit">Delete Selected</button>
</form>
```

### Toggle Button

```html
<button hx-post="/toggle" hx-swap="outerHTML">
    Toggle State
</button>
```

---

## Version Information

- **Current Version**: 2.0.8
- **IE11 Support**: Use htmx 1.x
- **Migration**: See official migration guides for 1.x to 2.x

---

## Caching

htmx works with standard HTTP caching mechanisms.

### Last-Modified / If-Modified-Since

If your server adds the `Last-Modified` header, the browser automatically adds `If-Modified-Since` to subsequent requests.

### Using Vary Header

If your server renders different content based on `HX-Request` header:
```
Vary: HX-Request
```

### Cache Buster Parameter

```javascript
htmx.config.getCacheBusterParam = true;
```

### ETag Support

htmx works with ETag. Ensure different ETags for different content based on `HX-Request` header.

---

## WebSockets & Server-Sent Events

Supported via extensions:

### Server-Sent Events (SSE)

```html
<body hx-ext="sse">
    <div sse-connect="/events" sse-swap="message">
        <!-- Content updated on SSE messages -->
    </div>
</body>
```

### WebSockets

```html
<body hx-ext="ws">
    <div ws-connect="/chat">
        <form ws-send>
            <input name="message">
            <button>Send</button>
        </form>
    </div>
</body>
```

---

## CORS Configuration

When using htmx cross-origin, configure your server:

```
Access-Control-Allow-Headers: HX-Request, HX-Trigger, HX-Target, HX-Current-URL
Access-Control-Expose-Headers: HX-Location, HX-Push-Url, HX-Redirect, HX-Refresh, HX-Replace-Url, HX-Reswap, HX-Retarget, HX-Trigger, HX-Trigger-After-Settle, HX-Trigger-After-Swap
```

---

*This knowledge base is based on the official htmx documentation. For the most up-to-date information, visit [htmx.org](https://htmx.org).*
