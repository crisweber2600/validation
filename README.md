# Validation

## Validation Flow Configuration

Validation flows can be registered via `AddValidationFlows` by supplying a JSON configuration. Each entry specifies which consumers to enable and any manual validation rules.

```
[
  {
    "Type": "<FullyQualifiedTypeName>",
    "SaveValidation": true,
    "SaveCommit": true,
    "DeleteValidation": true,
    "DeleteCommit": true,
    "MetricProperty": "Metric",
    "ThresholdType": 1,
    "ThresholdValue": 0.2,
    "ManualRules": [
      { "Property": "Metric", "GreaterThan": 0 }
    ]
  }
]
```

`ManualRules` allow simple property rules to be registered at startup. The example above enforces that `Metric > 0` for `Item` entities. See `config/sample-validation-flows.json` for a full example.