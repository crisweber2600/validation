# Agent Instructions

All code changes in this repository must follow Test-Driven Development (TDD):

1. Write or update tests to describe the desired behavior before changing implementation code. Run `dotnet test` and ensure the new tests fail.
2. Update implementation code until all tests pass.
3. Always execute `dotnet test` before committing any changes. The build and tests must succeed.

Where practical, keep each service or controller implementation under **200 lines**.
If a service or controller grows beyond this limit, consider refactoring into smaller components.

