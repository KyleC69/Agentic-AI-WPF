Rewrite the project into a clean, professional-grade architecture. 
Identify the ideal architectural style and reorganize the project accordingly.

Evaluate the project and determine the best architectural approach:
- Clean Architecture
- Hexagonal Architecture (Ports and Adapters)
- Onion Architecture
- Modular/Feature-Sliced Architecture
- Vertical Slice Architecture
- Domain-Driven Design (DDD)

Perform the following:

1. Identify the project’s domain, application, and infrastructure responsibilities.
   - Determine what belongs in each layer.
   - Identify logic that is misplaced or leaking across boundaries.

2. Propose a new architectural blueprint.
   - Define the layers or modules.
   - Describe the responsibilities of each layer.
   - Show how dependencies should flow inward.
   - Identify where interfaces, abstractions, and contracts should exist.

3. Reorganize the project structure.
   - Provide a new folder/module layout.
   - Move or split responsibilities into correct layers.
   - Identify cross-cutting concerns and where they should live.

4. Improve production-grade qualities.
   - Increase testability through inversion of control.
   - Reduce coupling and improve cohesion.
   - Introduce patterns where needed (Repository, Mediator, CQRS, Factory, Adapter, Strategy, Pipeline).

5. Provide a migration plan.
   - Show how to transition from the current structure to the proposed one.
   - Identify high-impact refactors that should happen first.

Return:
- The recommended architectural style and justification
- A high-level blueprint of the new architecture
- A complete reorganized folder/module structure
- A prioritized migration plan