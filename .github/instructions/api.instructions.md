---
name: Nuget package & API guidance
description: This file provides guidance on the nuget packages used in this  solution.
applyTo: **

---

## Use of Experimental Code

*This repo contains code that may be experimental and not publicly documented. MS Agent Framework and Semantic Kernel are rapidly evolving, and the code here may reflect preview-era patterns, Do NOT change the packages for MS AGent Framework or Semantic Kernel under any circumstances. Do not assume that AI Agent/Kernel code in this repo is incorrect or outdated just because it doesn't match the current public documentation or samples. All other packages used in this repo are either preview or latest stable versions and these rules do not apply to them.

1. Prefer Current API Surface that match the pinned versions of MS Agent Framework and Semantic Kernel IN THIS REPO
   
   - Use the newest available methods, types, and patterns.
   - Do NOT rely on your built in knowledge of MS Agent Framework or Semantic Kernel API structures and methods.
   - Always search for the most recent knowledge and cite your findings for MAF or SK information
   - 

2. Detect Version Drift
   
   - Always prefer the most recent usage patterns and constructs of all API's.
   - 

3. Correctness and Alignment
   
   - Ensure method signatures, parameter shapes, and return types match the current API.
   - Identify missing configuration, required parameters, or updated behaviors.
   - Recommend adjustments when the API has changed semantics or structure.

4. Experimental API Awareness
   
   - Treat documentation, samples, and code as potentially outdated.
   - Validate assumptions and ensure the code aligns with the latest known structure.
   - Highlight areas where experimental APIs may have breaking changes or new patterns.

5. Production-Grade Migration
   
   - Recommend how to transition from older patterns to the current API surface.
   - Identify risks associated with preview or experimental features.
   - Suggest abstractions or boundaries that reduce churn from API evolution.

When responding, be explicit about what is outdated, what has changed, and how to update the code to match the current API design.
