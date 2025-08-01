# validation

This repository demonstrates a validation framework with audit tracking.

`SaveAudit` records store whether an entity save was valid along with the metric evaluated.
`NannyRecord` captures the last summarised metric for an entity and environment details.
It is useful for auditing and troubleshooting validation behaviour.
