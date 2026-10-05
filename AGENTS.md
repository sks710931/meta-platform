# Agent rules

Hard rules for every change. Product behavior lives elsewhere. These rules are about code quality only.

## Boundaries

- One module, one reason to change. Do not add a second responsibility to an existing module.
- Dependency direction is one way: delivery → application → domain. Domain imports nothing from delivery or infrastructure.
- Modules do not import each other's internals. Share a type only through an explicit contract.
- No business rule in a controller, handler, component, or migration.
- A change touches one module, or it is labeled cross-cutting and says why. Do not refactor a neighbor in the same change.

## Files

- One use case per file. Name the file after the action.
- A file over 200 lines must be split, unless it is generated.
- A function does one thing. If the name needs "and", split it.
- No global database, client, or config read inside domain code. Pass dependencies in.
- Public functions name their inputs. No boolean flags that switch behavior. Split the function.

## Types and errors

- Strict types. No `any`. No unchecked casts.
- Parse unknown input at the edge. Domain code receives typed values.
- Expected failures return a result. Throw only for programmer errors.
- Do not swallow errors. Do not catch and return null.

## Data

- Money is integer minor units. No floats for money.
- Time is an injected clock. Do not call the system clock in domain code.
- Writes that correct history are new records. Do not mutate an audit row.
- Every tenant-scoped query filters by tenant. Add a test when you add a query.

## Tests

- Domain rules have unit tests with no network, no filesystem, and no real clock.
- Test the module through its public contract. Do not mock the unit under test.
- A bug fix includes a failing test that the fix turns green.
- Do not assert on log text or private fields.

## Diffs

- No drive-by format, rename, or dependency upgrade.
- No commented-out code. No new TODO without an owner and a reason.
- Secrets, tokens, and local env files are never committed.
- Generated code is regenerated, not hand-edited.
- If a rule in this file conflicts with a one-off shortcut, the rule wins.
