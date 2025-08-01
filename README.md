# validation

## Nanny Records

`NannyRecord` captures the last summarised metric for an entity. The `UnitOfWork`
updates or inserts a record each time changes are saved so that validation
metrics can be audited and troubleshooting information retained.

