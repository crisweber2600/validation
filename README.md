# validation

## NannyRecord

`NannyRecord` captures the last metric calculated for a saved entity. This audit
record is persisted via either EF Core or MongoDB depending on configuration.
It stores the metric value along with the running program and runtime identifier
to aid troubleshooting of validation history.