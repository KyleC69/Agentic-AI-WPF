Analyze the active file for architectural structure, clarity, and maintainability. 
Focus on identifying missing or weak architectural patterns and opportunities to improve the file’s organization.

Evaluate the following:

1. Cohesion and Responsibility
   - Does the file follow single-responsibility principles?
   - Are there mixed concerns that should be separated?
   - Are there hidden responsibilities or implicit behaviors?

2. Dependency Quality
   - Are dependencies injected cleanly?
   - Are there unnecessary static calls, service locators, or tight couplings?
   - Are abstractions missing where they would improve testability?

3. Pattern Alignment
   - Identify which architectural patterns this file *should* follow (e.g., Repository, Service, Factory, Adapter, Mediator, Pipeline, Strategy).
   - Point out where the implementation deviates from the expected pattern.
   - Suggest the correct pattern and show how the file could align with it.

4. Structure and Readability
   - Evaluate method ordering, naming, and logical grouping.
   - Identify overly dense methods, unclear flows, or missing helper abstractions.
   - Suggest reorganizations that improve clarity and intent.

5. Error Handling and Robustness
   - Identify missing guard clauses, null checks, and exception boundaries.
   - Suggest improvements for reliability and predictable behavior.

6. Production-Grade Expectations
   - Highlight anything that prevents this file from being considered production quality.
   - Recommend specific structural improvements, not just code tweaks.

Return your findings as:
- A concise summary of architectural issues
- A list of recommended structural improvements
- A rewritten outline showing how the file *should* be organized