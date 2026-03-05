Analyze the entire project for architectural structure, layering, and professional-grade organization. 
Identify missing patterns, weak boundaries, and opportunities to improve the overall architecture.

Evaluate the following:

1. Layering and Boundaries
   - Identify the project’s implicit architecture (e.g., layered, modular, vertical slice, clean architecture).
   - Determine whether responsibilities are correctly separated across layers.
   - Highlight any domain logic leaking into UI, infrastructure, or data layers.

2. Pattern Usage Across the Project
   - Identify which architectural patterns are present or missing (e.g., CQRS, Mediator, Repository, Factory, Adapter, Strategy, Observer, Pipeline).
   - Point out inconsistencies in how patterns are applied across files.
   - Recommend patterns that would improve clarity, extensibility, or testability.

3. Dependency Flow and Coupling
   - Map out dependency direction and identify cycles or inverted dependencies.
   - Highlight areas where abstractions or interfaces should exist but don’t.
   - Identify services or modules that are overloaded or acting as “god objects.”

4. Project Structure and Folder Organization
   - Evaluate whether the folder structure reflects the architecture.
   - Identify misplaced files, cross-cutting concerns, or unclear module boundaries.
   - Recommend a reorganized folder structure aligned with best practices.

5. Code Quality at Architectural Scale
   - Identify systemic issues: duplicated logic, inconsistent naming, missing contracts, unclear ownership.
   - Highlight areas where the project lacks cohesion or has accidental complexity.

6. Production-Grade Expectations
   - Identify what prevents the project from being considered production-grade.
   - Recommend structural improvements that increase maintainability, scalability, and clarity.

Return your findings as:
- A high-level architectural assessment
- A list of systemic issues and their impact
- A recommended architectural blueprint for the project
- A proposed folder and module structure