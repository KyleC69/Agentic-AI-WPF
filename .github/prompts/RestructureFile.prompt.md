Refactor the active file into a clean, professional architectural pattern. 
Identify the most appropriate pattern for the file’s purpose and rewrite its structure accordingly.

Determine which pattern best fits the file’s intent:
- Repository
- Service
- Factory
- Adapter
- Strategy
- Mediator
- Pipeline
- Command
- Query
- Value Object
- Aggregate Root

Perform the following:

1. Identify the file’s true responsibility.
   - Determine what the file is actually doing versus what it should be doing.
   - Highlight mixed concerns or responsibilities that should be separated.

2. Select the correct architectural pattern.
   - Explain why this pattern is the best fit.
   - Identify the core components the pattern requires.

3. Rewrite the file’s structure.
   - Provide a clean outline of classes, methods, and responsibilities.
   - Show how dependencies should be injected.
   - Show how interfaces or abstractions should be introduced.
   - Remove or relocate responsibilities that do not belong in this file.

4. Improve production-grade qualities.
   - Add guard clauses, error boundaries, and predictable behavior.
   - Improve naming, method grouping, and logical flow.
   - Ensure testability and extensibility.

Return:
- The chosen pattern and justification
- A list of structural issues in the current file
- A rewritten architectural outline for the file
- A short example of how the refactored file should look