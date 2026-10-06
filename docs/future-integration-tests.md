# Future onboarding integration-test scenarios

These scenarios preserve the intent of the removed standalone Python smoke script.
They are deferred until a lightweight PostgreSQL integration-test mechanism is authorized;
no integration infrastructure, mocked persistence tests, or coverage targets are added now.
Use an isolated migrated PostgreSQL database and the application persistence boundary.

- Atomic onboarding persistence: completing a session persists the account, messaging account,
  all phone numbers, and Completed state together.
- Duplicate external identifier rejection: each account, messaging, and phone external ID
  remains globally unique, including opaque nonnumeric provider values.
- Rollback on conflicts: any uniqueness conflict leaves no partial account graph and does
  not complete the losing session.
- Tenant-scoped account listing: an Organization sees only its own accounts and phones.
- Idempotent callback replay: an identical callback, including normalized display names and
  reordered phone records, returns the existing graph without inserting duplicates.
- Conflicting callback rejection: a different result for a completed session returns a
  conflict and preserves the original account graph and completion time.
- Concurrent duplicate callback handling: simultaneous identical callbacks converge on one
  account graph; the winner creates it and replays return that same persisted identity.

Also validate Domain and PostgreSQL identifier constraints against the same representative
Unicode, whitespace, control-character, and length-boundary fixtures when this suite exists.
