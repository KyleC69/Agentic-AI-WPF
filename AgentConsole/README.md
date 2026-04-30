
## Agent Console

### *PURPOSE:* An *optional* external floating window that captures the AI logging to assist in troubleshooting tools or models reasoning etc. 

- Does not create any dependencies
- Completely optional
- Works with or without IDE
- Works in debugger or without

  ### USAGE

  - Add ILoggerFactories in Agent options or .ctors, or create a LoggingClient and wire into your pipelines
  - Add console logger
  - Enable trace level logging
 
  - Now you have verbose logging of tool, reasoning, and models interactions in a versatile movable window.
